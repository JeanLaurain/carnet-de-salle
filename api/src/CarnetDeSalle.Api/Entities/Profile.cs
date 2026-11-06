namespace CarnetDeSalle.Api.Entities;

/// <summary>Données qui servent au calcul de la dépense et des objectifs (table app.Profiles).</summary>
public class Profile
{
    public Guid UserId { get; set; }
    public Sex Sex { get; set; } = Sex.Male;
    public DateOnly? BirthDate { get; set; }
    public short HeightCm { get; set; } = 173;
    public decimal ActivityFactor { get; set; } = 1.375m;
    public short DailyKcalOffset { get; set; } = -500;
    public decimal ProteinGPerKg { get; set; } = 1.8m;
    public decimal FatShareOfKcal { get; set; } = 0.30m;
    public short FiberTargetG { get; set; } = 30;
    public string TimeZone { get; set; } = "Europe/Paris";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
