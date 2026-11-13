using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarnetDeSalle.Api.Tests;

/// <summary>
/// Connexion factice pour les tests : l'en-tête X-Test-User donne l'Id de l'utilisateur connecté.
/// Sans l'en-tête, personne n'est connecté et les routes protégées répondent 401.
/// Remplace le cookie de connexion (#27), que les tests n'ont pas à simuler.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers[UserHeader].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Le même claim que posera le cookie : ClaimsPrincipalExtensions.GetUserId le lit
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
