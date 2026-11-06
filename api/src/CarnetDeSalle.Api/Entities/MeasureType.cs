namespace CarnetDeSalle.Api.Entities;

/// <summary>Comment se mesure un exercice. Stocké en minuscules : load, time, reps.</summary>
public enum MeasureType
{
    /// <summary>Charge × répétitions.</summary>
    Load,
    /// <summary>Durée en secondes (gainage).</summary>
    Time,
    /// <summary>Répétitions seules, sans charge.</summary>
    Reps,
}
