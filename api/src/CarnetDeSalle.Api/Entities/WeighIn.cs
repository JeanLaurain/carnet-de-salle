namespace CarnetDeSalle.Api.Entities;

/// <summary>Pesée, une par jour au plus (table app.WeighIns).</summary>
public class WeighIn
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public decimal WeightKg { get; set; }
    /// <summary>Tour de taille au nombril, facultatif.</summary>
    public decimal? WaistCm { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
