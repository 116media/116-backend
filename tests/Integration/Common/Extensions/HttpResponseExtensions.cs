using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Common.Extensions;

/// <summary>
/// Assertion-friendly helpers for reading and validating HTTP response bodies in
/// integration tests. Encourages typed deserialization into the real production
/// response records instead of stringly-typed <c>JsonDocument</c> inspection.
/// </summary>
public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

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
    /// Asserts that the response is an RFC7807 ProblemDetails error with the expected status
    /// code. This overload pins the status and the ProblemDetails shape only, so it cannot tell
    /// two guards behind the same status apart; prefer the
    /// <see cref="ShouldBeProblem{TException}" /> overload whenever the endpoint can reach this
    /// status from more than one guard.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="status">The expected HTTP status code.</param>
    /// <param name="allowEmptyBody">
    /// Set only for responses the framework produces before the exception middleware runs,
    /// such as multipart model-binding failures.
    /// </param>
    public static async Task ShouldBeProblem(
        this HttpResponseMessage response,
        HttpStatusCode status,
        bool allowEmptyBody = false
    )
    {
        response.StatusCode.Should().Be(status);

        string raw = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(raw))
        {
            allowEmptyBody
                .Should()
                .BeTrue(
                    "every error this application raises is translated to ProblemDetails by the "
                        + "global exception middleware; an empty body means that did not happen"
                );
            return;
        }

        ProblemDetails problem = ParseProblem(raw, status);
        problem.Title.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Asserts that the response is the ProblemDetails the global exception middleware produces
    /// for <typeparamref name="TException" />, pinning the status code, the problem
    /// <c>Title</c> (which every strategy sets to <c>nameof(TException)</c>) and the exact
    /// localized <c>Detail</c>.
    /// </summary>
    /// <typeparam name="TException">
    /// The exception type the endpoint is expected to have raised. It supplies the expected
    /// title, so renaming the exception breaks the test at compile time rather than silently.
    /// </typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="status">The expected HTTP status code.</param>
    /// <param name="detail">
    /// The exact expected detail. Resolve it through <c>BaseApiTest.Localized</c> rather than
    /// hardcoding the sentence, so the assertion survives .resx copy edits and holds under
    /// every supported <c>Accept-Language</c>.
    /// </param>
    public static async Task ShouldBeProblem<TException>(
        this HttpResponseMessage response,
        HttpStatusCode status,
        string detail
    )
        where TException : Exception
    {
        response.StatusCode.Should().Be(status);

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should()
            .NotBeNullOrWhiteSpace(
                "a {0} is translated to a ProblemDetails body by the global exception middleware",
                typeof(TException).Name
            );

        ProblemDetails problem = ParseProblem(raw, status);

        problem
            .Title.Should()
            .Be(
                typeof(TException).Name,
                "every exception strategy titles the problem after the exception type it handles"
            );

        problem.Detail.Should().Be(detail, "the detail is what tells two guards behind the same status code apart");
    }

    /// <summary>
    /// Deserializes a non-empty error body into <see cref="ProblemDetails" /> and asserts the
    /// envelope agrees with the transport status code.
    /// </summary>
    /// <param name="raw">The raw, non-empty response body.</param>
    /// <param name="status">The expected HTTP status code.</param>
    /// <returns>The deserialized, non-null problem.</returns>
    private static ProblemDetails ParseProblem(string raw, HttpStatusCode status)
    {
        ProblemDetails? problem = JsonSerializer.Deserialize<ProblemDetails>(raw, JsonOptions);
        problem.Should().NotBeNull("a non-empty error response should be a ProblemDetails body");
        problem!.Status.Should().Be((int)status);

        return problem;
    }
}
