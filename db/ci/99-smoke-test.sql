/* CI uniquement : insère un jeu de données minimal puis supprime l'utilisateur
   pour vérifier les contraintes et les suppressions en cascade. */
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @u uniqueidentifier = NEWID();
INSERT dbo.AspNetUsers (Id, UserName) VALUES (@u, N'ci@example.test');
INSERT app.Profiles (UserId, BirthDate) VALUES (@u, '1995-06-15');

INSERT app.Exercises (Code, Name, Muscles, MeasureType) VALUES ('presse', N'Presse à cuisses', N'Cuisses · fessiers', 'load'), ('planche', N'Gainage planche', N'Abdos · tronc', 'time');
DECLARE @presse int = (SELECT Id FROM app.Exercises WHERE Code = 'presse');
DECLARE @planche int = (SELECT Id FROM app.Exercises WHERE Code = 'planche');
INSERT app.ExerciseTips (ExerciseId, Position, Text) VALUES (@presse, 1, N'Pieds à largeur d''épaules.');

INSERT app.Programs (UserId, Version, Name, Goal, Frequency, WarmupTitle, WarmupMinutes, ChangeNotes)
VALUES (@u, 7, N'Full body', N'Perdre du gras et prendre du muscle', N'1 à 2 séances par semaine', N'Vélo ou rameur', 5, N'["Nouvel onglet Repas"]');
DECLARE @p int = SCOPE_IDENTITY();
INSERT app.WorkoutTemplates (ProgramId, Code, Name, DurationMinutes, CardioName, CardioMinutes, CardioMet, Position)
VALUES (@p, 'A', N'Séance A', 55, N'Tapis de course incliné', 15, 7.0, 1);
DECLARE @t int = SCOPE_IDENTITY();
INSERT app.WorkoutTemplateExercises (WorkoutTemplateId, ExerciseId, Position, Sets, RepsMin, RepsMax, LoadIncrementKg)
VALUES (@t, @presse, 1, 3, 10, 12, 5);
INSERT app.WorkoutTemplateExercises (WorkoutTemplateId, ExerciseId, Position, Sets, SecondsMin, SecondsMax, RestSec)
VALUES (@t, @planche, 2, 3, 20, 40, 60);

DECLARE @w uniqueidentifier = NEWID();
INSERT app.Workouts (Id, UserId, ProgramId, WorkoutTemplateId, TemplateCode, ProgramVersion, [Date], DurationMinutes, WarmupDone, CardioMinutes, Note, EstimatedKcal)
VALUES (@w, @u, @p, @t, 'A', 7, '2026-11-04', 51, 1, 15, N'Séance agréable', 240);
INSERT app.WorkoutExercises (WorkoutId, ExerciseId, Position, ExerciseName, MeasureType) VALUES (@w, @presse, 1, N'Presse à cuisses', 'load');
DECLARE @we bigint = SCOPE_IDENTITY();
INSERT app.WorkoutSets (WorkoutExerciseId, SetNumber, LoadKg, Reps) VALUES (@we, 1, 70, 12), (@we, 2, 70, 12), (@we, 3, 80, 16);
INSERT app.WorkoutExercises (WorkoutId, ExerciseId, Position, ExerciseName, MeasureType) VALUES (@w, @planche, 2, N'Gainage planche', 'time');
SET @we = SCOPE_IDENTITY();
INSERT app.WorkoutSets (WorkoutExerciseId, SetNumber, DurationSec) VALUES (@we, 1, 52), (@we, 2, 53), (@we, 3, 54);
INSERT app.WorkoutAnalyses (WorkoutId, Source, Title, Summary, Advice, Model) VALUES (@w, 'claude', N'Belle progression', N'Bilan', N'Conseil', 'claude-sonnet-5-5');
INSERT app.WorkoutAnalysisItems (WorkoutId, ExerciseId, Feedback, Instruction, SuggestedLoadKg) VALUES (@w, @presse, N'Avis', N'Consigne', 80);

INSERT app.WeighIns (UserId, [Date], WeightKg) VALUES (@u, '2026-10-25', 79), (@u, '2026-10-30', 78.5);
DECLARE @m uniqueidentifier = NEWID();
INSERT app.Meals (Id, UserId, [Date], MealType, Name, Servings, Kcal, ProteinG, CarbsG, FatG, Confidence, Source)
VALUES (@m, @u, '2026-11-04', 'lunch', N'Pâtes bolognaise', 1.5, 1005, 48, 138, 27, 'medium', 'claude');
INSERT app.MealItems (MealId, Position, Name, Quantity, Kcal) VALUES (@m, 1, N'Pâtes cuites', N'250 g', 390);
INSERT app.AiUsage (UserId, Feature, Model, InputTokens, OutputTokens, Succeeded) VALUES (@u, 'meal-estimate', 'claude-sonnet-5-5', 900, 300, 1);

/* Contrainte attendue : une seule version active par utilisateur */
BEGIN TRY
    INSERT app.Programs (UserId, Version, Name, Goal, Frequency, WarmupTitle, WarmupMinutes) VALUES (@u, 8, N'x', N'x', N'x', N'x', 5);
    THROW 50001, 'UX_Programs_OneActivePerUser aurait dû refuser un 2e programme actif.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 50001 THROW;
    PRINT 'OK : un seul programme actif par utilisateur.';
END CATCH;

/* Suppression en cascade de toutes les données de l'utilisateur */
DELETE dbo.AspNetUsers WHERE Id = @u;
IF EXISTS (SELECT 1 FROM app.Workouts) OR EXISTS (SELECT 1 FROM app.WorkoutSets) OR EXISTS (SELECT 1 FROM app.Meals)
   OR EXISTS (SELECT 1 FROM app.Programs) OR EXISTS (SELECT 1 FROM app.WeighIns) OR EXISTS (SELECT 1 FROM app.AiUsage)
    THROW 50002, 'La suppression en cascade a laissé des lignes.', 1;
PRINT 'OK : suppression en cascade.';
GO
