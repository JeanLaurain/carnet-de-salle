/* =============================================================================
   Carnet de salle : schéma de référence de la base (SQL Server 2022 / 2025)
   -----------------------------------------------------------------------------
   - Ce script décrit le schéma MÉTIER (schéma SQL `app`).
   - Les tables d'identité (dbo.AspNetUsers, dbo.AspNetUserLogins,
     dbo.AspNetUserPasskeys, ...) sont créées par la migration EF Core
     d'ASP.NET Core Identity (.NET 10, IdentitySchemaVersions.Version3).
   - En production, la base est créée par les migrations EF Core (code first).
     Ce fichier sert de référence pour écrire les entités et la configuration
     EF Core, et il est validé en CI contre un vrai SQL Server (db.yml).
   - Clés : uniqueidentifier pour les données créées par l'utilisateur
     (séances, repas, pesées), ce qui permet de générer l'Id côté client
     pour le mode hors ligne ; int/bigint IDENTITY pour le reste.
   - Dates « métier » en `date` (jour local de l'utilisateur), horodatages
     techniques en UTC (`datetime2(0)`).
   Documentation : docs/base-de-donnees.md
   ============================================================================= */

IF SCHEMA_ID(N'app') IS NULL EXEC (N'CREATE SCHEMA app AUTHORIZATION dbo');
GO

/* -----------------------------------------------------------------------------
   Profil : données qui servent au calcul de la dépense et des objectifs
   ----------------------------------------------------------------------------- */
CREATE TABLE app.Profiles (
    UserId              uniqueidentifier NOT NULL,
    Sex                 char(1)          NOT NULL CONSTRAINT DF_Profiles_Sex DEFAULT ('M'),
    BirthDate           date             NULL,              -- l'âge se calcule à la date du jour concerné
    HeightCm            smallint         NOT NULL CONSTRAINT DF_Profiles_HeightCm DEFAULT (173),
    ActivityFactor      decimal(4,3)     NOT NULL CONSTRAINT DF_Profiles_ActivityFactor DEFAULT (1.375),
    DailyKcalOffset     smallint         NOT NULL CONSTRAINT DF_Profiles_DailyKcalOffset DEFAULT (-500),
    ProteinGPerKg       decimal(3,1)     NOT NULL CONSTRAINT DF_Profiles_ProteinGPerKg DEFAULT (1.8),
    TimeZone            varchar(64)      NOT NULL CONSTRAINT DF_Profiles_TimeZone DEFAULT ('Europe/Paris'),
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_Profiles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_Profiles_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Profiles PRIMARY KEY (UserId),
    CONSTRAINT FK_Profiles_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Profiles_Sex CHECK (Sex IN ('M', 'F')),
    CONSTRAINT CK_Profiles_HeightCm CHECK (HeightCm BETWEEN 130 AND 220),
    CONSTRAINT CK_Profiles_ActivityFactor CHECK (ActivityFactor BETWEEN 1.1 AND 2.0),
    CONSTRAINT CK_Profiles_DailyKcalOffset CHECK (DailyKcalOffset BETWEEN -1000 AND 1000),
    CONSTRAINT CK_Profiles_ProteinGPerKg CHECK (ProteinGPerKg BETWEEN 0.8 AND 3.0)
);
GO

/* -----------------------------------------------------------------------------
   Catalogue d'exercices (commun à tous les utilisateurs)
   ----------------------------------------------------------------------------- */
