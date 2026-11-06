namespace CarnetDeSalle.Api.Entities;

/// <summary>Séance type d'un programme : A, B, C... (table app.WorkoutTemplates).</summary>
public class WorkoutTemplate
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public short DurationMinutes { get; set; }
    public string CardioName { get; set; } = "";
    public string? CardioDetail { get; set; }
    public short CardioMinutes { get; set; }
    /// <summary>MET du cardio pour l'estimation des kcal (tapis incliné 7,0 ; vélo 6,0).</summary>
    public decimal CardioMet { get; set; }
    public byte Position { get; set; }

    public TrainingProgram Program { get; set; } = null!;
    public List<WorkoutTemplateExercise> Exercises { get; set; } = [];
}
