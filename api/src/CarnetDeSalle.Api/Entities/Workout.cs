namespace CarnetDeSalle.Api.Entities;

/// <summary>Séance réalisée (table app.Workouts). L'Id est généré par le front (mode hors ligne).</summary>
public class Workout
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int? ProgramId { get; set; }
    public int? WorkoutTemplateId { get; set; }
    /// <summary>Copie du code de la séance type (« A ») au moment de la séance.</summary>
    public string TemplateCode { get; set; } = "";
    public int ProgramVersion { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public short? DurationMinutes { get; set; }
    public bool WarmupDone { get; set; }
    public short? CardioMinutes { get; set; }
    /// <summary>Ressenti écrit.</summary>
    public string? Note { get; set; }
    /// <summary>Kcal brûlées, nettes du repos (recalculables).</summary>
    public short? EstimatedKcal { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public TrainingProgram? Program { get; set; }
    public WorkoutTemplate? Template { get; set; }
    public List<WorkoutExercise> Exercises { get; set; } = [];
    public WorkoutAnalysis? Analysis { get; set; }
}
