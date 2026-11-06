using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> b)
    {
        b.ToTable("Exercises", "app", t =>
            t.HasCheckConstraint("CK_Exercises_MeasureType", "[MeasureType] IN ('load', 'time', 'reps')"));

        b.Property(e => e.Code).HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.Name).HasMaxLength(100);
        b.Property(e => e.Muscles).HasMaxLength(150);
        b.Property(e => e.MeasureType).HasConversion<LowercaseEnumConverter<MeasureType>>().HasMaxLength(10).IsUnicode(false);
        b.Property(e => e.Alternative).HasMaxLength(300);
        b.Property(e => e.AlternativeLabel).HasMaxLength(60);
        b.Property(e => e.VideoSearch).HasMaxLength(150);
        b.Property(e => e.IllustrationKey).HasMaxLength(50).IsUnicode(false);

        b.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UQ_Exercises_Code");

        // Exercice ajouté par un utilisateur : supprimé avec son compte
        b.HasOne(e => e.CreatedBy).WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Exercises_CreatedBy");
    }
}
