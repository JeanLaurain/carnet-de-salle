using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WeighInConfiguration : IEntityTypeConfiguration<WeighIn>
{
    public void Configure(EntityTypeBuilder<WeighIn> b)
    {
        b.ToTable("WeighIns", "app", t =>
        {
            t.HasCheckConstraint("CK_WeighIns_WeightKg", "[WeightKg] BETWEEN 30 AND 250");
            t.HasCheckConstraint("CK_WeighIns_WaistCm", "[WaistCm] IS NULL OR [WaistCm] BETWEEN 40 AND 200");
        });

        b.Property(w => w.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        b.Property(w => w.WeightKg).HasPrecision(5, 2);
        b.Property(w => w.WaistCm).HasPrecision(4, 1);
        b.Property(w => w.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Une pesée par jour
        b.HasIndex(w => new { w.UserId, w.Date }).IsUnique().HasDatabaseName("UQ_WeighIns_UserDate");

        b.HasOne(w => w.User).WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WeighIns_User");
    }
}
