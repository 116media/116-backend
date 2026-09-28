using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.V1;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicVoteOnTranslationRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicVoteOnTranslationRevisionRequestBuilder
{
    private EnumVote _vote;
    private string? _comment;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicVoteOnTranslationRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicVoteOnTranslationRevisionRequestBuilder()
    {
        _vote = EnumVote.Approve;
        _comment = null;
    }

    /// <summary>
    /// Sets the vote.
    /// </summary>
    /// <param name="vote">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicVoteOnTranslationRevisionRequestBuilder WithVote(EnumVote vote)
    {
        _vote = vote;
        return this;
    }

    /// <summary>
    /// Sets the comment.
    /// </summary>
    /// <param name="comment">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicVoteOnTranslationRevisionRequestBuilder WithComment(string? comment)
    {
        _comment = comment;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicVoteOnTranslationRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicVoteOnTranslationRevisionRequest instance.</returns>
    public PublicVoteOnTranslationRevisionRequest Build()
    {
        return new PublicVoteOnTranslationRevisionRequest(Vote: _vote, Comment: _comment);
    }
}
