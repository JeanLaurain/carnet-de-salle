namespace CarnetDeSalle.Api.Features.Programs;

/// <summary>Règles du cycle d'entraînement (regles-metier.md §8). Fonctions pures, testées en unitaire.</summary>
public static class ProgramCycle
{
    public const string DefaultTimeZone = "Europe/Paris";

    /// <summary>Date du jour pour l'utilisateur, dans son fuseau (Europe/Paris par défaut).</summary>
    public static DateOnly Today(TimeProvider clock, string? timeZoneId)
    {
        // 1. Le fuseau de l'utilisateur, ou Europe/Paris s'il n'en a pas (pas encore de profil)
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZone : timeZoneId);
        // 2. L'instant présent converti dans ce fuseau, dont on ne garde que la date
        var local = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), timeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    /// <summary>« Semaine 5 sur 8 » à partir du début du cycle et de la date du jour.</summary>
    public static CycleDto Compute(DateOnly? startsOn, byte? plannedWeeks, DateOnly today)
    {
        // 1. Pas de date de début, ou début dans le futur : CurrentWeek = null, IsOver = false.
        if (!startsOn.HasValue || startsOn.Value > today)
        {
            return new CycleDto(startsOn, plannedWeeks, null, false);
        }
        // 2. Jours écoulés = today.DayNumber - startsOn.DayNumber.
        var daysElapsed = today.DayNumber - startsOn.Value.DayNumber;
        // 3. Semaine = jours écoulés / 7 + 1 (division entière) : le jour du début et les 6 suivants = semaine 1.
        var currentWeek = daysElapsed / 7 + 1;
        // 4. IsOver = plannedWeeks renseigné et semaine > plannedWeeks.
        var isOver = plannedWeeks.HasValue && currentWeek > plannedWeeks.Value;
        //    On renvoie la vraie semaine même au-delà (« semaine 10 ») ; le front affiche le rappel.
        return new CycleDto(startsOn, plannedWeeks, currentWeek, isOver);
    }
}
