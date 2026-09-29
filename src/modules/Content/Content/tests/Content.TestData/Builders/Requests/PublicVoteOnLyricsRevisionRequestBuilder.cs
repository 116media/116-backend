using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.V1;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicVoteOnLyricsRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicVoteOnLyricsRevisionRequestBuilder
{
    private EnumVote _vote;
    private string? _comment;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicVoteOnLyricsRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicVoteOnLyricsRevisionRequestBuilder()
    {
        _vote = EnumVote.Approve;
        _comment = null;
    }

    /// <summary>
    /// Sets the vote.
    /// </summary>
    /// <param name="vote">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicVoteOnLyricsRevisionRequestBuilder WithVote(EnumVote vote)
    {
        _vote = vote;
        return this;
    }

    /// <summary>
    /// Sets the comment.
    /// </summary>
    /// <param name="comment">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicVoteOnLyricsRevisionRequestBuilder WithComment(string? comment)
    {
        _comment = comment;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicVoteOnLyricsRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicVoteOnLyricsRevisionRequest instance.</returns>
    public PublicVoteOnLyricsRevisionRequest Build()
    {
        return new PublicVoteOnLyricsRevisionRequest(Vote: _vote, Comment: _comment);
    }
}
