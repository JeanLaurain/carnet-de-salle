namespace CarnetDeSalle.Api.Entities;

/// <summary>Repas : total mangé, portions comprises (table app.Meals).</summary>
public class Meal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public MealType MealType { get; set; }
    public string Name { get; set; } = "";
    /// <summary>Texte saisi par l'utilisateur.</summary>
    public string? Description { get; set; }
    /// <summary>« 1 assiette d'environ 350 g » (pour 1 portion).</summary>
    public string? PortionText { get; set; }
    public decimal Servings { get; set; } = 1m;
    public short Kcal { get; set; }
    public decimal ProteinG { get; set; }
    public decimal? CarbsG { get; set; }
    public decimal? FatG { get; set; }
    /// <summary>Null : non estimé (différent de 0).</summary>
    public decimal? FiberG { get; set; }
    public EstimateConfidence? Confidence { get; set; }
    public MealSource Source { get; set; }
    public string? PhotoPath { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public List<MealItem> Items { get; set; } = [];
}
