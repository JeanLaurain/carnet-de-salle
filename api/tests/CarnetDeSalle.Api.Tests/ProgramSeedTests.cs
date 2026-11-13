using CarnetDeSalle.Api.Data.Seed;

namespace CarnetDeSalle.Api.Tests;

/// <summary>
/// Cohérence de programme.json, sans base : une faute de frappe dans le fichier se voit ici,
/// pas au premier GET /api/program d'un utilisateur.
/// </summary>
public class ProgramSeedTests
{
    [Fact]
    public async Task Le_fichier_se_lit_entierement()
    {
        var seed = await ProgramSeed.ReadSeedAsync(TestContext.Current.CancellationToken);

        Assert.Equal(7, seed.Version);
        Assert.Equal((byte)8, seed.PlannedWeeks);
        Assert.False(string.IsNullOrWhiteSpace(seed.Warmup.Title));
        Assert.NotEmpty(seed.ChangeNotes);
        Assert.NotEmpty(seed.Sessions);
        Assert.All(seed.Sessions, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Cardio.Name));
            Assert.True(s.Cardio.Met > 1, $"Séance {s.Code} : MET du cardio manquant");
            Assert.NotEmpty(s.Exercises);
        });
    }

    [Fact]
    public async Task Chaque_exercice_des_seances_est_dans_le_catalogue()
    {
        var seed = await ProgramSeed.ReadSeedAsync(TestContext.Current.CancellationToken);
        var catalogue = seed.Exercises.Select(e => e.Code).ToHashSet();

        var missing = seed.Sessions
            .SelectMany(s => s.Exercises)
            .Select(e => e.ExerciseCode)
            .Where(code => !catalogue.Contains(code));

        Assert.Empty(missing);   // en cas d'échec, xunit affiche les codes en cause
    }

    [Fact]
    public async Task Positions_uniques_et_objectifs_complets()
    {
        var seed = await ProgramSeed.ReadSeedAsync(TestContext.Current.CancellationToken);

        Assert.Equal(seed.Sessions.Count, seed.Sessions.Select(s => s.Position).Distinct().Count());
        Assert.Equal(seed.Sessions.Count, seed.Sessions.Select(s => s.Code).Distinct().Count());
        Assert.All(seed.Sessions, s =>
        {
            Assert.Equal(s.Exercises.Count, s.Exercises.Select(e => e.Position).Distinct().Count());
            // Chaque exercice a un objectif : des répétitions ou des secondes
            Assert.All(s.Exercises, e => Assert.True(
                e.RepsMin is not null || e.SecondsMin is not null,
                $"Séance {s.Code}, {e.ExerciseCode} : ni répétitions ni secondes"));
        });
    }
}
