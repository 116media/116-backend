using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicProposeLyricsRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicProposeLyricsRevisionRequestBuilder
{
    private string _proposedText;
    private string? _editSummary;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicProposeLyricsRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicProposeLyricsRevisionRequestBuilder()
    {
        _proposedText = "Corrected lyrics text";
        _editSummary = null;
    }

    /// <summary>
    /// Sets the proposed text.
    /// </summary>
    /// <param name="proposedText">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicProposeLyricsRevisionRequestBuilder WithProposedText(string proposedText)
    {
        _proposedText = proposedText;
        return this;
    }

    /// <summary>
    /// Sets the edit summary.
    /// </summary>
    /// <param name="editSummary">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicProposeLyricsRevisionRequestBuilder WithEditSummary(string? editSummary)
    {
        _editSummary = editSummary;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicProposeLyricsRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicProposeLyricsRevisionRequest instance.</returns>
    public PublicProposeLyricsRevisionRequest Build()
    {
        return new PublicProposeLyricsRevisionRequest(ProposedText: _proposedText, EditSummary: _editSummary);
    }
}
