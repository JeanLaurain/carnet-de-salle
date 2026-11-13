using System.Text.Json;
using System.Text.Json.Serialization;
using CarnetDeSalle.Api.Entities;
using static CarnetDeSalle.Api.Data.Seed.ProgramSeed;   // WarmupSeed... utilisables dans la liste des paramètres ci-dessous

namespace CarnetDeSalle.Api.Data.Seed;

public sealed record ProgramSeed(
        int Version,
        DateOnly Date,
        string Name,
        string Goal,
        string Frequency,
        string? Gym,
        short DefaultRestSec,
        byte? PlannedWeeks,
        WarmupSeed Warmup,
        IReadOnlyList<string> ChangeNotes,
        IReadOnlyList<SessionSeed> Sessions,
        IReadOnlyList<CatalogExerciseSeed> Exercises)
{
    /// <summary>Échauffement du programme.</summary>
    public sealed record WarmupSeed(
        string Title,
        string? Detail,
        byte Minutes,
        byte EstimatedMinutes,
        decimal Met);

    /// <summary>Séance type (A, B...) → WorkoutTemplate.</summary>
    public sealed record SessionSeed(
        string Code,
        string Name,
        short DurationMinutes,
        byte Position,
        CardioSeed Cardio,
        IReadOnlyList<SessionExerciseSeed> Exercises);

    /// <summary>Cardio de fin de séance.</summary>
    public sealed record CardioSeed(
        string Name,
        string? Detail,
        short Minutes,
        decimal Met);

    /// <summary>Place d'un exercice dans une séance → WorkoutTemplateExercise.</summary>
    public sealed record SessionExerciseSeed(
        byte Position,
        string ExerciseCode,
        byte Sets,
        byte? RepsMin,
        byte? RepsMax,
        short? SecondsMin,
        short? SecondsMax,
        decimal? LoadIncrementKg,
        short? RestSec);

    /// <summary>Exercice du catalogue commun → Exercise + ExerciseTips.</summary>
    public sealed record CatalogExerciseSeed(
        string Code,
        string Name,
        string Muscles,
        MeasureType MeasureType,
        IReadOnlyList<string> Tips,
        string? Alternative,
        string? AlternativeLabel,
        string? VideoSearch,
        string? IllustrationKey);

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },   // "load" → MeasureType.Load
    };

    public static async Task<ProgramSeed> ReadSeedAsync(CancellationToken ct)
    {
        await using var stream = typeof(ProgramSeed).Assembly.GetManifestResourceStream("programme.json")
            ?? throw new InvalidOperationException("Resource 'programme.json' not found.");
        return await JsonSerializer.DeserializeAsync<ProgramSeed>(stream, Options, ct)
            ?? throw new InvalidOperationException("Resource 'programme.json' is empty or invalid.");
    }
}
