using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutTemplateConfiguration : IEntityTypeConfiguration<WorkoutTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplate> b)
    {
        b.ToTable("WorkoutTemplates", "app", t =>
            t.HasCheckConstraint("CK_WorkoutTemplates_CardioMet", "[CardioMet] BETWEEN 1 AND 20"));

        b.Property(t => t.Code).HasMaxLength(5).IsUnicode(false);
        b.Property(t => t.Name).HasMaxLength(60);
        b.Property(t => t.CardioName).HasMaxLength(100);
        b.Property(t => t.CardioDetail).HasMaxLength(300);
        b.Property(t => t.CardioMet).HasPrecision(4, 2);

        b.HasIndex(t => new { t.ProgramId, t.Code }).IsUnique().HasDatabaseName("UQ_WorkoutTemplates_Code");

        b.HasOne(t => t.Program).WithMany(p => p.Templates)
            .HasForeignKey(t => t.ProgramId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkoutTemplates_Program");
    }
}
