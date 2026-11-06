using CarnetDeSalle.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarnetDeSalle.Api.Tests;

/// <summary>
/// Le modèle EF Core se construit et produit le schéma attendu. Aucune base n'est nécessaire :
/// GenerateCreateScript écrit seulement le SQL de création.
/// </summary>
public class ModelTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private string CreateScript()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Database.GenerateCreateScript();
    }

    [Theory]
    [InlineData("Profiles")]
    [InlineData("Exercises")]
    [InlineData("ExerciseTips")]
    [InlineData("Programs")]
    [InlineData("WorkoutTemplates")]
    [InlineData("WorkoutTemplateExercises")]
    [InlineData("Workouts")]
    [InlineData("WorkoutExercises")]
    [InlineData("WorkoutSets")]
    [InlineData("WorkoutAnalyses")]
    [InlineData("WorkoutAnalysisItems")]
    [InlineData("WeighIns")]
    [InlineData("Meals")]
    [InlineData("MealItems")]
    [InlineData("AiUsage")]
    public void Chaque_table_metier_est_dans_le_schema_app(string table)
    {
        Assert.Contains($"CREATE TABLE [app].[{table}]", CreateScript());
    }

    [Fact]
    public void Les_tables_Identity_incluent_les_passkeys()
    {
        var script = CreateScript();

        Assert.Contains("CREATE TABLE [AspNetUsers]", script);
        Assert.Contains("CREATE TABLE [AspNetUserPasskeys]", script);
    }

    [Fact]
    public void Un_seul_programme_actif_par_utilisateur()
    {
        Assert.Contains("CREATE UNIQUE INDEX [UX_Programs_OneActivePerUser] ON [app].[Programs] ([UserId]) WHERE [IsActive] = 1", CreateScript());
    }

    [Fact]
    public void Les_enums_sont_stockes_comme_dans_le_schema()
    {
        var script = CreateScript();

        Assert.Contains("[MeasureType] varchar(10) NOT NULL", script);
        Assert.Contains("[Sex] char(1) NOT NULL", script);
        Assert.Contains("[MealType] varchar(12) NOT NULL", script);
    }
}
