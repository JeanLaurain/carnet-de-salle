using CarnetDeSalle.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarnetDeSalle.Api.Data;

/// <summary>
/// Base de l'appli : tables Identity (dbo.AspNet*) et tables métier (schéma app).
/// Référence : db/schema.sql. Une configuration par table dans Data/Configurations.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseTip> ExerciseTips => Set<ExerciseTip>();
    public DbSet<TrainingProgram> Programs => Set<TrainingProgram>();
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises => Set<WorkoutTemplateExercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<WorkoutAnalysis> WorkoutAnalyses => Set<WorkoutAnalysis>();
    public DbSet<WorkoutAnalysisItem> WorkoutAnalysisItems => Set<WorkoutAnalysisItem>();
    public DbSet<WeighIn> WeighIns => Set<WeighIn>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealItem> MealItems => Set<MealItem>();
    public DbSet<AiUsage> AiUsages => Set<AiUsage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HavePrecision(0);
    }
}
