using System.Formats.Tar;
using System.Runtime.InteropServices;
using CarnetDeSalle.Api.Entities;

namespace CarnetDeSalle.Api.Features.Workouts;

public static class WorkoutEnergyCalculator
{
    private const decimal StrengthMet = 3.5m;
    private const decimal WarmupMet = 4;
    private const int WarmupMinutes = 8;
    private const int EffortSecPerSet = 40;
    private const int MinDurationForCap = 20;
    private const int MinStrengthMinutes = 10;

    /// <summary>
    /// Estime la dépense énergétique nette d’une séance en calculant séparément la musculation, l’échauffement et le
    /// cardio, puis le total, avec arrondi à la dizaine.
    /// </summary>
    /// <remarks>Si l’échauffement n’est pas effectué, sa contribution est nulle. Si la durée de cardio est
    /// absente, elle est traitée comme 0.</remarks>
    /// <param name="input">Données de la séance utilisées pour le calcul, notamment le poids, l’échauffement, le cardio et les valeurs MET
    /// associées.</param>
    /// <returns>Un résultat contenant les calories estimées pour la musculation, l’échauffement, le cardio et le total.</returns>
    public static WorkoutEnergyResult Estimate(WorkoutEnergyInput input)
    {
        // 1. Minutes de musculation : StrengthMinutes(input)
        var strengthMinutes = StrengthMinutes(input);
        // 2. Minutes d'échauffement : WarmupMinutes si WarmupDone, sinon 0
        var warmupMinutes = input.WarmupDone ? WarmupMinutes : 0;
        // 3. Minutes de cardio : CardioMinutes, ou 0 si null
        var cardioMinutes = input.CardioMinutes.GetValueOrDefault(0);
        // 4. Trois NetKcal : musculation (StrengthMet), échauffement (WarmupMet), cardio (input.CardioMet)
        var netStrengthKcal = NetKcal(StrengthMet, input.WeightKg, strengthMinutes);
        var netWarmupKcal = NetKcal(WarmupMet, input.WeightKg, warmupMinutes);
        var netCardioKcal = NetKcal(input.CardioMet, input.WeightKg, cardioMinutes);
        // 5. Total = RoundToTen(somme exacte des trois) ; chaque part arrondie à l'unité pour le résultat
        return new WorkoutEnergyResult(
            StrengthKcal: netStrengthKcal.RoundToUnit(),
            WarmupKcal: netWarmupKcal.RoundToUnit(),
            CardioKcal: netCardioKcal.RoundToUnit(),
            TotalKcal: (netStrengthKcal + netWarmupKcal + netCardioKcal).RoundToTen());
    }

    /// <summary>
    /// Estime la dépense énergétique d’un entraînement planifié à partir d’un modèle.
    /// </summary>
    /// <remarks>L’échauffement est considéré comme effectué et aucune durée totale maximale n’est appliquée à
    /// la séance.</remarks>
    /// <param name="template">Modèle d’entraînement contenant les informations de cardio et les éléments nécessaires à la planification des
    /// séries.</param>
    /// <param name="defaultRestSec">Durée de repos par défaut, en secondes, appliquée lors de la génération des séries planifiées.</param>
    /// <param name="weightKg">Poids corporel, en kilogrammes, utilisé pour le calcul énergétique.</param>
    /// <returns>Résultat de l’estimation énergétique pour la séance planifiée.</returns>
    public static WorkoutEnergyResult EstimatePlanned(WorkoutTemplate template, short defaultRestSec, decimal weightKg)
    {
        // 1. Les séries : PlannedSets(template, defaultRestSec)
        var sets = PlannedSets(template, defaultRestSec);
        // 2. L'entrée : échauffement fait, cardio = template.CardioMinutes et template.CardioMet,
        //    durée totale inconnue (null : pas de plafond)
        var input = new WorkoutEnergyInput(
            WeightKg: weightKg,
            WarmupDone: true,
            CardioMinutes: template.CardioMinutes,
            CardioMet: template.CardioMet,
            DurationMinutes: null,
            Sets: sets);
        // 3. return Estimate(entrée)
        return Estimate(input);
    }

