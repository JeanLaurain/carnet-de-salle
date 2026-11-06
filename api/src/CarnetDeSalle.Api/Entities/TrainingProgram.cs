namespace CarnetDeSalle.Api.Entities;

/// <summary>
/// Une version du programme d'un utilisateur (table app.Programs).
/// Nommée TrainingProgram car « Program » est déjà la classe de Program.cs.
/// </summary>
public class TrainingProgram
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = "";
    public string Goal { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string? Gym { get; set; }
    public short DefaultRestSec { get; set; } = 90;
    public string WarmupTitle { get; set; } = "";
    public string? WarmupDetail { get; set; }
    public byte WarmupMinutes { get; set; }
    /// <summary>Notes de version, stockées en JSON : ["Séance B : hack squat remplacé par goblet squat"].</summary>
    public List<string>? ChangeNotes { get; set; }
    public DateOnly? StartsOn { get; set; }
    public byte? PlannedWeeks { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public List<WorkoutTemplate> Templates { get; set; } = [];
}
