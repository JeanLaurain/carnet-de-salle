using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutAnalysisItemConfiguration : IEntityTypeConfiguration<WorkoutAnalysisItem>
{
    public void Configure(EntityTypeBuilder<WorkoutAnalysisItem> b)
    {
        b.ToTable("WorkoutAnalysisItems", "app");

        b.Property(i => i.Feedback).HasMaxLength(400);
        b.Property(i => i.Instruction).HasMaxLength(300);
        b.Property(i => i.SuggestedLoadKg).HasPrecision(5, 2);

        b.HasIndex(i => new { i.WorkoutId, i.ExerciseId }).IsUnique().HasDatabaseName("UQ_WorkoutAnalysisItems_Exercise");

        b.HasOne(i => i.Analysis).WithMany(a => a.Items)
            .HasForeignKey(i => i.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkoutAnalysisItems_Analysis");
        b.HasOne(i => i.Exercise).WithMany()
            .HasForeignKey(i => i.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WorkoutAnalysisItems_Exercise");
    }
}
