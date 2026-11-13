using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarnetDeSalle.Api.Data.Seed;

public static class ExerciseCatalogSeeder
{
    public static async Task<int> SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var seed = await ProgramSeed.ReadSeedAsync(ct);

        var existing = await db.Exercises
            .Include(e => e.Tips)
            .Where(e => e.CreatedByUserId == null)
            .ToDictionaryAsync(e => e.Code, ct);

        foreach (var s in seed.Exercises)
        {
            if (!existing.TryGetValue(s.Code, out var exercise))
            {
                exercise = new Exercise { Code = s.Code };
                db.Exercises.Add(exercise);
            }

            exercise.Name = s.Name;
            exercise.Muscles = s.Muscles;
            exercise.MeasureType = s.MeasureType;
            exercise.Alternative = s.Alternative;
            exercise.AlternativeLabel = s.AlternativeLabel;
            exercise.VideoSearch = s.VideoSearch;
            exercise.IllustrationKey = s.IllustrationKey;

            // Conseils mis à jour place par place (index unique ExerciseId + Position)
            for (var i = 0; i < s.Tips.Count; i++)
            {
                var position = (byte)(i + 1);
                var tip = exercise.Tips.FirstOrDefault(t => t.Position == position);
                if (tip is null)
                    exercise.Tips.Add(new ExerciseTip { Position = position, Text = s.Tips[i] });
                else
                    tip.Text = s.Tips[i];
            }
            exercise.Tips.RemoveAll(t => t.Position > s.Tips.Count);
        }

        return await db.SaveChangesAsync(ct);
    }
}
