using CarnetDeSalle.Api.Entities;
using CarnetDeSalle.Api.Features.WeighIns;
using CarnetDeSalle.Api.Features.Workouts;

namespace CarnetDeSalle.Api.Tests.Workouts;

/// <summary>Kcal brûlées pendant une séance (regles-metier.md §3) : tests unitaires, sans base ni serveur.</summary>
public class WorkoutEnergyCalculatorTests
{
    // Séance réalisée

    [Fact]
    public void Exemple_de_la_doc_250_kcal()
    {
        // 80 kg, 51 min au total : la musculation est plafonnée à 51 - 8 - 15 = 28 min
        var result = WorkoutEnergyCalculator.Estimate(Input(80m, SessionASets(), durationMinutes: 51));

        Assert.Equal(93, result.StrengthKcal);
        Assert.Equal(32, result.WarmupKcal);
        Assert.Equal(120, result.CardioKcal);
        Assert.Equal(250, result.TotalKcal);
    }

    [Fact]
    public void Seance_A_du_4_novembre_240_kcal()
    {
        // Le poids vient des vraies pesées : 78,5 kg le 30 octobre, dernière pesée avant le 4 novembre
        var weightKg = WeightHistory.WeightOn(
            [WeighIn("2026-10-25", 79m), WeighIn("2026-10-30", 78.5m), WeighIn("2026-11-06", 78.4m)],
            new DateOnly(2026, 11, 4));

        var result = WorkoutEnergyCalculator.Estimate(Input(weightKg, SessionASets(), durationMinutes: 51));

        Assert.Equal(78.5m, weightKg);
        Assert.Equal(92, result.StrengthKcal);
        Assert.Equal(31, result.WarmupKcal);
        Assert.Equal(118, result.CardioKcal);
        Assert.Equal(240, result.TotalKcal);
    }

    [Fact]
    public void Duree_sous_20_minutes_pas_de_plafond()
    {
        // Séance B du 12 novembre, durée saisie « 1 min » : ignorée, les séries comptent 31 min
        var result = WorkoutEnergyCalculator.Estimate(
            Input(78.4m, SessionBSets(), cardioMet: 6m, durationMinutes: 1));

        Assert.Equal(101, result.StrengthKcal);
        Assert.Equal(230, result.TotalKcal);
    }

    [Fact]
    public void Le_plafond_ne_rallonge_pas_la_seance()
    {
        // Séance B du 29 octobre, 83 min : 60 min disponibles, mais les séries n'en font que 31
        var result = WorkoutEnergyCalculator.Estimate(
            Input(79m, SessionBSets(), cardioMet: 6m, durationMinutes: 83));

        Assert.Equal(102, result.StrengthKcal);
        Assert.Equal(230, result.TotalKcal);
    }

    [Fact]
    public void Sous_le_plafond_les_minutes_gardent_leurs_decimales()
    {
        // Séance A avec 83 min : pas de plafond, 1 899 s = 31,65 min (et non 31)
        var result = WorkoutEnergyCalculator.Estimate(Input(78.5m, SessionASets(), durationMinutes: 83));

        Assert.Equal(104, result.StrengthKcal);
    }

    [Fact]
    public void Plafond_jamais_sous_10_minutes()
    {
        // 25 - 8 - 15 = 2 min disponibles, remontées à 10 min
        var result = WorkoutEnergyCalculator.Estimate(Input(80m, SessionASets(), durationMinutes: 25));

        Assert.Equal(33, result.StrengthKcal);   // 2,5 × 80 kg × 10 min / 60
    }

    [Fact]
    public void Le_plancher_ne_gonfle_pas_une_seance_courte()
    {
        // 2 séries = 260 s = 4,33 min : sous le plancher de 10 min, on garde 4,33
        SetEffort[] sets = [new(null, 90), new(null, 90)];

        var result = WorkoutEnergyCalculator.Estimate(Input(80m, sets, durationMinutes: 25));

        Assert.Equal(14, result.StrengthKcal);
    }

    [Fact]
    public void Sans_echauffement_le_temps_disponible_ne_retire_que_le_cardio()
    {
        // 40 - 15 = 25 min disponibles (et non 40 - 8 - 15)
        var result = WorkoutEnergyCalculator.Estimate(
            Input(80m, SessionASets(), warmupDone: false, durationMinutes: 40));

        Assert.Equal(83, result.StrengthKcal);
        Assert.Equal(0, result.WarmupKcal);
    }

