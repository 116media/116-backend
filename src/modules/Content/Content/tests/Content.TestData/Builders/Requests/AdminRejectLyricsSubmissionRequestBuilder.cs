using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminRejectLyricsSubmissionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminRejectLyricsSubmissionRequestBuilder
{
    private string _note;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRejectLyricsSubmissionRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminRejectLyricsSubmissionRequestBuilder()
    {
        _note = "Not a good fit.";
    }

    /// <summary>
    /// Sets the note.
    /// </summary>
    /// <param name="note">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminRejectLyricsSubmissionRequestBuilder WithNote(string note)
    {
        _note = note;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminRejectLyricsSubmissionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminRejectLyricsSubmissionRequest instance.</returns>
    public AdminRejectLyricsSubmissionRequest Build()
    {
        return new AdminRejectLyricsSubmissionRequest(Note: _note);
    }
}
