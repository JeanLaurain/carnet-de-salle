using System.Globalization;
using CarnetDeSalle.Api.Features.Programs;
using Microsoft.Extensions.Time.Testing;

namespace CarnetDeSalle.Api.Tests.Programs;

/// <summary>Règles du cycle (regles-metier.md §8) : tests unitaires, sans base ni serveur.</summary>
public class ProgramCycleTests
{
    private static readonly DateOnly Start = new(2026, 11, 2);   // un lundi

    [Theory]
    [InlineData("2026-11-02", 1, false)]   // jour du début : semaine 1
    [InlineData("2026-11-08", 1, false)]   // 6 jours après : encore semaine 1
    [InlineData("2026-11-09", 2, false)]   // 7 jours après : semaine 2
    [InlineData("2026-12-27", 8, false)]   // dernier jour de la semaine 8
    [InlineData("2026-12-28", 9, true)]    // au-delà des 8 semaines : la vraie semaine, et le rappel
    public void Semaine_en_cours(string today, int expectedWeek, bool expectedIsOver)
    {
        var cycle = ProgramCycle.Compute(Start, 8, DateOnly.Parse(today, CultureInfo.InvariantCulture));

        Assert.Equal(Start, cycle.StartsOn);
        Assert.Equal((byte)8, cycle.PlannedWeeks);
        Assert.Equal(expectedWeek, cycle.CurrentWeek);
        Assert.Equal(expectedIsOver, cycle.IsOver);
    }

    [Fact]
    public void Sans_date_de_debut_pas_de_semaine()
    {
        var cycle = ProgramCycle.Compute(null, 8, new DateOnly(2026, 11, 13));

        Assert.Null(cycle.CurrentWeek);
        Assert.False(cycle.IsOver);
    }

    [Fact]
    public void Debut_dans_le_futur_pas_de_semaine()
    {
        var cycle = ProgramCycle.Compute(Start, 8, Start.AddDays(-1));

        Assert.Null(cycle.CurrentWeek);
        Assert.False(cycle.IsOver);
    }

    [Fact]
    public void Sans_duree_prevue_le_cycle_ne_se_termine_jamais()
    {
        var cycle = ProgramCycle.Compute(Start, null, Start.AddDays(365));

        Assert.Equal(53, cycle.CurrentWeek);
        Assert.False(cycle.IsOver);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Europe/Paris")]
    public void Aujourd_hui_suit_l_heure_de_Paris_par_defaut(string? timeZone)
    {
        // 23 h 30 UTC le 12 novembre = 0 h 30 le 13 à Paris (UTC+1 en hiver)
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 12, 23, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 13), ProgramCycle.Today(clock, timeZone));
    }

    [Fact]
    public void Aujourd_hui_tient_compte_de_l_heure_d_ete()
    {
        // 22 h 30 UTC le 1er juillet = 0 h 30 le 2 à Paris (UTC+2 en été)
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 7, 1, 22, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 7, 2), ProgramCycle.Today(clock, null));
    }

    [Fact]
    public void Aujourd_hui_suit_le_fuseau_du_profil()
    {
        // Même instant : encore le 12 novembre à New York (UTC-5)
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 12, 23, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 12), ProgramCycle.Today(clock, "America/New_York"));
    }
}
