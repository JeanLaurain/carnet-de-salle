using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class MealItemConfiguration : IEntityTypeConfiguration<MealItem>
{
    public void Configure(EntityTypeBuilder<MealItem> b)
    {
        b.ToTable("MealItems", "app");

        b.Property(i => i.Name).HasMaxLength(80);
        b.Property(i => i.Quantity).HasMaxLength(60);

        b.HasIndex(i => new { i.MealId, i.Position }).IsUnique().HasDatabaseName("UQ_MealItems_Position");

        b.HasOne(i => i.Meal).WithMany(m => m.Items)
            .HasForeignKey(i => i.MealId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MealItems_Meal");
    }
}
