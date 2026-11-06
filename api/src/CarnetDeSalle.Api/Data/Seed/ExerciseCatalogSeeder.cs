using System.Text.Json;
using CarnetDeSalle.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarnetDeSalle.Api.Data.Seed;

public static class ExerciseCatalogSeeder
{
    private sealed record SeedFile(List<SeedExercise> Exercices);

    private sealed record SeedExercise(
        string Code, string Nom, string Muscles, string Mesure, List<string> Conseils,
        string? Alternative, string? AlternativeLabel, string? VideoRecherche, string? Illustration);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<int> SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var seed = await ReadSeedAsync(ct);

        var existing = await db.Exercises
            .Include(e => e.Tips)
            .Where(e => e.CreatedByUserId == null)
            .ToDictionaryAsync(e => e.Code, ct);

        foreach (var s in seed.Exercices)
        {
            if (!existing.TryGetValue(s.Code, out var exercise))
            {
                exercise = new Exercise { Code = s.Code };
                db.Exercises.Add(exercise);
            }

            exercise.Name = s.Nom;
            exercise.Muscles = s.Muscles;
            exercise.MeasureType = Enum.Parse<MeasureType>(s.Mesure, ignoreCase: true);
            exercise.Alternative = s.Alternative;
            exercise.AlternativeLabel = s.AlternativeLabel;
            exercise.VideoSearch = s.VideoRecherche;
            exercise.IllustrationKey = s.Illustration;

            // Conseils mis à jour place par place (index unique ExerciseId + Position)
            for (var i = 0; i < s.Conseils.Count; i++)
            {
                var position = (byte)(i + 1);
                var tip = exercise.Tips.FirstOrDefault(t => t.Position == position);
                if (tip is null)
                    exercise.Tips.Add(new ExerciseTip { Position = position, Text = s.Conseils[i] });
                else
                    tip.Text = s.Conseils[i];
            }
            exercise.Tips.RemoveAll(t => t.Position > s.Conseils.Count);
        }

        return await db.SaveChangesAsync(ct);
    }

    private static async Task<SeedFile> ReadSeedAsync(CancellationToken ct)
    {
        await using var stream = typeof(ExerciseCatalogSeeder).Assembly.GetManifestResourceStream("programme.json")
            ?? throw new InvalidOperationException("Resource 'programme.json' not found.");
        return await JsonSerializer.DeserializeAsync<SeedFile>(stream, JsonOptions, ct)
            ?? throw new InvalidOperationException("Resource 'programme.json' is empty or invalid.");
    }
}
