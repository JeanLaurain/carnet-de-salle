using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> b)
    {
        b.ToTable("WorkoutSets", "app", t =>
            t.HasCheckConstraint("CK_WorkoutSets_Values",
                "([LoadKg] IS NULL OR [LoadKg] BETWEEN 0 AND 500)"
                + " AND ([Reps] IS NULL OR [Reps] BETWEEN 0 AND 200)"
                + " AND ([DurationSec] IS NULL OR [DurationSec] BETWEEN 0 AND 3600)"
                + " AND ([Reps] IS NOT NULL OR [DurationSec] IS NOT NULL)"));

        b.Property(s => s.LoadKg).HasPrecision(5, 2);

        b.HasIndex(s => new { s.WorkoutExerciseId, s.SetNumber }).IsUnique().HasDatabaseName("UQ_WorkoutSets_Number");

        b.HasOne(s => s.WorkoutExercise).WithMany(e => e.Sets)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkoutSets_WorkoutExercise");
    }
}
