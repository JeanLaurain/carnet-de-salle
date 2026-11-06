using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> b)
    {
        b.ToTable("Meals", "app", t =>
        {
            t.HasCheckConstraint("CK_Meals_MealType", "[MealType] IN ('breakfast', 'lunch', 'snack', 'dinner')");
            t.HasCheckConstraint("CK_Meals_Servings", "[Servings] > 0 AND [Servings] <= 12");
            t.HasCheckConstraint("CK_Meals_Kcal", "[Kcal] BETWEEN 0 AND 10000");
            t.HasCheckConstraint("CK_Meals_Confidence", "[Confidence] IS NULL OR [Confidence] IN ('high', 'medium', 'low')");
            t.HasCheckConstraint("CK_Meals_Source", "[Source] IN ('claude', 'photo', 'manual', 'recent')");
        });

        b.Property(m => m.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        b.Property(m => m.MealType).HasConversion<LowercaseEnumConverter<MealType>>().HasMaxLength(12).IsUnicode(false);
        b.Property(m => m.Name).HasMaxLength(80);
        b.Property(m => m.Description).HasMaxLength(2000);
        b.Property(m => m.PortionText).HasMaxLength(120);
        b.Property(m => m.Servings).HasPrecision(4, 2);
        b.Property(m => m.ProteinG).HasPrecision(6, 1);
        b.Property(m => m.CarbsG).HasPrecision(6, 1);
        b.Property(m => m.FatG).HasPrecision(6, 1);
        b.Property(m => m.FiberG).HasPrecision(6, 1);
        b.Property(m => m.Confidence).HasConversion<LowercaseEnumConverter<EstimateConfidence>>().HasMaxLength(10).IsUnicode(false);
        b.Property(m => m.Source).HasConversion<LowercaseEnumConverter<MealSource>>().HasMaxLength(10).IsUnicode(false);
        b.Property(m => m.PhotoPath).HasMaxLength(260);
        b.Property(m => m.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Bilan du jour : l'index contient déjà tout ce qu'il faut additionner
        b.HasIndex(m => new { m.UserId, m.Date })
            .IncludeProperties(m => new { m.Kcal, m.ProteinG, m.CarbsG, m.FatG, m.FiberG, m.MealType })
            .HasDatabaseName("IX_Meals_User_Date");

        b.HasOne(m => m.User).WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Meals_User");
    }
}
