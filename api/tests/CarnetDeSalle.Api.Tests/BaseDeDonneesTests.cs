using System.Net;
using CarnetDeSalle.Api.Data;
using CarnetDeSalle.Api.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarnetDeSalle.Api.Tests;

public class BaseDeDonneesTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Health_verifie_aussi_la_base()
    {
        var response = await api.CreateClient().GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Relancer_le_catalogue_ne_change_rien()
    {
        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(0, await ExerciseCatalogSeeder.SeedAsync(db));
        Assert.Equal(10, await db.Exercises.CountAsync());
    }
}
