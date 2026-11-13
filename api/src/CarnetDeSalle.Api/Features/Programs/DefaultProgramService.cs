using CarnetDeSalle.Api.Data;
using CarnetDeSalle.Api.Data.Seed;
using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using static CarnetDeSalle.Api.Data.Seed.ProgramSeed;

namespace CarnetDeSalle.Api.Features.Programs;

/// <summary>Donne le programme de programme.json à un utilisateur qui n'a pas encore de programme.</summary>
public sealed class DefaultProgramService(AppDbContext db)
{
    /// <summary>
    /// Crée le programme par défaut si l'utilisateur n'a pas de programme actif.
    /// Ne fait rien s'il en a déjà un (même modifié). Renvoie vrai si un programme a été créé.
    /// </summary>
    /// <param name="startsOn">Début du cycle : la date du jour dans le fuseau de l'utilisateur.</param>
    public async Task<bool> EnsureForUserAsync(Guid userId, DateOnly startsOn, CancellationToken ct = default)
    {
        // 1. L'utilisateur a déjà un programme actif : rien à faire.
        if (await HasActiveProgramAsync(userId, ct))
        {
            return false;
        }

        // 2. Lire le fichier.
        var seed = await ReadSeedAsync(ct);

        // 3. Id du catalogue par code, pour tous les exercices des séances.
        var codes = seed.Sessions
            .SelectMany(s => s.Exercises)
            .Select(ex => ex.ExerciseCode)
            .Distinct()
            .ToList();
        var exerciseIds = await db.Exercises
            .Where(e => e.CreatedByUserId == null && codes.Contains(e.Code))
            .ToDictionaryAsync(e => e.Code, e => e.Id, ct);

        //    Un code absent du catalogue = fichier incohérent, ou catalogue pas encore chargé (--migrate).
        var missing = codes.Where(c => !exerciseIds.ContainsKey(c)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Exercices absents du catalogue : {string.Join(", ", missing)}. Lancer ExerciseCatalogSeeder (--migrate).");
        }

        // 4. Programme, séances et exercices sont enregistrés ensemble, ou pas du tout.
        db.Programs.Add(BuildProgram(userId, seed, exerciseIds, startsOn));

        // 5. Deux premiers appels en même temps : l'index UX_Programs_OneActivePerUser fait échouer le second.
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // Oublie tout ce qui a été ajouté (programme, séances, exercices), pas seulement le programme
            db.ChangeTracker.Clear();

            // Un programme actif existe maintenant : l'autre appel l'a créé
            if (await HasActiveProgramAsync(userId, ct))
            {
                return false;
            }

            // Sinon ce n'est pas la collision attendue
            throw;
        }
    }

    private Task<bool> HasActiveProgramAsync(Guid userId, CancellationToken ct) =>
        db.Programs.AnyAsync(p => p.UserId == userId && p.IsActive, ct);

    /// <summary>Entités du programme, sans rien enregistrer.</summary>
    private static TrainingProgram BuildProgram(
        Guid userId, ProgramSeed seed, IReadOnlyDictionary<string, int> exerciseIds, DateOnly startsOn)
    => new()
    {
        UserId = userId,
        Version = seed.Version,
        Name = seed.Name,
        Goal = seed.Goal,
        Frequency = seed.Frequency,
        Gym = seed.Gym,
        DefaultRestSec = seed.DefaultRestSec,
        WarmupTitle = seed.Warmup.Title,
        WarmupDetail = seed.Warmup.Detail,
        WarmupMinutes = seed.Warmup.Minutes,
        ChangeNotes = [.. seed.ChangeNotes],
        StartsOn = startsOn,
        PlannedWeeks = seed.PlannedWeeks,
        IsActive = true,
        Templates = [.. seed.Sessions.Select(s => BuildTemplate(s, exerciseIds))],
    };

    /// <summary>Une séance type et ses exercices.</summary>
    private static WorkoutTemplate BuildTemplate(SessionSeed session, IReadOnlyDictionary<string, int> exerciseIds)
    => new()
    {
        Code = session.Code,
        Name = session.Name,
        DurationMinutes = session.DurationMinutes,
        Position = session.Position,
        CardioName = session.Cardio.Name,
        CardioDetail = session.Cardio.Detail,
        CardioMinutes = session.Cardio.Minutes,
        CardioMet = session.Cardio.Met,
        Exercises = [.. session.Exercises.Select(ex => new WorkoutTemplateExercise
        {
            ExerciseId = exerciseIds[ex.ExerciseCode],
            Position = ex.Position,
            Sets = ex.Sets,
            RepsMin = ex.RepsMin,
            RepsMax = ex.RepsMax,
            SecondsMin = ex.SecondsMin,
            SecondsMax = ex.SecondsMax,
            LoadIncrementKg = ex.LoadIncrementKg,
            RestSec = ex.RestSec,
        })],
    };
}
