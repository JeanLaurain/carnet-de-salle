using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class AiUsageConfiguration : IEntityTypeConfiguration<AiUsage>
{
    public void Configure(EntityTypeBuilder<AiUsage> b)
    {
        b.ToTable("AiUsage", "app");

        b.Property(u => u.Feature).HasMaxLength(30).IsUnicode(false);
        b.Property(u => u.Model).HasMaxLength(60).IsUnicode(false);
        b.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasIndex(u => new { u.UserId, u.CreatedAt }).HasDatabaseName("IX_AiUsage_User_CreatedAt");

        b.HasOne(u => u.User).WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_AiUsage_User");
    }
}
