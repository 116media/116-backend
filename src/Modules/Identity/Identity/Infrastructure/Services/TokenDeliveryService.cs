using System.Text.RegularExpressions;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.Services;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Manages token delivery based on client type.
/// Web clients (Dashboard, WebApp) receive tokens via HttpOnly cookies,
/// while mobile clients receive tokens in the JSON response body.
/// </summary>
/// <param name="httpContextAccessor">
/// Accessor for the current HTTP context.
/// </param>
/// <param name="sessionMetadataService">
/// Service for extracting client application type from request headers.
/// </param>
public class TokenDeliveryService(
    IHttpContextAccessor httpContextAccessor,
    ISessionMetadataService sessionMetadataService
) : ITokenDeliveryService
{
    /// <summary>
    /// Client types that receive tokens via HttpOnly cookies.
    /// </summary>
    private static readonly HashSet<EnumClient> WebClients = [EnumClient.Dashboard, EnumClient.WebApp];

    /// <summary>
    /// Pattern to extract the API version prefix from the request path.
    /// Matches segments like "api/v1", "api/v2", etc.
    /// </summary>
    private static readonly Regex ApiVersionPattern = new(@"^(/api/v\d+)", RegexOptions.Compiled);

    /// <inheritdoc />
    public bool IsWebClient()
    {
        EnumClient clientApp = sessionMetadataService.ExtractClientApp();
        return WebClients.Contains(item: clientApp);
    }

    /// <inheritdoc />
    public void SetTokenCookies(AuthenticationResult authResult)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        bool isProduction = IsProductionEnvironment();

        httpContext.Response.Cookies.Append(
            key: TokenCookieConstants.AccessTokenCookie,
            value: authResult.AccessToken,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = "/",
                Expires = authResult.AccessTokenExpiresAt,
            }
        );

        httpContext.Response.Cookies.Append(
            key: TokenCookieConstants.RefreshTokenCookie,
            value: authResult.RefreshToken,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = BuildRefreshTokenCookiePath(),
                Expires = authResult.RefreshTokenExpiresAt,
            }
        );
    }

    /// <inheritdoc />
    public void ClearTokenCookies()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        bool isProduction = IsProductionEnvironment();

        httpContext.Response.Cookies.Delete(
            key: TokenCookieConstants.AccessTokenCookie,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = "/",
            }
        );

        httpContext.Response.Cookies.Delete(
            key: TokenCookieConstants.RefreshTokenCookie,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = BuildRefreshTokenCookiePath(),
            }
        );
    }

    /// <inheritdoc />
    public string? ReadRefreshToken(string? bodyRefreshToken)
    {
        if (!IsWebClient())
        {
            return bodyRefreshToken;
        }

        return httpContextAccessor.HttpContext?.Request.Cookies[TokenCookieConstants.RefreshTokenCookie];
    }

    /// <summary>
    /// Builds the refresh token cookie path dynamically from the current request path.
    /// Extracts the API version prefix and combines it with the scope and session endpoint.
    /// Example: /api/v1/public/sessions or /api/v1/admin/sessions.
    /// </summary>
    private string BuildRefreshTokenCookiePath()
    {
        string? requestPath = httpContextAccessor.HttpContext?.Request.Path.Value;
        string apiPrefix = ExtractApiVersionPrefix(requestPath: requestPath);
        string scope = IsAdminClient() ? IdentityConstants.Admin : IdentityConstants.Public;

        return $"{apiPrefix}/{scope}/{SessionRouteConstants.Endpoint}";
    }

    /// <summary>
    /// Extracts the API version prefix from the request path.
    /// </summary>
    /// <param name="requestPath">
    /// The full request path (e.g., /api/v1/public/auth/login).
    /// </param>
    /// <returns>
    /// The version prefix (e.g., /api/v1). Falls back to /api/v1 if not matched.
    /// </returns>
    private static string ExtractApiVersionPrefix(string? requestPath)
    {
        if (string.IsNullOrEmpty(requestPath))
        {
            return "/api/v1";
        }

        Match match = ApiVersionPattern.Match(requestPath);
        return match.Success ? match.Groups[1].Value : "/api/v1";
    }

    /// <summary>
    /// Determines whether the current request originates from an admin client (Dashboard).
    /// </summary>
    private bool IsAdminClient()
    {
        EnumClient clientApp = sessionMetadataService.ExtractClientApp();
        return clientApp == EnumClient.Dashboard;
    }

    /// <summary>
    /// Determines whether the application is running in a production environment.
    /// </summary>
    private static bool IsProductionEnvironment()
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        return environment.Equals(value: "Production", comparisonType: StringComparison.OrdinalIgnoreCase);
    }
}
