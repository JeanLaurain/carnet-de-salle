namespace CarnetDeSalle.Api.Entities;

/// <summary>Comment un repas a été saisi. Stocké en minuscules : claude, photo, manual, recent.</summary>
public enum MealSource
{
    Claude,
    Photo,
    Manual,
    Recent,
}
