using CarnetDeSalle.Api.Entities;

namespace CarnetDeSalle.Api.Features.Programs;

/// <summary>Programme actif renvoyé par GET /api/program.</summary>
public sealed record ProgramDto(
    int Version,
    string Name,
    string Goal,
    string Frequency,
    string? Gym,
    short DefaultRestSec,
    WarmupDto Warmup,
    IReadOnlyList<string> ChangeNotes,
    CycleDto Cycle,
    IReadOnlyList<SessionDto> Sessions);

/// <summary>Échauffement affiché en haut de la séance.</summary>
public sealed record WarmupDto(
    string Title,
    string? Detail,
    byte Minutes);

/// <summary>Cycle d'entraînement : « semaine 5 sur 8 ».</summary>
/// <param name="CurrentWeek">Null si le programme n'a pas de date de début.</param>
/// <param name="IsOver">Vrai au-delà des semaines prévues : rappel « change quelques machines ».</param>
public sealed record CycleDto(
    DateOnly? StartsOn,
    byte? PlannedWeeks,
    int? CurrentWeek,
    bool IsOver);

/// <summary>Séance type (A, B...).</summary>
public sealed record SessionDto(
    string Code,
    string Name,
    short DurationMinutes,
    byte Position,
    CardioDto Cardio,
    IReadOnlyList<SessionExerciseDto> Exercises);

/// <summary>Cardio de fin de séance. Met sert au calcul des kcal (#16).</summary>
public sealed record CardioDto(
    string Name,
    string? Detail,
    short Minutes,
    decimal Met);

/// <summary>Exercice d'une séance type, avec sa cible et de quoi le faire bien.</summary>
/// <param name="Code">Code de l'exercice ; sert aussi à choisir l'illustration si IllustrationKey est vide.</param>
/// <param name="RestSec">Repos effectif : celui de l'exercice, sinon celui du programme.</param>
public sealed record SessionExerciseDto(
    byte Position,
    string Code,
    string Name,
    string Muscles,
    MeasureType MeasureType,
    byte Sets,
    byte? RepsMin,
    byte? RepsMax,
    short? SecondsMin,
    short? SecondsMax,
    decimal? LoadIncrementKg,
    short RestSec,
    IReadOnlyList<string> Tips,
    string? Alternative,
    string? AlternativeLabel,
    string? VideoSearch,
    string? IllustrationKey);
