using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarnetDeSalle.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiUsage",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Feature = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Model = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiUsage_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "varbinary(1024)", maxLength: 1024, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Muscles = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MeasureType = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Alternative = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AlternativeLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    VideoSearch = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IllustrationKey = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.CheckConstraint("CK_Exercises_MeasureType", "[MeasureType] IN ('load', 'time', 'reps')");
                    table.ForeignKey(
                        name: "FK_Exercises_CreatedBy",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MealType = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PortionText = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Servings = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    Kcal = table.Column<short>(type: "smallint", nullable: false),
                    ProteinG = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    CarbsG = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: true),
                    FatG = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: true),
                    FiberG = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: true),
                    Confidence = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Source = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                    table.CheckConstraint("CK_Meals_Confidence", "[Confidence] IS NULL OR [Confidence] IN ('high', 'medium', 'low')");
                    table.CheckConstraint("CK_Meals_Kcal", "[Kcal] BETWEEN 0 AND 10000");
                    table.CheckConstraint("CK_Meals_MealType", "[MealType] IN ('breakfast', 'lunch', 'snack', 'dinner')");
                    table.CheckConstraint("CK_Meals_Servings", "[Servings] > 0 AND [Servings] <= 12");
                    table.CheckConstraint("CK_Meals_Source", "[Source] IN ('claude', 'photo', 'manual', 'recent')");
                    table.ForeignKey(
                        name: "FK_Meals_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                schema: "app",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sex = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    HeightCm = table.Column<short>(type: "smallint", nullable: false),
                    ActivityFactor = table.Column<decimal>(type: "decimal(4,3)", precision: 4, scale: 3, nullable: false),
                    DailyKcalOffset = table.Column<short>(type: "smallint", nullable: false),
                    ProteinGPerKg = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    FatShareOfKcal = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    FiberTargetG = table.Column<short>(type: "smallint", nullable: false),
                    TimeZone = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                    table.CheckConstraint("CK_Profiles_ActivityFactor", "[ActivityFactor] BETWEEN 1.1 AND 2.0");
                    table.CheckConstraint("CK_Profiles_DailyKcalOffset", "[DailyKcalOffset] BETWEEN -1000 AND 1000");
                    table.CheckConstraint("CK_Profiles_FatShareOfKcal", "[FatShareOfKcal] BETWEEN 0.15 AND 0.45");
                    table.CheckConstraint("CK_Profiles_FiberTargetG", "[FiberTargetG] BETWEEN 10 AND 80");
                    table.CheckConstraint("CK_Profiles_HeightCm", "[HeightCm] BETWEEN 130 AND 220");
                    table.CheckConstraint("CK_Profiles_ProteinGPerKg", "[ProteinGPerKg] BETWEEN 0.8 AND 3.0");
                    table.CheckConstraint("CK_Profiles_Sex", "[Sex] IN ('M', 'F')");
                    table.ForeignKey(
                        name: "FK_Profiles_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gym = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultRestSec = table.Column<short>(type: "smallint", nullable: false),
                    WarmupTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WarmupDetail = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WarmupMinutes = table.Column<byte>(type: "tinyint", nullable: false),
                    ChangeNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedWeeks = table.Column<byte>(type: "tinyint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.CheckConstraint("CK_Programs_ChangeNotes", "[ChangeNotes] IS NULL OR ISJSON([ChangeNotes]) = 1");
                    table.CheckConstraint("CK_Programs_PlannedWeeks", "[PlannedWeeks] IS NULL OR [PlannedWeeks] BETWEEN 1 AND 52");
                    table.ForeignKey(
                        name: "FK_Programs_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeighIns",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    WaistCm = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeighIns", x => x.Id);
                    table.CheckConstraint("CK_WeighIns_WaistCm", "[WaistCm] IS NULL OR [WaistCm] BETWEEN 40 AND 200");
                    table.CheckConstraint("CK_WeighIns_WeightKg", "[WeightKg] BETWEEN 30 AND 250");
                    table.ForeignKey(
                        name: "FK_WeighIns_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseTips",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseTips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseTips_Exercise",
                        column: x => x.ExerciseId,
                        principalSchema: "app",
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealItems",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Quantity = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Kcal = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealItems_Meal",
                        column: x => x.MealId,
                        principalSchema: "app",
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplates",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DurationMinutes = table.Column<short>(type: "smallint", nullable: false),
                    CardioName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardioDetail = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CardioMinutes = table.Column<short>(type: "smallint", nullable: false),
                    CardioMet = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplates", x => x.Id);
                    table.CheckConstraint("CK_WorkoutTemplates_CardioMet", "[CardioMet] BETWEEN 1 AND 20");
                    table.ForeignKey(
                        name: "FK_WorkoutTemplates_Program",
                        column: x => x.ProgramId,
                        principalSchema: "app",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workouts",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: true),
                    WorkoutTemplateId = table.Column<int>(type: "int", nullable: true),
                    TemplateCode = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    ProgramVersion = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    DurationMinutes = table.Column<short>(type: "smallint", nullable: true),
                    WarmupDone = table.Column<bool>(type: "bit", nullable: false),
                    CardioMinutes = table.Column<short>(type: "smallint", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedKcal = table.Column<short>(type: "smallint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.Id);
                    table.CheckConstraint("CK_Workouts_Cardio", "[CardioMinutes] IS NULL OR [CardioMinutes] BETWEEN 0 AND 300");
                    table.CheckConstraint("CK_Workouts_Duration", "[DurationMinutes] IS NULL OR [DurationMinutes] BETWEEN 1 AND 600");
                    table.ForeignKey(
                        name: "FK_Workouts_Program",
                        column: x => x.ProgramId,
                        principalSchema: "app",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workouts_Template",
                        column: x => x.WorkoutTemplateId,
                        principalSchema: "app",
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workouts_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplateExercises",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutTemplateId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false),
                    Sets = table.Column<byte>(type: "tinyint", nullable: false),
                    RepsMin = table.Column<byte>(type: "tinyint", nullable: true),
                    RepsMax = table.Column<byte>(type: "tinyint", nullable: true),
                    SecondsMin = table.Column<short>(type: "smallint", nullable: true),
                    SecondsMax = table.Column<short>(type: "smallint", nullable: true),
                    LoadIncrementKg = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    RestSec = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplateExercises", x => x.Id);
                    table.CheckConstraint("CK_WorkoutTemplateExercises_Reps", "[RepsMin] IS NULL OR [RepsMax] IS NULL OR [RepsMin] <= [RepsMax]");
                    table.CheckConstraint("CK_WorkoutTemplateExercises_Seconds", "[SecondsMin] IS NULL OR [SecondsMax] IS NULL OR [SecondsMin] <= [SecondsMax]");
                    table.CheckConstraint("CK_WorkoutTemplateExercises_Sets", "[Sets] BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateExercises_Exercise",
                        column: x => x.ExerciseId,
                        principalSchema: "app",
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateExercises_Template",
                        column: x => x.WorkoutTemplateId,
                        principalSchema: "app",
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutAnalyses",
                schema: "app",
                columns: table => new
                {
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Advice = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Model = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutAnalyses", x => x.WorkoutId);
                    table.CheckConstraint("CK_WorkoutAnalyses_Source", "[Source] IN ('claude', 'manual')");
                    table.ForeignKey(
                        name: "FK_WorkoutAnalyses_Workout",
                        column: x => x.WorkoutId,
                        principalSchema: "app",
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutExercises",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false),
                    ExerciseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MeasureType = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExercises", x => x.Id);
                    table.CheckConstraint("CK_WorkoutExercises_MeasureType", "[MeasureType] IN ('load', 'time', 'reps')");
                    table.ForeignKey(
                        name: "FK_WorkoutExercises_Exercise",
                        column: x => x.ExerciseId,
                        principalSchema: "app",
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutExercises_Workout",
                        column: x => x.WorkoutId,
                        principalSchema: "app",
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutAnalysisItems",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SuggestedLoadKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    SuggestedDurationSec = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutAnalysisItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutAnalysisItems_Analysis",
                        column: x => x.WorkoutId,
                        principalSchema: "app",
                        principalTable: "WorkoutAnalyses",
                        principalColumn: "WorkoutId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutAnalysisItems_Exercise",
                        column: x => x.ExerciseId,
                        principalSchema: "app",
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSets",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutExerciseId = table.Column<long>(type: "bigint", nullable: false),
                    SetNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    LoadKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Reps = table.Column<short>(type: "smallint", nullable: true),
                    DurationSec = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSets", x => x.Id);
                    table.CheckConstraint("CK_WorkoutSets_Values", "([LoadKg] IS NULL OR [LoadKg] BETWEEN 0 AND 500) AND ([Reps] IS NULL OR [Reps] BETWEEN 0 AND 200) AND ([DurationSec] IS NULL OR [DurationSec] BETWEEN 0 AND 3600) AND ([Reps] IS NOT NULL OR [DurationSec] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_WorkoutSets_WorkoutExercise",
                        column: x => x.WorkoutExerciseId,
                        principalSchema: "app",
                        principalTable: "WorkoutExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsage_User_CreatedAt",
                schema: "app",
                table: "AiUsage",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedByUserId",
                schema: "app",
                table: "Exercises",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Exercises_Code",
                schema: "app",
                table: "Exercises",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ExerciseTips_Position",
                schema: "app",
                table: "ExerciseTips",
                columns: new[] { "ExerciseId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MealItems_Position",
                schema: "app",
                table: "MealItems",
                columns: new[] { "MealId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meals_User_Date",
                schema: "app",
                table: "Meals",
                columns: new[] { "UserId", "Date" })
                .Annotation("SqlServer:Include", new[] { "Kcal", "ProteinG", "CarbsG", "FatG", "FiberG", "MealType" });

            migrationBuilder.CreateIndex(
                name: "UQ_Programs_UserVersion",
                schema: "app",
                table: "Programs",
                columns: new[] { "UserId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Programs_OneActivePerUser",
                schema: "app",
                table: "Programs",
                column: "UserId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "UQ_WeighIns_UserDate",
                schema: "app",
                table: "WeighIns",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutAnalysisItems_ExerciseId",
                schema: "app",
                table: "WorkoutAnalysisItems",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "UQ_WorkoutAnalysisItems_Exercise",
                schema: "app",
                table: "WorkoutAnalysisItems",
                columns: new[] { "WorkoutId", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_Exercise",
                schema: "app",
                table: "WorkoutExercises",
                column: "ExerciseId")
                .Annotation("SqlServer:Include", new[] { "WorkoutId" });

            migrationBuilder.CreateIndex(
                name: "UQ_WorkoutExercises_Exercise",
                schema: "app",
                table: "WorkoutExercises",
                columns: new[] { "WorkoutId", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_ProgramId",
                schema: "app",
                table: "Workouts",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_User_Date",
                schema: "app",
                table: "Workouts",
                columns: new[] { "UserId", "Date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_WorkoutTemplateId",
                schema: "app",
                table: "Workouts",
                column: "WorkoutTemplateId");

            migrationBuilder.CreateIndex(
                name: "UQ_WorkoutSets_Number",
                schema: "app",
                table: "WorkoutSets",
                columns: new[] { "WorkoutExerciseId", "SetNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplateExercises_ExerciseId",
                schema: "app",
                table: "WorkoutTemplateExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "UQ_WorkoutTemplateExercises_Position",
                schema: "app",
                table: "WorkoutTemplateExercises",
                columns: new[] { "WorkoutTemplateId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WorkoutTemplates_Code",
                schema: "app",
                table: "WorkoutTemplates",
                columns: new[] { "ProgramId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiUsage",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ExerciseTips",
                schema: "app");

            migrationBuilder.DropTable(
                name: "MealItems",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Profiles",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WeighIns",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkoutAnalysisItems",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkoutSets",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkoutTemplateExercises",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Meals",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkoutAnalyses",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkoutExercises",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Exercises",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Workouts",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkoutTemplates",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Programs",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
