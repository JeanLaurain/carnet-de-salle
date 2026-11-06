namespace CarnetDeSalle.Api.Entities;

/// <summary>Avis et consigne de l'analyse pour un exercice (table app.WorkoutAnalysisItems).</summary>
public class WorkoutAnalysisItem
{
    public long Id { get; set; }
    public Guid WorkoutId { get; set; }
    public int ExerciseId { get; set; }
    public string? Feedback { get; set; }
    /// <summary>Consigne pour la prochaine fois.</summary>
    public string? Instruction { get; set; }
    public decimal? SuggestedLoadKg { get; set; }
    public short? SuggestedDurationSec { get; set; }

    public WorkoutAnalysis Analysis { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
