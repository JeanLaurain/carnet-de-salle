using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class ExerciseTipConfiguration : IEntityTypeConfiguration<ExerciseTip>
{
    public void Configure(EntityTypeBuilder<ExerciseTip> b)
    {
        b.ToTable("ExerciseTips", "app");

        b.Property(t => t.Text).HasMaxLength(400);

        b.HasIndex(t => new { t.ExerciseId, t.Position }).IsUnique().HasDatabaseName("UQ_ExerciseTips_Position");

        b.HasOne(t => t.Exercise).WithMany(e => e.Tips)
            .HasForeignKey(t => t.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ExerciseTips_Exercise");
    }
}
