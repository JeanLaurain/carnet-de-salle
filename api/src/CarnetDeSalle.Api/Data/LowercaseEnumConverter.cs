using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CarnetDeSalle.Api.Data;

/// <summary>
/// Stocke un enum en texte minuscule (MeasureType.Load → « load »), comme l'attendent
/// les contraintes CHECK du schéma.
/// </summary>
public sealed class LowercaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    value => value.ToString().ToLowerInvariant(),
    text => Enum.Parse<TEnum>(text, true))
    where TEnum : struct, Enum
{
}
