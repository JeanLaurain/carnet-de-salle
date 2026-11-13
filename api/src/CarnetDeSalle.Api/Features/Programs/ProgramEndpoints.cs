using System.Security.Claims;
using CarnetDeSalle.Api.Common;
using CarnetDeSalle.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CarnetDeSalle.Api.Features.Programs;

public static class ProgramEndpoints
{
    /// <summary>Routes /api/program. Dans Program.cs : api.MapProgramEndpoints();</summary>
    public static RouteGroupBuilder MapProgramEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/program")
            .RequireAuthorization()
            .WithTags("Programme");

        group.MapGet("/", GetProgramAsync)
            .WithName("GetProgram");

        return group;
    }

    /// <summary>GET /api/program : programme actif de l'utilisateur connecté.</summary>
    private static async Task<Results<Ok<ProgramDto>, NotFound>> GetProgramAsync(
        ClaimsPrincipal user, AppDbContext db, DefaultProgramService defaults, TimeProvider clock, CancellationToken ct)
    {
        // 1. userId = user.GetUserId().
        var userId = user.GetUserId();
        // 2. Date du jour dans le fuseau du profil (Europe/Paris sans profil).
        var timeZone = await db.Profiles
            .Where(p => p.UserId == userId)
            .Select(p => p.TimeZone)
            .FirstOrDefaultAsync(ct);
        var today = ProgramCycle.Today(clock, timeZone);
        // 3. Crée le programme par défaut au premier appel ; le cycle commence aujourd'hui.
        await defaults.EnsureForUserAsync(userId, today, ct);
        // 4. dto = await LoadActiveProgramAsync(db, userId, today, ct).
        var dto = await LoadActiveProgramAsync(db, userId, today, ct);
        // 5. TypedResults.Ok(dto), ou TypedResults.NotFound() si null (ne devrait pas arriver après l'étape 3).
        return dto is not null ? TypedResults.Ok(dto) : TypedResults.NotFound();
    }

    /// <summary>Le programme actif projeté en ProgramDto, en une seule requête.</summary>
    private static async Task<ProgramDto?> LoadActiveProgramAsync(
        AppDbContext db, Guid userId, DateOnly today, CancellationToken ct)
    {
        // 1. db.Programs.AsNoTracking().Where(UserId == userId && IsActive).Select(p => new ProgramDto(...)).
        //    Dans le Select :
        //    - séances triées par Position, exercices triés par Position,
        //      conseils triés par Position (Tips = conseils de l'exercice, OrderBy(Position), Select(Text)) ;
        //    - RestSec = RestSec de l'exercice ?? DefaultRestSec du programme ;
        //    - ChangeNotes : celles du programme, ou une liste vide ;
        //    - Cycle : new CycleDto(p.StartsOn, p.PlannedWeeks, null, false) pour l'instant,
        //      car EF ne sait pas traduire ProgramCycle en SQL.
        var dto = await db.Programs
            .AsNoTracking()
            .AsSingleQuery()   // une requête : quelques dizaines de lignes, pas de risque d'explosion
            .Where(p => p.UserId == userId && p.IsActive)
            .Select(p => new ProgramDto(
                p.Version,
                p.Name,
                p.Goal,
                p.Frequency,
                p.Gym,
                p.DefaultRestSec,
                new WarmupDto(
                    p.WarmupTitle,
                    p.WarmupDetail,
                    p.WarmupMinutes),
                p.ChangeNotes ?? new List<string>(),
                new CycleDto(
                    p.StartsOn,
                    p.PlannedWeeks,
                    null,
                    false),
                p.Templates
                    .OrderBy(t => t.Position)
                    .Select(t => new SessionDto(
                        t.Code,
                        t.Name,
                        t.DurationMinutes,
                        t.Position,
                        new CardioDto(
                            t.CardioName,
                            t.CardioDetail,
                            t.CardioMinutes,
                            t.CardioMet),
                        t.Exercises
                            .OrderBy(e => e.Position)
                            .Select(e => new SessionExerciseDto(
                                e.Position,
                                e.Exercise.Code,
                                e.Exercise.Name,
                                e.Exercise.Muscles,
                                e.Exercise.MeasureType,
                                e.Sets,
                                e.RepsMin,
                                e.RepsMax,
                                e.SecondsMin,
                                e.SecondsMax,
                                e.LoadIncrementKg,
                                e.RestSec ?? p.DefaultRestSec,
                                e.Exercise.Tips
                                    .OrderBy(tip => tip.Position)
                                    .Select(tip => tip.Text)
                                    .ToList(),
                                e.Exercise.Alternative,
                                e.Exercise.AlternativeLabel,
                                e.Exercise.VideoSearch,
                                e.Exercise.IllustrationKey
                            ))
                            .ToList()
                    ))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

        // 3. Si trouvé : compléter le cycle après la requête,
        //    dto with { Cycle = ProgramCycle.Compute(dto.Cycle.StartsOn, dto.Cycle.PlannedWeeks, today) }.
        return dto is not null
            ? dto with { Cycle = ProgramCycle.Compute(dto.Cycle.StartsOn, dto.Cycle.PlannedWeeks, today) }
            : null;
    }
}
