using System.Globalization;
using CarnetDeSalle.Api.Entities;
using CarnetDeSalle.Api.Features.WeighIns;

namespace CarnetDeSalle.Api.Tests.WeighIns;

/// <summary>Poids retenu pour une date (regles-metier.md §3) : tests unitaires, sans base.</summary>
public class WeightHistoryTests
{
    // Tes pesées d'octobre et novembre
    private static readonly WeighIn[] WeighIns =
        [WeighIn("2026-10-25", 79m), WeighIn("2026-10-30", 78.5m), WeighIn("2026-11-06", 78.4m)];

    [Fact]
    public void Derniere_pesee_avant_la_date()
    {
        Assert.Equal(78.5m, WeightHistory.WeightOn(WeighIns, Date("2026-11-04")));
    }

    [Fact]
    public void Pesee_du_jour_meme_comptee()
    {
        Assert.Equal(78.5m, WeightHistory.WeightOn(WeighIns, Date("2026-10-30")));
    }

    [Fact]
    public void Avant_toute_pesee_la_premiere()
    {
        Assert.Equal(79m, WeightHistory.WeightOn(WeighIns, Date("2026-10-20")));
    }

    [Fact]
    public void Sans_pesee_80_kg()
    {
        Assert.Equal(80m, WeightHistory.WeightOn([], Date("2026-11-04")));
    }

    [Theory]
    [InlineData("2026-10-20", "79")]     // avant toute pesée : la plus ancienne, pas la première de la liste
    [InlineData("2026-11-04", "78.5")]   // la plus récente avant la date
    [InlineData("2026-11-12", "78.4")]   // après toutes les pesées : la dernière
    public void Pesees_dans_le_desordre(string date, string expectedKg)
    {
        WeighIn[] shuffled = [WeighIns[2], WeighIns[0], WeighIns[1]];

        Assert.Equal(
            decimal.Parse(expectedKg, CultureInfo.InvariantCulture),
            WeightHistory.WeightOn(shuffled, Date(date)));
    }

    private static DateOnly Date(string date) => DateOnly.Parse(date, CultureInfo.InvariantCulture);

    private static WeighIn WeighIn(string date, decimal weightKg) => new() { Date = Date(date), WeightKg = weightKg };
}
