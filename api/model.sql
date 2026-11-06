IF SCHEMA_ID(N'app') IS NULL EXEC(N'CREATE SCHEMA [app];');
GO


CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(256) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[AiUsage] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [Feature] varchar(30) NOT NULL,
    [Model] varchar(60) NOT NULL,
    [InputTokens] int NOT NULL,
    [OutputTokens] int NOT NULL,
    [Succeeded] bit NOT NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_AiUsage] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AiUsage_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserPasskeys] (
    [CredentialId] varbinary(1024) NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Data] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AspNetUserPasskeys] PRIMARY KEY ([CredentialId]),
    CONSTRAINT [FK_AspNetUserPasskeys_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[Exercises] (
    [Id] int NOT NULL IDENTITY,
    [Code] varchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Muscles] nvarchar(150) NOT NULL,
    [MeasureType] varchar(10) NOT NULL,
    [Alternative] nvarchar(300) NULL,
    [AlternativeLabel] nvarchar(60) NULL,
    [VideoSearch] nvarchar(150) NULL,
    [IllustrationKey] varchar(50) NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Exercises] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Exercises_MeasureType] CHECK ([MeasureType] IN ('load', 'time', 'reps')),
    CONSTRAINT [FK_Exercises_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[Meals] (
    [Id] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [UserId] uniqueidentifier NOT NULL,
    [Date] date NOT NULL,
    [MealType] varchar(12) NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [PortionText] nvarchar(120) NULL,
    [Servings] decimal(4,2) NOT NULL,
    [Kcal] smallint NOT NULL,
    [ProteinG] decimal(6,1) NOT NULL,
    [CarbsG] decimal(6,1) NULL,
    [FatG] decimal(6,1) NULL,
    [FiberG] decimal(6,1) NULL,
    [Confidence] varchar(10) NULL,
    [Source] varchar(10) NOT NULL,
    [PhotoPath] nvarchar(260) NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Meals] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Meals_Confidence] CHECK ([Confidence] IS NULL OR [Confidence] IN ('high', 'medium', 'low')),
    CONSTRAINT [CK_Meals_Kcal] CHECK ([Kcal] BETWEEN 0 AND 10000),
    CONSTRAINT [CK_Meals_MealType] CHECK ([MealType] IN ('breakfast', 'lunch', 'snack', 'dinner')),
    CONSTRAINT [CK_Meals_Servings] CHECK ([Servings] > 0 AND [Servings] <= 12),
    CONSTRAINT [CK_Meals_Source] CHECK ([Source] IN ('claude', 'photo', 'manual', 'recent')),
    CONSTRAINT [FK_Meals_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[Profiles] (
    [UserId] uniqueidentifier NOT NULL,
    [Sex] char(1) NOT NULL,
    [BirthDate] date NULL,
    [HeightCm] smallint NOT NULL,
    [ActivityFactor] decimal(4,3) NOT NULL,
    [DailyKcalOffset] smallint NOT NULL,
    [ProteinGPerKg] decimal(3,1) NOT NULL,
    [FatShareOfKcal] decimal(3,2) NOT NULL,
    [FiberTargetG] smallint NOT NULL,
    [TimeZone] varchar(64) NOT NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Profiles] PRIMARY KEY ([UserId]),
    CONSTRAINT [CK_Profiles_ActivityFactor] CHECK ([ActivityFactor] BETWEEN 1.1 AND 2.0),
    CONSTRAINT [CK_Profiles_DailyKcalOffset] CHECK ([DailyKcalOffset] BETWEEN -1000 AND 1000),
    CONSTRAINT [CK_Profiles_FatShareOfKcal] CHECK ([FatShareOfKcal] BETWEEN 0.15 AND 0.45),
    CONSTRAINT [CK_Profiles_FiberTargetG] CHECK ([FiberTargetG] BETWEEN 10 AND 80),
    CONSTRAINT [CK_Profiles_HeightCm] CHECK ([HeightCm] BETWEEN 130 AND 220),
    CONSTRAINT [CK_Profiles_ProteinGPerKg] CHECK ([ProteinGPerKg] BETWEEN 0.8 AND 3.0),
    CONSTRAINT [CK_Profiles_Sex] CHECK ([Sex] IN ('M', 'F')),
    CONSTRAINT [FK_Profiles_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[Programs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [Version] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Goal] nvarchar(150) NOT NULL,
    [Frequency] nvarchar(100) NOT NULL,
    [Gym] nvarchar(100) NULL,
    [DefaultRestSec] smallint NOT NULL,
    [WarmupTitle] nvarchar(100) NOT NULL,
    [WarmupDetail] nvarchar(300) NULL,
    [WarmupMinutes] tinyint NOT NULL,
    [ChangeNotes] nvarchar(max) NULL,
    [StartsOn] date NULL,
    [PlannedWeeks] tinyint NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Programs] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Programs_ChangeNotes] CHECK ([ChangeNotes] IS NULL OR ISJSON([ChangeNotes]) = 1),
    CONSTRAINT [CK_Programs_PlannedWeeks] CHECK ([PlannedWeeks] IS NULL OR [PlannedWeeks] BETWEEN 1 AND 52),
    CONSTRAINT [FK_Programs_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[WeighIns] (
    [Id] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [UserId] uniqueidentifier NOT NULL,
    [Date] date NOT NULL,
    [WeightKg] decimal(5,2) NOT NULL,
    [WaistCm] decimal(4,1) NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_WeighIns] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WeighIns_WaistCm] CHECK ([WaistCm] IS NULL OR [WaistCm] BETWEEN 40 AND 200),
    CONSTRAINT [CK_WeighIns_WeightKg] CHECK ([WeightKg] BETWEEN 30 AND 250),
    CONSTRAINT [FK_WeighIns_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[ExerciseTips] (
    [Id] int NOT NULL IDENTITY,
    [ExerciseId] int NOT NULL,
    [Position] tinyint NOT NULL,
    [Text] nvarchar(400) NOT NULL,
    CONSTRAINT [PK_ExerciseTips] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExerciseTips_Exercise] FOREIGN KEY ([ExerciseId]) REFERENCES [app].[Exercises] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[MealItems] (
    [Id] bigint NOT NULL IDENTITY,
    [MealId] uniqueidentifier NOT NULL,
    [Position] tinyint NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [Quantity] nvarchar(60) NULL,
    [Kcal] smallint NULL,
    CONSTRAINT [PK_MealItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MealItems_Meal] FOREIGN KEY ([MealId]) REFERENCES [app].[Meals] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[WorkoutTemplates] (
    [Id] int NOT NULL IDENTITY,
    [ProgramId] int NOT NULL,
    [Code] varchar(5) NOT NULL,
    [Name] nvarchar(60) NOT NULL,
    [DurationMinutes] smallint NOT NULL,
    [CardioName] nvarchar(100) NOT NULL,
    [CardioDetail] nvarchar(300) NULL,
    [CardioMinutes] smallint NOT NULL,
    [CardioMet] decimal(4,2) NOT NULL,
    [Position] tinyint NOT NULL,
    CONSTRAINT [PK_WorkoutTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WorkoutTemplates_CardioMet] CHECK ([CardioMet] BETWEEN 1 AND 20),
    CONSTRAINT [FK_WorkoutTemplates_Program] FOREIGN KEY ([ProgramId]) REFERENCES [app].[Programs] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[Workouts] (
    [Id] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [UserId] uniqueidentifier NOT NULL,
    [ProgramId] int NULL,
    [WorkoutTemplateId] int NULL,
    [TemplateCode] varchar(5) NOT NULL,
    [ProgramVersion] int NOT NULL,
    [Date] date NOT NULL,
    [StartedAt] datetimeoffset(0) NULL,
    [DurationMinutes] smallint NULL,
    [WarmupDone] bit NOT NULL,
    [CardioMinutes] smallint NULL,
    [Note] nvarchar(2000) NULL,
    [EstimatedKcal] smallint NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Workouts] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Workouts_Cardio] CHECK ([CardioMinutes] IS NULL OR [CardioMinutes] BETWEEN 0 AND 300),
    CONSTRAINT [CK_Workouts_Duration] CHECK ([DurationMinutes] IS NULL OR [DurationMinutes] BETWEEN 1 AND 600),
    CONSTRAINT [FK_Workouts_Program] FOREIGN KEY ([ProgramId]) REFERENCES [app].[Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Workouts_Template] FOREIGN KEY ([WorkoutTemplateId]) REFERENCES [app].[WorkoutTemplates] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Workouts_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[WorkoutTemplateExercises] (
    [Id] int NOT NULL IDENTITY,
    [WorkoutTemplateId] int NOT NULL,
    [ExerciseId] int NOT NULL,
    [Position] tinyint NOT NULL,
    [Sets] tinyint NOT NULL,
    [RepsMin] tinyint NULL,
    [RepsMax] tinyint NULL,
    [SecondsMin] smallint NULL,
    [SecondsMax] smallint NULL,
    [LoadIncrementKg] decimal(4,2) NULL,
    [RestSec] smallint NULL,
    CONSTRAINT [PK_WorkoutTemplateExercises] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WorkoutTemplateExercises_Reps] CHECK ([RepsMin] IS NULL OR [RepsMax] IS NULL OR [RepsMin] <= [RepsMax]),
    CONSTRAINT [CK_WorkoutTemplateExercises_Seconds] CHECK ([SecondsMin] IS NULL OR [SecondsMax] IS NULL OR [SecondsMin] <= [SecondsMax]),
    CONSTRAINT [CK_WorkoutTemplateExercises_Sets] CHECK ([Sets] BETWEEN 1 AND 10),
    CONSTRAINT [FK_WorkoutTemplateExercises_Exercise] FOREIGN KEY ([ExerciseId]) REFERENCES [app].[Exercises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutTemplateExercises_Template] FOREIGN KEY ([WorkoutTemplateId]) REFERENCES [app].[WorkoutTemplates] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[WorkoutAnalyses] (
    [WorkoutId] uniqueidentifier NOT NULL,
    [Source] varchar(10) NOT NULL,
    [Title] nvarchar(100) NULL,
    [Summary] nvarchar(1000) NULL,
    [Advice] nvarchar(400) NULL,
    [Model] varchar(60) NULL,
    [CreatedAt] datetime2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_WorkoutAnalyses] PRIMARY KEY ([WorkoutId]),
    CONSTRAINT [CK_WorkoutAnalyses_Source] CHECK ([Source] IN ('claude', 'manual')),
    CONSTRAINT [FK_WorkoutAnalyses_Workout] FOREIGN KEY ([WorkoutId]) REFERENCES [app].[Workouts] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[WorkoutExercises] (
    [Id] bigint NOT NULL IDENTITY,
    [WorkoutId] uniqueidentifier NOT NULL,
    [ExerciseId] int NOT NULL,
    [Position] tinyint NOT NULL,
    [ExerciseName] nvarchar(100) NOT NULL,
    [MeasureType] varchar(10) NOT NULL,
    CONSTRAINT [PK_WorkoutExercises] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WorkoutExercises_MeasureType] CHECK ([MeasureType] IN ('load', 'time', 'reps')),
    CONSTRAINT [FK_WorkoutExercises_Exercise] FOREIGN KEY ([ExerciseId]) REFERENCES [app].[Exercises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutExercises_Workout] FOREIGN KEY ([WorkoutId]) REFERENCES [app].[Workouts] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [app].[WorkoutAnalysisItems] (
    [Id] bigint NOT NULL IDENTITY,
    [WorkoutId] uniqueidentifier NOT NULL,
    [ExerciseId] int NOT NULL,
    [Feedback] nvarchar(400) NULL,
    [Instruction] nvarchar(300) NULL,
    [SuggestedLoadKg] decimal(5,2) NULL,
    [SuggestedDurationSec] smallint NULL,
    CONSTRAINT [PK_WorkoutAnalysisItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkoutAnalysisItems_Analysis] FOREIGN KEY ([WorkoutId]) REFERENCES [app].[WorkoutAnalyses] ([WorkoutId]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkoutAnalysisItems_Exercise] FOREIGN KEY ([ExerciseId]) REFERENCES [app].[Exercises] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [app].[WorkoutSets] (
    [Id] bigint NOT NULL IDENTITY,
    [WorkoutExerciseId] bigint NOT NULL,
    [SetNumber] tinyint NOT NULL,
    [LoadKg] decimal(5,2) NULL,
    [Reps] smallint NULL,
    [DurationSec] smallint NULL,
    CONSTRAINT [PK_WorkoutSets] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WorkoutSets_Values] CHECK (([LoadKg] IS NULL OR [LoadKg] BETWEEN 0 AND 500) AND ([Reps] IS NULL OR [Reps] BETWEEN 0 AND 200) AND ([DurationSec] IS NULL OR [DurationSec] BETWEEN 0 AND 3600) AND ([Reps] IS NOT NULL OR [DurationSec] IS NOT NULL)),
    CONSTRAINT [FK_WorkoutSets_WorkoutExercise] FOREIGN KEY ([WorkoutExerciseId]) REFERENCES [app].[WorkoutExercises] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AiUsage_User_CreatedAt] ON [app].[AiUsage] ([UserId], [CreatedAt]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserPasskeys_UserId] ON [AspNetUserPasskeys] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_Exercises_CreatedByUserId] ON [app].[Exercises] ([CreatedByUserId]);
GO


CREATE UNIQUE INDEX [UQ_Exercises_Code] ON [app].[Exercises] ([Code]);
GO


CREATE UNIQUE INDEX [UQ_ExerciseTips_Position] ON [app].[ExerciseTips] ([ExerciseId], [Position]);
GO


CREATE UNIQUE INDEX [UQ_MealItems_Position] ON [app].[MealItems] ([MealId], [Position]);
GO


CREATE INDEX [IX_Meals_User_Date] ON [app].[Meals] ([UserId], [Date]) INCLUDE ([Kcal], [ProteinG], [CarbsG], [FatG], [FiberG], [MealType]);
GO


CREATE UNIQUE INDEX [UQ_Programs_UserVersion] ON [app].[Programs] ([UserId], [Version]);
GO


CREATE UNIQUE INDEX [UX_Programs_OneActivePerUser] ON [app].[Programs] ([UserId]) WHERE [IsActive] = 1;
GO


CREATE UNIQUE INDEX [UQ_WeighIns_UserDate] ON [app].[WeighIns] ([UserId], [Date]);
GO


CREATE INDEX [IX_WorkoutAnalysisItems_ExerciseId] ON [app].[WorkoutAnalysisItems] ([ExerciseId]);
GO


CREATE UNIQUE INDEX [UQ_WorkoutAnalysisItems_Exercise] ON [app].[WorkoutAnalysisItems] ([WorkoutId], [ExerciseId]);
GO


CREATE INDEX [IX_WorkoutExercises_Exercise] ON [app].[WorkoutExercises] ([ExerciseId]) INCLUDE ([WorkoutId]);
GO


CREATE UNIQUE INDEX [UQ_WorkoutExercises_Exercise] ON [app].[WorkoutExercises] ([WorkoutId], [ExerciseId]);
GO


CREATE INDEX [IX_Workouts_ProgramId] ON [app].[Workouts] ([ProgramId]);
GO


CREATE INDEX [IX_Workouts_User_Date] ON [app].[Workouts] ([UserId], [Date] DESC);
GO


CREATE INDEX [IX_Workouts_WorkoutTemplateId] ON [app].[Workouts] ([WorkoutTemplateId]);
GO


CREATE UNIQUE INDEX [UQ_WorkoutSets_Number] ON [app].[WorkoutSets] ([WorkoutExerciseId], [SetNumber]);
GO


CREATE INDEX [IX_WorkoutTemplateExercises_ExerciseId] ON [app].[WorkoutTemplateExercises] ([ExerciseId]);
GO


CREATE UNIQUE INDEX [UQ_WorkoutTemplateExercises_Position] ON [app].[WorkoutTemplateExercises] ([WorkoutTemplateId], [Position]);
GO


CREATE UNIQUE INDEX [UQ_WorkoutTemplates_Code] ON [app].[WorkoutTemplates] ([ProgramId], [Code]);
GO


