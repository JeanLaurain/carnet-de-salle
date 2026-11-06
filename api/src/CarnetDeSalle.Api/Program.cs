using CarnetDeSalle.Api.Data;
using CarnetDeSalle.Api.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// « --migrate » : applique les migrations EF Core puis s'arrête (service « migrate » du docker compose).
var migrateOnly = args.Contains("--migrate");
var builder = WebApplication.CreateBuilder(args.Where(a => a != "--migrate").ToArray());

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

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

// TODO (ticket « Entités EF Core ») : AddDbContext + .AddDbContextCheck<AppDbContext>() sur les health checks.

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
    // TODO (ticket « Migration initiale ») :
    // using var scope = app.Services.CreateScope();
    // await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    app.Logger.LogInformation("Aucune migration à appliquer pour l'instant.");
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

app.Run();

/// <summary>Version déployée (tag de l'image Docker).</summary>
public record VersionInfo(string Version);

/// <summary>Rend Program visible pour WebApplicationFactory dans les tests.</summary>
public partial class Program { }
