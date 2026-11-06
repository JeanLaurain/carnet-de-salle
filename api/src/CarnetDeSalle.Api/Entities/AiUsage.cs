namespace CarnetDeSalle.Api.Entities;

/// <summary>Un appel à l'API Claude, pour suivre le coût (table app.AiUsage).</summary>
public class AiUsage
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    /// <summary>meal-estimate, workout-analysis...</summary>
    public string Feature { get; set; } = "";
    public string Model { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public bool Succeeded { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
