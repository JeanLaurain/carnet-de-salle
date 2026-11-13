using System.Security.Claims;

namespace CarnetDeSalle.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id de l'utilisateur connecté (claim NameIdentifier), posé par le cookie de connexion (#27)
    /// ou par TestAuthHandler dans les tests.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        // 1. Lire le claim : user.FindFirstValue(ClaimTypes.NameIdentifier).
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. Le convertir en Guid (Guid.TryParse).
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        // 3. Absent ou invalide : lever une InvalidOperationException. La route exige d'être connecté,
        //    donc ce cas est un bug, pas une erreur de l'utilisateur.
        throw new InvalidOperationException("User ID claim is missing or invalid.");
    }
}