    /// Séries prévues : Sets fois par exercice, SecondsMax si l'exercice est en temps, sinon null,
    /// repos = RestSec ?? defaultRestSec. Demande template.Exercises[...].Exercise chargé (MeasureType).
    private static IReadOnlyList<SetEffort> PlannedSets(WorkoutTemplate template, short defaultRestSec)
    {
        var sets = new List<SetEffort>();

        foreach (var exercise in template.Exercises)
        {
            var measureType = exercise.Exercise.MeasureType;
            var durationSec = measureType == MeasureType.Time ? exercise.SecondsMax : null;
            var restSec = exercise.RestSec ?? defaultRestSec;
            for (var i = 0; i < exercise.Sets; i++)
                sets.Add(new SetEffort(durationSec, restSec));
        }

        return sets;
    }

    /// <summary>
    /// Calcule les minutes de musculation à partir des séries, puis applique un plafonnement selon le temps total
    /// disponible de la séance.
    /// </summary>
    /// <remarks>Les durées sont exprimées en minutes. Pour chaque série, la durée explicite est utilisée si
    /// présente ; sinon une durée d’effort par défaut additionnée au repos est utilisée.</remarks>
    /// <param name="input">Données de séance utilisées pour calculer la durée des séries et le temps disponible après cardio et
    /// échauffement.</param>
    /// <returns>Le total des séries en minutes si la durée de séance est absente ou inférieure au seuil de plafonnement ; sinon
    /// la plus petite valeur entre ce total et une borne supérieure calculée à partir du temps disponible, avec une
    /// borne inférieure minimale.</returns>
    private static decimal StrengthMinutes(WorkoutEnergyInput input)
    {
        var seriesMinutes = (decimal)input.Sets.Sum(s => (s.DurationSec ?? EffortSecPerSet) + s.RestSec) / 60;

        if (input.DurationMinutes is null || input.DurationMinutes < MinDurationForCap)
            return seriesMinutes;

        var availableMinutes = (decimal)input.DurationMinutes - input.CardioMinutes.GetValueOrDefault(0);

        if (input.WarmupDone)
            availableMinutes -= WarmupMinutes;

        return Math.Min(seriesMinutes, Math.Max(MinStrengthMinutes, availableMinutes));
    }

    /// <summary>
    /// Calcule les kilocalories nettes dépensées pour une activité sur une durée donnée.
    /// </summary>
    /// <remarks>Applique la formule (MET - 1) × poids (kg) × durée (min) ÷ 60.</remarks>
    /// <param name="met">Équivalent métabolique (MET) de l’activité.</param>
    /// <param name="weightKg">Poids corporel en kilogrammes.</param>
    /// <param name="minutes">Durée de l’activité en minutes.</param>
    /// <returns>Kilocalories nettes dépensées, hors dépense au repos.</returns>
    private static decimal NetKcal(decimal met, decimal weightKg, decimal minutes) =>
        (met - 1) * weightKg * minutes / 60;

    /// <summary>
    /// Arrondit une valeur en kilocalories à la dizaine la plus proche en appliquant un arrondi à l’écart de zéro.
    /// </summary>
    /// <param name="kcal">Valeur en kilocalories à arrondir.</param>
    /// <returns>Valeur arrondie à la dizaine la plus proche, sous forme d’entier.</returns>
    private static int RoundToTen(this decimal kcal) =>
        (int)Math.Round(kcal / 10, MidpointRounding.AwayFromZero) * 10;

    /// <summary>
    /// Arrondit une valeur décimale à l’unité la plus proche en éloignant les valeurs médianes de zéro.
    /// </summary>
    /// <remarks>Utilise MidpointRounding.AwayFromZero pour les valeurs se terminant par ,5. Une exception de
    /// dépassement peut se produire si la valeur arrondie est hors de la plage de Int32.</remarks>
    /// <param name="around">Valeur décimale à arrondir.</param>
    /// <returns>Valeur entière obtenue après arrondi à l’unité.</returns>
    private static int RoundToUnit(this decimal around) =>
        (int)Math.Round(around, MidpointRounding.AwayFromZero);
}
