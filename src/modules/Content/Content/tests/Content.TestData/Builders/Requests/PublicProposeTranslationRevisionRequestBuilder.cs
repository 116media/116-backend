using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicProposeTranslationRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicProposeTranslationRevisionRequestBuilder
{
    private string _proposedText;
    private string? _editSummary;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicProposeTranslationRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicProposeTranslationRevisionRequestBuilder()
    {
        _proposedText = "A better translation";
        _editSummary = null;
    }

    /// <summary>
    /// Sets the proposed text.
    /// </summary>
    /// <param name="proposedText">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicProposeTranslationRevisionRequestBuilder WithProposedText(string proposedText)
    {
        _proposedText = proposedText;
        return this;
    }

    /// <summary>
    /// Sets the edit summary.
    /// </summary>
    /// <param name="editSummary">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicProposeTranslationRevisionRequestBuilder WithEditSummary(string? editSummary)
    {
        _editSummary = editSummary;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicProposeTranslationRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicProposeTranslationRevisionRequest instance.</returns>
    public PublicProposeTranslationRevisionRequest Build()
    {
        return new PublicProposeTranslationRevisionRequest(ProposedText: _proposedText, EditSummary: _editSummary);
    }
}
