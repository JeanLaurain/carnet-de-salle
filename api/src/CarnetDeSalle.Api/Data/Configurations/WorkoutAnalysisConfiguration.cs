using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutAnalysisConfiguration : IEntityTypeConfiguration<WorkoutAnalysis>
{
    public void Configure(EntityTypeBuilder<WorkoutAnalysis> b)
    {
        b.ToTable("WorkoutAnalyses", "app", t =>
            t.HasCheckConstraint("CK_WorkoutAnalyses_Source", "[Source] IN ('claude', 'manual')"));
        b.HasKey(a => a.WorkoutId);

        b.Property(a => a.Source).HasConversion<LowercaseEnumConverter<AnalysisSource>>().HasMaxLength(10).IsUnicode(false);
        b.Property(a => a.Title).HasMaxLength(100);
        b.Property(a => a.Summary).HasMaxLength(1000);
        b.Property(a => a.Advice).HasMaxLength(400);
        b.Property(a => a.Model).HasMaxLength(60).IsUnicode(false);
        b.Property(a => a.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Une analyse au plus par séance, supprimée avec elle
        b.HasOne(a => a.Workout).WithOne(w => w.Analysis)
            .HasForeignKey<WorkoutAnalysis>(a => a.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkoutAnalyses_Workout");
    }
}
