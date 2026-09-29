using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminApproveLyricsSubmissionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminApproveLyricsSubmissionRequestBuilder
{
    private string _slug;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminApproveLyricsSubmissionRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminApproveLyricsSubmissionRequestBuilder()
    {
        _slug = $"submission-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Sets the slug.
    /// </summary>
    /// <param name="slug">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminApproveLyricsSubmissionRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminApproveLyricsSubmissionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminApproveLyricsSubmissionRequest instance.</returns>
    public AdminApproveLyricsSubmissionRequest Build()
    {
        return new AdminApproveLyricsSubmissionRequest(Slug: _slug);
    }
}
