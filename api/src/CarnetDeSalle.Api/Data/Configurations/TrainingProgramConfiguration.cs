using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> b)
    {
        b.ToTable("Programs", "app", t =>
        {
            t.HasCheckConstraint("CK_Programs_ChangeNotes", "[ChangeNotes] IS NULL OR ISJSON([ChangeNotes]) = 1");
            t.HasCheckConstraint("CK_Programs_PlannedWeeks", "[PlannedWeeks] IS NULL OR [PlannedWeeks] BETWEEN 1 AND 52");
        });

        b.Property(p => p.Name).HasMaxLength(100);
        b.Property(p => p.Goal).HasMaxLength(150);
        b.Property(p => p.Frequency).HasMaxLength(100);
        b.Property(p => p.Gym).HasMaxLength(100);
        b.Property(p => p.WarmupTitle).HasMaxLength(100);
        b.Property(p => p.WarmupDetail).HasMaxLength(300);
        b.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        // ChangeNotes (List<string>) : EF le stocke en JSON dans un nvarchar(max)

        b.HasIndex(p => new { p.UserId, p.Version }).IsUnique().HasDatabaseName("UQ_Programs_UserVersion");
        // Un seul programme actif par utilisateur
        b.HasIndex(p => p.UserId).IsUnique().HasFilter("[IsActive] = 1").HasDatabaseName("UX_Programs_OneActivePerUser");

        b.HasOne(p => p.User).WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Programs_User");
    }
}
