using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a lyrics-text correction revision by its unique identifier.
/// </summary>
public class LyricsRevisionByIdSpecification(Guid id) : Specification<LyricsRevisionEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsRevisionEntity, bool>> ToExpression()
    {
        return revision => revision.Id == id;
    }
}

/// <summary>
/// Specification that matches a lyrics revision vote by its revision and voter, used to
/// pre-check for a duplicate vote before the DB-level unique constraint backstop rejects it.
/// </summary>
public class LyricsRevisionVoteByRevisionAndUserSpecification(Guid revisionId, Guid userId)
    : Specification<LyricsRevisionVoteEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsRevisionVoteEntity, bool>> ToExpression()
    {
        return vote => vote.RevisionId == revisionId && vote.UserId == userId;
    }
}

/// <summary>
/// Specification that matches every vote cast on a given lyrics revision, used to tally net
/// approvals.
/// </summary>
public class LyricsRevisionVoteByRevisionIdSpecification(Guid revisionId) : Specification<LyricsRevisionVoteEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsRevisionVoteEntity, bool>> ToExpression()
    {
        return vote => vote.RevisionId == revisionId;
    }
}
