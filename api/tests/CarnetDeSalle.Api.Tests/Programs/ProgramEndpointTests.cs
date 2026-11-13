using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarnetDeSalle.Api.Data;
using CarnetDeSalle.Api.Entities;
using CarnetDeSalle.Api.Features.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarnetDeSalle.Api.Tests.Programs;

/// <summary>GET /api/program, contre une vraie base SQL Server (Testcontainers).</summary>
public class ProgramEndpointTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    // Mêmes réglages que l'API : enums en camelCase ("load", "time")
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [Fact]
    public async Task Sans_utilisateur_connecte_401()
    {
        var response = await api.CreateClient().GetAsync("/api/program", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Premier_appel_cree_le_programme_par_defaut()
    {
        var userId = await api.CreateUserAsync();

        var program = await GetProgramAsync(userId);

        Assert.Equal(7, program.Version);
        Assert.Equal("Full body débutant A/B", program.Name);
        Assert.Equal(90, program.DefaultRestSec);
        Assert.Equal(2, program.ChangeNotes.Count);

        Assert.Equal("Vélo ou rameur", program.Warmup.Title);
        Assert.Equal(5, program.Warmup.Minutes);

        // Séances A puis B, cinq exercices chacune, dans l'ordre du fichier
        Assert.Equal(["A", "B"], program.Sessions.Select(s => s.Code));
        Assert.Equal(
            ["presse", "dc_machine", "tirage", "leg_curl", "planche"],
            program.Sessions[0].Exercises.Select(e => e.Code));
        Assert.Equal(
            ["hack_squat", "rowing_machine", "dev_epaules", "hip_thrust_machine", "crunch_machine"],
            program.Sessions[1].Exercises.Select(e => e.Code));
        Assert.All(program.Sessions, s => Assert.Equal([1, 2, 3, 4, 5], s.Exercises.Select(e => (int)e.Position)));

        // Cardio de fin de séance, avec son MET pour le calcul des kcal
        Assert.Equal("Tapis de course incliné", program.Sessions[0].Cardio.Name);
        Assert.Equal(15, program.Sessions[0].Cardio.Minutes);
        Assert.Equal(7m, program.Sessions[0].Cardio.Met);
        Assert.Equal("Vélo", program.Sessions[1].Cardio.Name);
        Assert.Equal(6m, program.Sessions[1].Cardio.Met);

        // Objectifs d'un exercice à charge
        var presse = program.Sessions[0].Exercises[0];
        Assert.Equal(MeasureType.Load, presse.MeasureType);
        Assert.Equal(3, presse.Sets);
        Assert.Equal((byte)10, presse.RepsMin);
        Assert.Equal((byte)12, presse.RepsMax);
        Assert.Equal(5m, presse.LoadIncrementKg);
        Assert.Equal(3, presse.Tips.Count);
        Assert.All(presse.Tips, tip => Assert.False(string.IsNullOrWhiteSpace(tip)));

        // ... et d'un exercice en secondes
        var planche = program.Sessions[0].Exercises[4];
        Assert.Equal(MeasureType.Time, planche.MeasureType);
        Assert.NotNull(planche.SecondsMin);
        Assert.Null(planche.RepsMin);

        // Le cycle commence le jour du premier appel
        Assert.Equal(ParisToday(), program.Cycle.StartsOn);
        Assert.Equal((byte)8, program.Cycle.PlannedWeeks);
        Assert.Equal(1, program.Cycle.CurrentWeek);
        Assert.False(program.Cycle.IsOver);
    }

    [Fact]
    public async Task Les_types_de_mesure_sortent_en_texte()
    {
        var userId = await api.CreateUserAsync();

        var json = await api.CreateClientFor(userId)
            .GetStringAsync("/api/program", TestContext.Current.CancellationToken);

        // Contrat avec le front : "load" / "time", pas 0 / 1
        Assert.Contains("\"measureType\":\"load\"", json);
        Assert.Contains("\"measureType\":\"time\"", json);
    }

    [Fact]
    public async Task Le_repos_effectif_tient_compte_du_repos_par_defaut()
    {
        var userId = await api.CreateUserAsync();

        var exercises = (await GetProgramAsync(userId)).Sessions
            .SelectMany(s => s.Exercises)
            .ToDictionary(e => e.Code);

        Assert.Equal(90, exercises["presse"].RestSec);          // pas de repos dans le fichier : celui du programme
        Assert.Equal(60, exercises["planche"].RestSec);         // repos propre à l'exercice
        Assert.Equal(60, exercises["crunch_machine"].RestSec);
    }

    [Fact]
    public async Task Deuxieme_appel_ne_cree_pas_de_second_programme()
    {
        var userId = await api.CreateUserAsync();

        await GetProgramAsync(userId);
        await GetProgramAsync(userId);

        Assert.Equal(1, await CountProgramsAsync(userId));
    }

    [Fact]
    public async Task Appels_simultanes_un_seul_programme()
    {
        var userId = await api.CreateUserAsync();
        var client = api.CreateClientFor(userId);

        // Premier affichage de l'appli : plusieurs requêtes peuvent partir en même temps
        var responses = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => client.GetAsync("/api/program", TestContext.Current.CancellationToken)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        Assert.Equal(1, await CountProgramsAsync(userId));
    }

    [Fact]
    public async Task Chaque_utilisateur_a_son_propre_programme()
    {
        var alice = await api.CreateUserAsync();
        var bob = await api.CreateUserAsync();

        await GetProgramAsync(alice);
        await WithDbAsync(db => db.Programs
            .Where(p => p.UserId == alice)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Name, "Programme d'Alice")));

        var programBob = await GetProgramAsync(bob);

        Assert.Equal("Full body débutant A/B", programBob.Name);
        Assert.Equal(1, await CountProgramsAsync(alice));
        Assert.Equal(1, await CountProgramsAsync(bob));
    }

    [Fact]
    public async Task Un_programme_existant_n_est_pas_remplace()
    {
        var userId = await api.CreateUserAsync();
        await GetProgramAsync(userId);
        await WithDbAsync(db => db.Programs
            .Where(p => p.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Name, "Mon programme")));

        var program = await GetProgramAsync(userId);

        Assert.Equal("Mon programme", program.Name);
    }

    [Theory]
    [InlineData(15, 3, false)]   // 15 jours après le début : semaine 3 sur 8
    [InlineData(55, 8, false)]   // dernier jour de la semaine 8
    [InlineData(56, 9, true)]    // cycle terminé : le front affiche le rappel
    public async Task La_semaine_du_cycle_avance_avec_le_temps(int daysSinceStart, int week, bool isOver)
    {
        var userId = await api.CreateUserAsync();
        await GetProgramAsync(userId);
        var startsOn = ParisToday().AddDays(-daysSinceStart);
        await WithDbAsync(db => db.Programs
            .Where(p => p.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StartsOn, startsOn)));

        var cycle = (await GetProgramAsync(userId)).Cycle;

        Assert.Equal(startsOn, cycle.StartsOn);
        Assert.Equal(week, cycle.CurrentWeek);
        Assert.Equal(isOver, cycle.IsOver);
    }

    private async Task<ProgramDto> GetProgramAsync(Guid userId)
    {
        var response = await api.CreateClientFor(userId)
            .GetAsync("/api/program", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var program = await response.Content.ReadFromJsonAsync<ProgramDto>(Json, TestContext.Current.CancellationToken);
        Assert.NotNull(program);
        return program;
    }

    private Task<int> CountProgramsAsync(Guid userId) =>
        WithDbAsync(db => db.Programs.CountAsync(p => p.UserId == userId));

    private async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        using var scope = api.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    // Les utilisateurs de test n'ont pas de profil : l'API prend Europe/Paris
    private static DateOnly ParisToday() => ProgramCycle.Today(TimeProvider.System, null);
}
