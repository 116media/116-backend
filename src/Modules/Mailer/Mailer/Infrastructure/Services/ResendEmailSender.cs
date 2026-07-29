using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;
using Microsoft.Extensions.Configuration;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// Resend HTTP API implementation of <see cref="IEmailSender" /> — a typed
/// <see cref="HttpClient" /> posting to <c>/emails</c>, no SDK package,
/// following the Odesli adapter precedent.
/// </summary>
/// <param name="httpClient">The typed HTTP client used for API calls.</param>
/// <param name="configuration">The configuration providing the API key and sender settings.</param>
public class ResendEmailSender(HttpClient httpClient, IConfiguration configuration) : IEmailSender
{
    /// <summary>
    /// Default Resend API base URL, used when <c>RESEND_API_URL</c> is not configured.
    /// </summary>
    private const string DefaultApiUrl = "https://api.resend.com";

    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        string baseUrl = (configuration["RESEND_API_URL"] ?? DefaultApiUrl).TrimEnd('/');
        string apiKey =
            configuration["RESEND_API_KEY"]
            ?? throw new InvalidOperationException("RESEND_API_KEY env variable is missing or empty.");
        string fromAddress =
            configuration["EMAIL_FROM_ADDRESS"]
            ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS env variable is missing or empty.");
        string fromName = configuration["EMAIL_FROM_NAME"] ?? "116";

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(
            new
            {
                from = $"{fromName} <{fromAddress}>",
                to = new[] { message.To.Address },
                subject = message.Subject,
                html = message.HtmlBody,
                text = message.TextBody,
            }
        );

        HttpResponseMessage response;

        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new EmailDeliveryException(message: $"Resend API unreachable: {exception.Message}");
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new EmailDeliveryException(
                message: $"Resend API answered {(int)response.StatusCode}: {body}",
                isTransient: IsTransient(response.StatusCode)
            );
        }
    }

    /// <summary>
    /// Classifies a Resend failure status: 429 and 5xx are retryable; other
    /// 4xx responses (invalid payload, unverified domain) are permanent.
    /// </summary>
    internal static bool IsTransient(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    }
}
