using System.Security.Claims;
using IdentityService.ViewModels;
using Microsoft.AspNetCore;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Text.Json;

namespace IdentityService.Presentation.Controllers
{
    public class AuthorizationController(IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager) : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager = applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager = authorizationManager;

        [HttpGet("~/connect/authorize")]
        public  ActionResult Authorize()
        {
            return View("Authorize", new AuthorizeViewModel
            {
                ApplicationName = "Starkeeper",
                Scope = "openid profile email"
            });
        }

        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Accept()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the profile of the logged-in user.
            HttpClient httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            });
            httpClient.BaseAddress = new Uri("http://localhost:5057/users");
            var response = await httpClient.GetAsync("");
            var content = await response.Content.ReadAsStringAsync();
            dynamic? user = JsonDocument.Parse(content).RootElement;

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            var authorizations = new List<object?>();

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var claims = new List<Claim>
            {
                new Claim(Claims.Subject, user.GetProperty("id").GetString()),
            };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            // Create a new ClaimsPrincipal holding the ClaimsIdentity.
            var principal = new ClaimsPrincipal(identity);

            // Set the list of scopes granted to the client application.
            principal.SetScopes(new[]
            {
                Scopes.OpenId,
                Scopes.Email,
                Scopes.Profile,
                Scopes.OfflineAccess
            }.Intersect(request.GetScopes()));

            var authorization = authorizations.LastOrDefault();

            authorization ??= await _authorizationManager.CreateAsync(
                identity: identity,
                subject: user.GetProperty("id").GetString(),
                client: (await _applicationManager.GetIdAsync(application))!,
                type: AuthorizationTypes.Permanent,
                scopes: identity.GetScopes());

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            // Signing in the user will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
    }
}
