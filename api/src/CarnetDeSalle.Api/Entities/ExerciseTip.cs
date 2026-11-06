namespace CarnetDeSalle.Api.Entities;

/// <summary>Conseil « Comment faire » d'un exercice (table app.ExerciseTips).</summary>
public class ExerciseTip
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public byte Position { get; set; }
    public string Text { get; set; } = "";

    public Exercise Exercise { get; set; } = null!;
}
