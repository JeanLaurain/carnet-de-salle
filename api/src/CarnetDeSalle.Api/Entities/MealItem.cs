namespace CarnetDeSalle.Api.Entities;

/// <summary>Élément du détail d'un repas, pour 1 portion (table app.MealItems).</summary>
public class MealItem
{
    public long Id { get; set; }
    public Guid MealId { get; set; }
    public byte Position { get; set; }
    public string Name { get; set; } = "";
    /// <summary>« 200 g ».</summary>
    public string? Quantity { get; set; }
    public short? Kcal { get; set; }

    public Meal Meal { get; set; } = null!;
}
