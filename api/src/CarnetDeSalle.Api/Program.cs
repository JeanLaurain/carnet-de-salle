using System.Text.Json;
using System.Text.Json.Serialization;
using CarnetDeSalle.Api.Data;
using CarnetDeSalle.Api.Data.Seed;
using CarnetDeSalle.Api.Entities;
using CarnetDeSalle.Api.Features.Programs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// « --migrate » : applique les migrations EF Core puis s'arrête (service « migrate » du docker compose).
var migrateOnly = args.Contains("--migrate");
var builder = WebApplication.CreateBuilder(args.Where(a => a != "--migrate").ToArray());

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.UseCompatibilityLevel(150));
});

builder.Services
    .AddIdentityCore<AppUser>(o => o.Stores.SchemaVersion = IdentitySchemaVersions.Version3)
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();

// Le vrai schéma de connexion (cookie) arrive avec #27 : d'ici là, seuls les tests
// (TestAuthHandler) peuvent appeler les routes protégées.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Horloge injectée : les tests la remplacent pour fixer « aujourd'hui »
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<DefaultProgramService>();

// MeasureType sort en "load" / "time" / "reps" au lieu de 0 / 1 / 2
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

// Clés des cookies conservées dans un volume : sinon chaque déploiement déconnecte tout le monde.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("CarnetDeSalle");
}

// L'API est derrière deux proxys (reverse proxy DSM puis nginx) et n'est joignable
// que sur le réseau Docker interne : on fait confiance aux en-têtes X-Forwarded-*.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 2;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (migrateOnly)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    var changes = await ExerciseCatalogSeeder.SeedAsync(dbContext);
    app.Logger.LogInformation("Migrations appliquées, catalogue d'exercices : {Changes} modification(s)", changes);
    return;
}

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var api = app.MapGroup("/api");

api.MapHealthChecks("/health");

api.MapGet("/version", (IConfiguration configuration) => new VersionInfo(configuration["App:Version"] ?? "dev"))
    .WithName("GetVersion");

api.MapProgramEndpoints();

app.Run();

/// <summary>Version déployée (tag de l'image Docker).</summary>
public record VersionInfo(string Version);

/// <summary>Rend Program visible pour WebApplicationFactory dans les tests.</summary>
public partial class Program { }
