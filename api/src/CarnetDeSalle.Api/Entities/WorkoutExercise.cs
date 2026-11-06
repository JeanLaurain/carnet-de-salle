namespace CarnetDeSalle.Api.Entities;

/// <summary>Exercice fait pendant une séance (table app.WorkoutExercises).</summary>
public class WorkoutExercise
{
    public long Id { get; set; }
    public Guid WorkoutId { get; set; }
    public int ExerciseId { get; set; }
    public byte Position { get; set; }
    /// <summary>Copie du nom au moment de la séance.</summary>
    public string ExerciseName { get; set; } = "";
    public MeasureType MeasureType { get; set; }

    public Workout Workout { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public List<WorkoutSet> Sets { get; set; } = [];
}
