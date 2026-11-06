using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> b)
    {
        b.ToTable("WorkoutExercises", "app", t =>
            t.HasCheckConstraint("CK_WorkoutExercises_MeasureType", "[MeasureType] IN ('load', 'time', 'reps')"));

        b.Property(e => e.ExerciseName).HasMaxLength(100);
        b.Property(e => e.MeasureType).HasConversion<LowercaseEnumConverter<MeasureType>>().HasMaxLength(10).IsUnicode(false);

        b.HasIndex(e => new { e.WorkoutId, e.ExerciseId }).IsUnique().HasDatabaseName("UQ_WorkoutExercises_Exercise");
        // Historique d'un exercice (onglet Suivi)
        b.HasIndex(e => e.ExerciseId).IncludeProperties(e => e.WorkoutId).HasDatabaseName("IX_WorkoutExercises_Exercise");

        b.HasOne(e => e.Workout).WithMany(w => w.Exercises)
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkoutExercises_Workout");
        b.HasOne(e => e.Exercise).WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WorkoutExercises_Exercise");
    }
}
