using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutTemplateExerciseConfiguration : IEntityTypeConfiguration<WorkoutTemplateExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplateExercise> b)
    {
        b.ToTable("WorkoutTemplateExercises", "app", t =>
        {
            t.HasCheckConstraint("CK_WorkoutTemplateExercises_Sets", "[Sets] BETWEEN 1 AND 10");
            t.HasCheckConstraint("CK_WorkoutTemplateExercises_Reps", "[RepsMin] IS NULL OR [RepsMax] IS NULL OR [RepsMin] <= [RepsMax]");
            t.HasCheckConstraint("CK_WorkoutTemplateExercises_Seconds", "[SecondsMin] IS NULL OR [SecondsMax] IS NULL OR [SecondsMin] <= [SecondsMax]");
        });

        b.Property(e => e.LoadIncrementKg).HasPrecision(4, 2);

        b.HasIndex(e => new { e.WorkoutTemplateId, e.Position }).IsUnique().HasDatabaseName("UQ_WorkoutTemplateExercises_Position");

        b.HasOne(e => e.Template).WithMany(t => t.Exercises)
            .HasForeignKey(e => e.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkoutTemplateExercises_Template");
        // Un exercice utilisé dans un programme ne peut pas être supprimé
        b.HasOne(e => e.Exercise).WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WorkoutTemplateExercises_Exercise");
    }
}
