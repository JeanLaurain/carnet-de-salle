namespace CarnetDeSalle.Api.Entities;

/// <summary>Une série : charge et répétitions, ou durée (table app.WorkoutSets).</summary>
public class WorkoutSet
{
    public long Id { get; set; }
    public long WorkoutExerciseId { get; set; }
    public byte SetNumber { get; set; }
    public decimal? LoadKg { get; set; }
    public short? Reps { get; set; }
    public short? DurationSec { get; set; }

    public WorkoutExercise WorkoutExercise { get; set; } = null!;
}
