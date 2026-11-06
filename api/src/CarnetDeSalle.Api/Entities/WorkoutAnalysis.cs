namespace CarnetDeSalle.Api.Entities;

/// <summary>Analyse d'une séance, par Claude ou saisie à la main (table app.WorkoutAnalyses).</summary>
public class WorkoutAnalysis
{
    public Guid WorkoutId { get; set; }
    public AnalysisSource Source { get; set; }
    public string? Title { get; set; }
    /// <summary>Bilan.</summary>
    public string? Summary { get; set; }
    /// <summary>Conseil prioritaire.</summary>
    public string? Advice { get; set; }
    /// <summary>Modèle Claude utilisé.</summary>
    public string? Model { get; set; }
    public DateTime CreatedAt { get; set; }

    public Workout Workout { get; set; } = null!;
    public List<WorkoutAnalysisItem> Items { get; set; } = [];
}
