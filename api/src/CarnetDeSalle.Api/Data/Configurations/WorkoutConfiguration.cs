using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarnetDeSalle.Api.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> b)
    {
        b.ToTable("Workouts", "app", t =>
        {
            t.HasCheckConstraint("CK_Workouts_Duration", "[DurationMinutes] IS NULL OR [DurationMinutes] BETWEEN 1 AND 600");
            t.HasCheckConstraint("CK_Workouts_Cardio", "[CardioMinutes] IS NULL OR [CardioMinutes] BETWEEN 0 AND 300");
        });

        // Id fourni par le front ; à défaut, SQL Server en génère un
        b.Property(w => w.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        b.Property(w => w.TemplateCode).HasMaxLength(5).IsUnicode(false);
        b.Property(w => w.StartedAt).HasPrecision(0);
        b.Property(w => w.Note).HasMaxLength(2000);
        b.Property(w => w.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasIndex(w => new { w.UserId, w.Date }).IsDescending(false, true).HasDatabaseName("IX_Workouts_User_Date");

        b.HasOne(w => w.User).WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Workouts_User");
        // Pas de cascade depuis le programme : SQL Server refuse deux chemins de suppression vers une même table
        b.HasOne(w => w.Program).WithMany()
            .HasForeignKey(w => w.ProgramId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Workouts_Program");
        b.HasOne(w => w.Template).WithMany()
            .HasForeignKey(w => w.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Workouts_Template");
    }
}
