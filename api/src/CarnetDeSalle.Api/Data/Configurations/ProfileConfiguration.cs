using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.ToTable("Profiles", "app", t =>
        {
            t.HasCheckConstraint("CK_Profiles_Sex", "[Sex] IN ('M', 'F')");
            t.HasCheckConstraint("CK_Profiles_HeightCm", "[HeightCm] BETWEEN 130 AND 220");
            t.HasCheckConstraint("CK_Profiles_ActivityFactor", "[ActivityFactor] BETWEEN 1.1 AND 2.0");
            t.HasCheckConstraint("CK_Profiles_DailyKcalOffset", "[DailyKcalOffset] BETWEEN -1000 AND 1000");
            t.HasCheckConstraint("CK_Profiles_ProteinGPerKg", "[ProteinGPerKg] BETWEEN 0.8 AND 3.0");
            t.HasCheckConstraint("CK_Profiles_FatShareOfKcal", "[FatShareOfKcal] BETWEEN 0.15 AND 0.45");
            t.HasCheckConstraint("CK_Profiles_FiberTargetG", "[FiberTargetG] BETWEEN 10 AND 80");
        });
        b.HasKey(p => p.UserId);

        // char(1) : « M » ou « F »
        b.Property(p => p.Sex)
            .HasConversion(v => v == Sex.Female ? "F" : "M", v => v == "F" ? Sex.Female : Sex.Male)
            .HasMaxLength(1).IsFixedLength().IsUnicode(false);
        b.Property(p => p.ActivityFactor).HasPrecision(4, 3);
        b.Property(p => p.ProteinGPerKg).HasPrecision(3, 1);
        b.Property(p => p.FatShareOfKcal).HasPrecision(3, 2);
        b.Property(p => p.TimeZone).HasMaxLength(64).IsUnicode(false);
        b.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Profil 1-1 avec le compte : supprimé avec lui
        b.HasOne(p => p.User).WithOne()
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Profiles_User");
    }
}
