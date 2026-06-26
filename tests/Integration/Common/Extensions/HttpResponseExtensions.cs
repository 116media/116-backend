using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Common.Extensions;

/// <summary>
/// Assertion-friendly helpers for reading and validating HTTP response bodies in
/// integration tests. Encourages typed deserialization into the real production
/// response records instead of stringly-typed <c>JsonDocument</c> inspection.
/// </summary>
public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Deserializes the response body into <typeparamref name="T" /> (a production
    /// response record) and asserts it is non-null.
    /// </summary>
    /// <typeparam name="T">The response type to deserialize into.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The deserialized, non-null body.</returns>
    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        T? value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        value.Should().NotBeNull("the response body should deserialize into {0}", typeof(T).Name);
        return value!;
    }

    /// <summary>
    /// Asserts that the response is an RFC7807 ProblemDetails error with the
    /// expected status code (and, optionally, a matching error code/detail).
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="status">The expected HTTP status code.</param>
    /// <param name="errorCode">
    /// Optional substring expected in the problem's <c>code</c> extension or detail.
    /// </param>
    public static async Task ShouldBeProblem(
        this HttpResponseMessage response,
        HttpStatusCode status,
        string? errorCode = null
    )
    {
        response.StatusCode.Should().Be(status);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem.Should().NotBeNull("error responses should carry a ProblemDetails body");
        problem!.Status.Should().Be((int)status);
        problem.Title.Should().NotBeNullOrWhiteSpace();

        if (errorCode is null)
        {
            return;
        }

        string haystack = problem.Detail ?? string.Empty;
        if (problem.Extensions.TryGetValue("code", out object? code) && code is not null)
        {
            haystack += code.ToString();
        }

        haystack.Should().Contain(errorCode);
    }
}
