using CarnetDeSalle.Api.Data;
using CarnetDeSalle.Api.Data.Seed;
using CarnetDeSalle.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace CarnetDeSalle.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sql =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2019-latest").Build();   // même version que le NAS

    public async ValueTask InitializeAsync()
    {
        await _sql.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await ExerciseCatalogSeeder.SeedAsync(db);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // L'API utilise la base du conteneur au lieu de celle d'appsettings
        builder.UseSetting("ConnectionStrings:Default", _sql.GetConnectionString());

        // Connexion factice : l'en-tête X-Test-User remplace le cookie (voir TestAuthHandler)
        builder.ConfigureTestServices(services => services
            .AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { }));
    }

    /// <summary>
    /// Crée un utilisateur dans AspNetUsers (les tables de l'appli ont une clé étrangère vers lui)
    /// et renvoie son Id. Un nouvel utilisateur par test : les tests ne se gênent pas entre eux.
    /// </summary>
    public async Task<Guid> CreateUserAsync()
    {
        var id = Guid.NewGuid();
        var email = $"{id}@test.local";

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(new AppUser
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await db.SaveChangesAsync();
        return id;
    }

    /// <summary>Client HTTP connecté en tant que cet utilisateur.</summary>
    public HttpClient CreateClientFor(Guid userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, userId.ToString());
        return client;
    }

    public override async ValueTask DisposeAsync()
    {
        await _sql.DisposeAsync();
        await base.DisposeAsync();
    }
}