CREATE TABLE app.Exercises (
    Id                  int IDENTITY (1, 1) NOT NULL,
    Code                varchar(50)      NOT NULL,          -- 'presse', 'hack_squat', ... (clé stable, sert aussi pour l'illustration)
    Name                nvarchar(100)    NOT NULL,
    Muscles             nvarchar(150)    NOT NULL,
    MeasureType         varchar(10)      NOT NULL,          -- load = charge × reps, time = secondes, reps = répétitions seules
    Alternative         nvarchar(300)    NULL,
    AlternativeLabel    nvarchar(60)     NULL,              -- ex. « Pas d'haltère libre ? » (sinon « Machine prise ? »)
    VideoSearch         nvarchar(150)    NULL,              -- requête YouTube de démonstration
    IllustrationKey     varchar(50)      NULL,
    IsActive            bit              NOT NULL CONSTRAINT DF_Exercises_IsActive DEFAULT (1),
    CONSTRAINT PK_Exercises PRIMARY KEY (Id),
    CONSTRAINT UQ_Exercises_Code UNIQUE (Code),
    CONSTRAINT CK_Exercises_MeasureType CHECK (MeasureType IN ('load', 'time', 'reps'))
);
GO

CREATE TABLE app.ExerciseTips (
    Id                  int IDENTITY (1, 1) NOT NULL,
    ExerciseId          int              NOT NULL,
    Position            tinyint          NOT NULL,
    Text                nvarchar(400)    NOT NULL,
    CONSTRAINT PK_ExerciseTips PRIMARY KEY (Id),
    CONSTRAINT FK_ExerciseTips_Exercise FOREIGN KEY (ExerciseId) REFERENCES app.Exercises (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ExerciseTips_Position UNIQUE (ExerciseId, Position)
);
GO

/* -----------------------------------------------------------------------------
   Programme : versionné, un seul programme actif par utilisateur
   ----------------------------------------------------------------------------- */
CREATE TABLE app.Programs (
    Id                  int IDENTITY (1, 1) NOT NULL,
    UserId              uniqueidentifier NOT NULL,
    Version             int              NOT NULL,
    Name                nvarchar(100)    NOT NULL,
    Goal                nvarchar(150)    NOT NULL,          -- « Perdre du gras et prendre du muscle »
    Frequency           nvarchar(100)    NOT NULL,          -- « 1 à 2 séances par semaine »
    Gym                 nvarchar(100)    NULL,              -- « One Fitness »
    DefaultRestSec      smallint         NOT NULL CONSTRAINT DF_Programs_DefaultRestSec DEFAULT (90),
    WarmupTitle         nvarchar(100)    NOT NULL,
    WarmupDetail        nvarchar(300)    NULL,
    WarmupMinutes       tinyint          NOT NULL,
    ChangeNotes         nvarchar(max)    NULL,              -- JSON : ["Nouvel onglet Repas ...", ...]
    IsActive            bit              NOT NULL CONSTRAINT DF_Programs_IsActive DEFAULT (1),
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_Programs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Programs PRIMARY KEY (Id),
    CONSTRAINT FK_Programs_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Programs_UserVersion UNIQUE (UserId, Version),
    CONSTRAINT CK_Programs_ChangeNotes CHECK (ChangeNotes IS NULL OR ISJSON(ChangeNotes) = 1)
);
GO
CREATE UNIQUE INDEX UX_Programs_OneActivePerUser ON app.Programs (UserId) WHERE IsActive = 1;
GO

/* Séances types du programme (A, B, ...) */
CREATE TABLE app.WorkoutTemplates (
    Id                  int IDENTITY (1, 1) NOT NULL,
    ProgramId           int              NOT NULL,
    Code                varchar(5)       NOT NULL,          -- 'A', 'B'
    Name                nvarchar(60)     NOT NULL,
    DurationMinutes     smallint         NOT NULL,
    CardioName          nvarchar(100)    NOT NULL,
    CardioDetail        nvarchar(300)    NULL,
    CardioMinutes       smallint         NOT NULL,
    CardioMet           decimal(4,2)     NOT NULL,          -- MET du cardio pour l'estimation des kcal (tapis incliné 7,0 ; vélo 6,0)
    Position            tinyint          NOT NULL,
    CONSTRAINT PK_WorkoutTemplates PRIMARY KEY (Id),
    CONSTRAINT FK_WorkoutTemplates_Program FOREIGN KEY (ProgramId) REFERENCES app.Programs (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WorkoutTemplates_Code UNIQUE (ProgramId, Code),
    CONSTRAINT CK_WorkoutTemplates_CardioMet CHECK (CardioMet BETWEEN 1 AND 20)
);
GO

CREATE TABLE app.WorkoutTemplateExercises (
    Id                  int IDENTITY (1, 1) NOT NULL,
    WorkoutTemplateId   int              NOT NULL,
    ExerciseId          int              NOT NULL,
    Position            tinyint          NOT NULL,
    Sets                tinyint          NOT NULL,
    RepsMin             tinyint          NULL,
    RepsMax             tinyint          NULL,
    SecondsMin          smallint         NULL,
    SecondsMax          smallint         NULL,
    LoadIncrementKg     decimal(4,2)     NULL,              -- palier de charge (2,5 ou 5 kg)
    RestSec             smallint         NULL,              -- NULL = repos par défaut du programme
    CONSTRAINT PK_WorkoutTemplateExercises PRIMARY KEY (Id),
    CONSTRAINT FK_WorkoutTemplateExercises_Template FOREIGN KEY (WorkoutTemplateId) REFERENCES app.WorkoutTemplates (Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkoutTemplateExercises_Exercise FOREIGN KEY (ExerciseId) REFERENCES app.Exercises (Id),
    CONSTRAINT UQ_WorkoutTemplateExercises_Position UNIQUE (WorkoutTemplateId, Position),
    CONSTRAINT CK_WorkoutTemplateExercises_Sets CHECK (Sets BETWEEN 1 AND 10),
    CONSTRAINT CK_WorkoutTemplateExercises_Reps CHECK (RepsMin IS NULL OR RepsMax IS NULL OR RepsMin <= RepsMax),
    CONSTRAINT CK_WorkoutTemplateExercises_Seconds CHECK (SecondsMin IS NULL OR SecondsMax IS NULL OR SecondsMin <= SecondsMax)
);
GO

/* -----------------------------------------------------------------------------
   Séances réalisées
   ----------------------------------------------------------------------------- */
CREATE TABLE app.Workouts (
    Id                  uniqueidentifier NOT NULL CONSTRAINT DF_Workouts_Id DEFAULT (NEWSEQUENTIALID()),
    UserId              uniqueidentifier NOT NULL,
    ProgramId           int              NULL,
    WorkoutTemplateId   int              NULL,
    TemplateCode        varchar(5)       NOT NULL,          -- copie du code ('A') au moment de la séance
    ProgramVersion      int              NOT NULL,
    [Date]              date             NOT NULL,
    StartedAt           datetimeoffset(0) NULL,
    DurationMinutes     smallint         NULL,
    WarmupDone          bit              NOT NULL CONSTRAINT DF_Workouts_WarmupDone DEFAULT (0),
    CardioMinutes       smallint         NULL,
    Note                nvarchar(2000)   NULL,              -- ressenti
    EstimatedKcal       smallint         NULL,              -- kcal brûlées (nettes du repos), recalculables
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_Workouts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Workouts PRIMARY KEY (Id),
    CONSTRAINT FK_Workouts_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Workouts_Program FOREIGN KEY (ProgramId) REFERENCES app.Programs (Id),
    CONSTRAINT FK_Workouts_Template FOREIGN KEY (WorkoutTemplateId) REFERENCES app.WorkoutTemplates (Id),
    CONSTRAINT CK_Workouts_Duration CHECK (DurationMinutes IS NULL OR DurationMinutes BETWEEN 1 AND 600),
    CONSTRAINT CK_Workouts_Cardio CHECK (CardioMinutes IS NULL OR CardioMinutes BETWEEN 0 AND 300)
);
GO
CREATE INDEX IX_Workouts_User_Date ON app.Workouts (UserId, [Date] DESC);
GO

CREATE TABLE app.WorkoutExercises (
    Id                  bigint IDENTITY (1, 1) NOT NULL,
    WorkoutId           uniqueidentifier NOT NULL,
    ExerciseId          int              NOT NULL,
    Position            tinyint          NOT NULL,
    ExerciseName        nvarchar(100)    NOT NULL,          -- copie du nom au moment de la séance
    MeasureType         varchar(10)      NOT NULL,
    CONSTRAINT PK_WorkoutExercises PRIMARY KEY (Id),
    CONSTRAINT FK_WorkoutExercises_Workout FOREIGN KEY (WorkoutId) REFERENCES app.Workouts (Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkoutExercises_Exercise FOREIGN KEY (ExerciseId) REFERENCES app.Exercises (Id),
    CONSTRAINT UQ_WorkoutExercises_Exercise UNIQUE (WorkoutId, ExerciseId),
    CONSTRAINT CK_WorkoutExercises_MeasureType CHECK (MeasureType IN ('load', 'time', 'reps'))
);
GO
CREATE INDEX IX_WorkoutExercises_Exercise ON app.WorkoutExercises (ExerciseId) INCLUDE (WorkoutId);
GO

CREATE TABLE app.WorkoutSets (
    Id                  bigint IDENTITY (1, 1) NOT NULL,
    WorkoutExerciseId   bigint           NOT NULL,
    SetNumber           tinyint          NOT NULL,
    LoadKg              decimal(5,2)     NULL,
    Reps                smallint         NULL,
    DurationSec         smallint         NULL,
    CONSTRAINT PK_WorkoutSets PRIMARY KEY (Id),
    CONSTRAINT FK_WorkoutSets_WorkoutExercise FOREIGN KEY (WorkoutExerciseId) REFERENCES app.WorkoutExercises (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WorkoutSets_Number UNIQUE (WorkoutExerciseId, SetNumber),
    CONSTRAINT CK_WorkoutSets_Values CHECK (
        (LoadKg IS NULL OR LoadKg BETWEEN 0 AND 500)
        AND (Reps IS NULL OR Reps BETWEEN 0 AND 200)
        AND (DurationSec IS NULL OR DurationSec BETWEEN 0 AND 3600)
        AND (Reps IS NOT NULL OR DurationSec IS NOT NULL))
);
GO

/* Analyse d'une séance (par Claude, ou consigne saisie à la main) */
CREATE TABLE app.WorkoutAnalyses (
    WorkoutId           uniqueidentifier NOT NULL,
    Source              varchar(10)      NOT NULL,          -- claude | manual
    Title               nvarchar(100)    NULL,
    Summary             nvarchar(1000)   NULL,              -- bilan
    Advice              nvarchar(400)    NULL,              -- conseil prioritaire
    Model               varchar(60)      NULL,              -- modèle Claude utilisé
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_WorkoutAnalyses_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_WorkoutAnalyses PRIMARY KEY (WorkoutId),
    CONSTRAINT FK_WorkoutAnalyses_Workout FOREIGN KEY (WorkoutId) REFERENCES app.Workouts (Id) ON DELETE CASCADE,
    CONSTRAINT CK_WorkoutAnalyses_Source CHECK (Source IN ('claude', 'manual'))
);
GO

CREATE TABLE app.WorkoutAnalysisItems (
    Id                  bigint IDENTITY (1, 1) NOT NULL,
    WorkoutId           uniqueidentifier NOT NULL,
    ExerciseId          int              NOT NULL,
    Feedback            nvarchar(400)    NULL,              -- avis
    Instruction         nvarchar(300)    NULL,              -- consigne pour la prochaine fois
    SuggestedLoadKg     decimal(5,2)     NULL,
    SuggestedDurationSec smallint        NULL,
    CONSTRAINT PK_WorkoutAnalysisItems PRIMARY KEY (Id),
    CONSTRAINT FK_WorkoutAnalysisItems_Analysis FOREIGN KEY (WorkoutId) REFERENCES app.WorkoutAnalyses (WorkoutId) ON DELETE CASCADE,
    CONSTRAINT FK_WorkoutAnalysisItems_Exercise FOREIGN KEY (ExerciseId) REFERENCES app.Exercises (Id),
    CONSTRAINT UQ_WorkoutAnalysisItems_Exercise UNIQUE (WorkoutId, ExerciseId)
);
GO

/* -----------------------------------------------------------------------------
   Pesées
   ----------------------------------------------------------------------------- */
CREATE TABLE app.WeighIns (
    Id                  uniqueidentifier NOT NULL CONSTRAINT DF_WeighIns_Id DEFAULT (NEWSEQUENTIALID()),
    UserId              uniqueidentifier NOT NULL,
    [Date]              date             NOT NULL,
    WeightKg            decimal(5,2)     NOT NULL,
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_WeighIns_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_WeighIns PRIMARY KEY (Id),
    CONSTRAINT FK_WeighIns_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WeighIns_UserDate UNIQUE (UserId, [Date]),
    CONSTRAINT CK_WeighIns_WeightKg CHECK (WeightKg BETWEEN 30 AND 250)
);
GO

/* -----------------------------------------------------------------------------
   Repas
   ----------------------------------------------------------------------------- */
CREATE TABLE app.Meals (
    Id                  uniqueidentifier NOT NULL CONSTRAINT DF_Meals_Id DEFAULT (NEWSEQUENTIALID()),
    UserId              uniqueidentifier NOT NULL,
    [Date]              date             NOT NULL,
    MealType            varchar(12)      NOT NULL,          -- breakfast | lunch | snack | dinner
    Name                nvarchar(80)     NOT NULL,
    Description         nvarchar(2000)   NULL,              -- texte saisi par l'utilisateur
    PortionText         nvarchar(120)    NULL,              -- « 1 assiette d'environ 350 g » (pour 1 portion)
    Servings            decimal(4,2)     NOT NULL CONSTRAINT DF_Meals_Servings DEFAULT (1),
    Kcal                smallint         NOT NULL,          -- total mangé (portions comprises)
    ProteinG            decimal(6,1)     NOT NULL CONSTRAINT DF_Meals_ProteinG DEFAULT (0),
    CarbsG              decimal(6,1)     NULL,
    FatG                decimal(6,1)     NULL,
    Confidence          varchar(10)      NULL,              -- high | medium | low (estimation Claude)
    Source              varchar(10)      NOT NULL,          -- claude | photo | manual | recent
    PhotoPath           nvarchar(260)    NULL,              -- optionnel : chemin de la photo sur le NAS
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_Meals_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Meals PRIMARY KEY (Id),
    CONSTRAINT FK_Meals_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Meals_MealType CHECK (MealType IN ('breakfast', 'lunch', 'snack', 'dinner')),
    CONSTRAINT CK_Meals_Servings CHECK (Servings > 0 AND Servings <= 12),
    CONSTRAINT CK_Meals_Kcal CHECK (Kcal BETWEEN 0 AND 10000),
    CONSTRAINT CK_Meals_Confidence CHECK (Confidence IS NULL OR Confidence IN ('high', 'medium', 'low')),
    CONSTRAINT CK_Meals_Source CHECK (Source IN ('claude', 'photo', 'manual', 'recent'))
);
GO
CREATE INDEX IX_Meals_User_Date ON app.Meals (UserId, [Date]) INCLUDE (Kcal, ProteinG, MealType);
GO

CREATE TABLE app.MealItems (
    Id                  bigint IDENTITY (1, 1) NOT NULL,
    MealId              uniqueidentifier NOT NULL,
    Position            tinyint          NOT NULL,
    Name                nvarchar(80)     NOT NULL,
    Quantity            nvarchar(60)     NULL,              -- « 200 g »
    Kcal                smallint         NULL,              -- pour 1 portion
    CONSTRAINT PK_MealItems PRIMARY KEY (Id),
    CONSTRAINT FK_MealItems_Meal FOREIGN KEY (MealId) REFERENCES app.Meals (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_MealItems_Position UNIQUE (MealId, Position)
);
GO

/* -----------------------------------------------------------------------------
   Suivi de l'usage de l'API Claude (phase « Claude », pour surveiller le coût)
   ----------------------------------------------------------------------------- */
CREATE TABLE app.AiUsage (
    Id                  bigint IDENTITY (1, 1) NOT NULL,
    UserId              uniqueidentifier NOT NULL,
    Feature             varchar(30)      NOT NULL,          -- meal-estimate | workout-analysis | ...
    Model               varchar(60)      NOT NULL,
    InputTokens         int              NOT NULL,
    OutputTokens        int              NOT NULL,
    Succeeded           bit              NOT NULL,
    CreatedAt           datetime2(0)     NOT NULL CONSTRAINT DF_AiUsage_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AiUsage PRIMARY KEY (Id),
    CONSTRAINT FK_AiUsage_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_AiUsage_User_CreatedAt ON app.AiUsage (UserId, CreatedAt);
GO
