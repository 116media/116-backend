using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Session.Ports;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
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

    /// <inheritdoc />
    public bool IsWebClient()
    {
        EnumClient clientApp = sessionMetadataService.ExtractClientApp();
        return WebClients.Contains(item: clientApp);
    }

    /// <inheritdoc />
    public void SetTokenCookies(AuthenticationDto authResult)
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
                Secure = true,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = "/",
                Expires = authResult.AccessTokenExpiresAt,
            }
        );

        if (authResult.RefreshToken is null)
        {
            return;
        }

        httpContext.Response.Cookies.Append(
            key: TokenCookieConstants.RefreshTokenCookie,
            value: authResult.RefreshToken,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = "/",
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
                Secure = true,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = "/",
            }
        );

        httpContext.Response.Cookies.Delete(
            key: TokenCookieConstants.RefreshTokenCookie,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
                Path = "/",
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
    /// Determines whether the application is running in a production environment.
    /// </summary>
    private static bool IsProductionEnvironment()
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        return environment.Equals(value: "Production", comparisonType: StringComparison.OrdinalIgnoreCase);
    }
}
