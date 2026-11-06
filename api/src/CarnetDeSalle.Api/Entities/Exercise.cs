namespace CarnetDeSalle.Api.Entities;

/// <summary>Exercice du catalogue commun, ou ajouté par un utilisateur (table app.Exercises).</summary>
public class Exercise
{
    public int Id { get; set; }
    /// <summary>Clé stable (« presse », « hack_squat »...), sert aussi à choisir l'illustration.</summary>
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Muscles { get; set; } = "";
    public MeasureType MeasureType { get; set; }
    public string? Alternative { get; set; }
    public string? AlternativeLabel { get; set; }
    public string? VideoSearch { get; set; }
    public string? IllustrationKey { get; set; }
    /// <summary>Null : catalogue commun ; sinon exercice ajouté par cet utilisateur.</summary>
    public Guid? CreatedByUserId { get; set; }
    public bool IsActive { get; set; } = true;

    public AppUser? CreatedBy { get; set; }
    public List<ExerciseTip> Tips { get; set; } = [];
}
