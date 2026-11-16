using CarnetDeSalle.Api.Entities;

namespace CarnetDeSalle.Api.Features.WeighIns;

public static class WeightHistory
{
    private const decimal DefaultWeightKg = 80;

    /// <summary>
    /// Obtient le poids applicable à une date donnée à partir d'une séquence de pesées.
    /// </summary>
    /// <remarks>La sélection privilégie une pesée datée au plus tard à la date fournie, puis applique un
    /// repli sur la première pesée disponible.</remarks>
    /// <param name="weighIns">Séquence de pesées utilisée pour déterminer le poids.</param>
    /// <param name="date">Date de référence pour la recherche du poids.</param>
    /// <returns>Le poids de la pesée la plus récente à la date indiquée ou avant celle-ci ; à défaut, le poids de la pesée la
    /// plus ancienne ; si la séquence est vide, la valeur par défaut de poids.</returns>
    public static decimal WeightOn(IEnumerable<WeighIn> weighIns, DateOnly date) =>
        weighIns.Where(s => s.Date <= date).MaxBy(s => s.Date)?.WeightKg ??
        weighIns.MinBy(s => s.Date)?.WeightKg ??
        DefaultWeightKg;
}