    [Fact]
    public void Sans_echauffement_ni_cardio_ces_parts_valent_0()
    {
        var result = WorkoutEnergyCalculator.Estimate(
            Input(80m, SessionASets(), warmupDone: false, cardioMinutes: null));

        Assert.Equal(0, result.WarmupKcal);
        Assert.Equal(0, result.CardioKcal);
        Assert.Equal(106, result.StrengthKcal);   // 105,5 arrondi loin de zéro
        Assert.Equal(110, result.TotalKcal);
    }

    [Fact]
    public void Serie_chronometree_sans_duree_compte_40_secondes()
    {
        var withoutDuration = WorkoutEnergyCalculator.Estimate(Input(80m, [new(null, 60)]));
        var with40Seconds = WorkoutEnergyCalculator.Estimate(Input(80m, [new(40, 60)]));

        Assert.Equal(with40Seconds, withoutDuration);
    }

    [Fact]
    public void Arrondi_245_donne_250()
    {
        // Cardio seul : (7 - 1) × 98 kg × 25 min / 60 = 245 kcal pile.
        // L'arrondi par défaut de C# (bancaire) donnerait 240.
        var result = WorkoutEnergyCalculator.Estimate(
            Input(98m, [], warmupDone: false, cardioMinutes: 25));

        Assert.Equal(245, result.CardioKcal);
        Assert.Equal(250, result.TotalKcal);
    }

    // Séance prévue (séance type du programme par défaut, 78,3 kg)

    [Fact]
    public void Prevue_seance_A_250_kcal()
    {
        // Presse, développé couché, tirage, leg curl : 3 séries, repos par défaut (90 s).
        // Gainage : 3 séries de 40 s au plus, 60 s de repos. Tapis incliné : 15 min, MET 7.
        var template = Template(15, 7m,
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, null),
            (MeasureType.Time, 3, 40, 60));

        var result = WorkoutEnergyCalculator.EstimatePlanned(template, 90, 78.3m);

        Assert.Equal(101, result.StrengthKcal);   // 31 min : le repos du gainage compte
        Assert.Equal(31, result.WarmupKcal);
        Assert.Equal(117, result.CardioKcal);
        Assert.Equal(250, result.TotalKcal);
    }

    [Fact]
    public void Prevue_seance_B_230_kcal()
    {
        // Hack squat, rowing, développé épaules, hip thrust : repos par défaut.
        // Crunch : 60 s de repos. Vélo : 15 min, MET 6.
        var template = Template(15, 6m,
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, null),
            (MeasureType.Load, 3, null, 60));

        var result = WorkoutEnergyCalculator.EstimatePlanned(template, 90, 78.3m);

        Assert.Equal(101, result.StrengthKcal);
        Assert.Equal(98, result.CardioKcal);
        Assert.Equal(230, result.TotalKcal);
    }

    // Données de test

    /// <summary>Séance A du 4 novembre : 12 séries à charge (repos 90 s), gainage 52, 53 et 54 s (repos 60 s).</summary>
    private static List<SetEffort> SessionASets() =>
        [.. Enumerable.Repeat(new SetEffort(null, 90), 12), new(52, 60), new(53, 60), new(54, 60)];

    /// <summary>Séance B : 12 séries à charge (repos 90 s), 3 séries de crunch (repos 60 s).</summary>
    private static List<SetEffort> SessionBSets() =>
        [.. Enumerable.Repeat(new SetEffort(null, 90), 12), .. Enumerable.Repeat(new SetEffort(null, 60), 3)];

    /// <summary>Par défaut : échauffement fait, 15 min de tapis incliné (MET 7), durée totale inconnue.</summary>
    private static WorkoutEnergyInput Input(
        decimal weightKg,
        IReadOnlyList<SetEffort> sets,
        bool warmupDone = true,
        short? cardioMinutes = 15,
        decimal cardioMet = 7m,
        short? durationMinutes = null) =>
        new(weightKg, warmupDone, cardioMinutes, cardioMet, durationMinutes, sets);

    /// <summary>Séance type en mémoire, avec l'exercice du catalogue de chaque ligne (pour son MeasureType).</summary>
    private static WorkoutTemplate Template(
        short cardioMinutes,
        decimal cardioMet,
        params (MeasureType Type, byte Sets, short? SecondsMax, short? RestSec)[] exercises) =>
        new()
        {
            CardioMinutes = cardioMinutes,
            CardioMet = cardioMet,
            Exercises = [.. exercises.Select((e, i) => new WorkoutTemplateExercise
            {
                Position = (byte)(i + 1),
                Sets = e.Sets,
                SecondsMax = e.SecondsMax,
                RestSec = e.RestSec,
                Exercise = new Exercise { MeasureType = e.Type },
            })],
        };

    private static WeighIn WeighIn(string date, decimal weightKg) =>
        new() { Date = DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture), WeightKg = weightKg };
}
