namespace CarnetDeSalle.Api.Features.Workouts;

public sealed record SetEffort(
    short? DurationSec,
    short RestSec);

public sealed record WorkoutEnergyInput(
    decimal WeightKg,
    bool WarmupDone,
    short? CardioMinutes,
    decimal CardioMet,
    short? DurationMinutes,
    IReadOnlyList<SetEffort> Sets);

public sealed record WorkoutEnergyResult(
    int StrengthKcal,
    int WarmupKcal,
    int CardioKcal,
    int TotalKcal);
