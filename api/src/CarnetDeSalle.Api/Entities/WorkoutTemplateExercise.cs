namespace CarnetDeSalle.Api.Entities;

/// <summary>Exercice prévu dans une séance type (table app.WorkoutTemplateExercises).</summary>
public class WorkoutTemplateExercise
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public int ExerciseId { get; set; }
    public byte Position { get; set; }
    public byte Sets { get; set; }
    public byte? RepsMin { get; set; }
    public byte? RepsMax { get; set; }
    public short? SecondsMin { get; set; }
    public short? SecondsMax { get; set; }
    /// <summary>Palier de charge : 2,5 ou 5 kg.</summary>
    public decimal? LoadIncrementKg { get; set; }
    /// <summary>Null : repos par défaut du programme.</summary>
    public short? RestSec { get; set; }

    public WorkoutTemplate Template { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
