using _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminRequestLyricsSubmissionRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminRequestLyricsSubmissionRevisionRequestBuilder
{
    private string _note;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRequestLyricsSubmissionRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminRequestLyricsSubmissionRevisionRequestBuilder()
    {
        _note = "Please fix formatting.";
    }

    /// <summary>
    /// Sets the note.
    /// </summary>
    /// <param name="note">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminRequestLyricsSubmissionRevisionRequestBuilder WithNote(string note)
    {
        _note = note;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminRequestLyricsSubmissionRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminRequestLyricsSubmissionRevisionRequest instance.</returns>
    public AdminRequestLyricsSubmissionRevisionRequest Build()
    {
        return new AdminRequestLyricsSubmissionRevisionRequest(Note: _note);
    }
}
