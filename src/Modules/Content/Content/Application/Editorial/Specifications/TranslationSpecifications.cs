using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a lyrics translation by its unique identifier.
/// </summary>
public class TranslationByIdSpecification(Guid id) : Specification<LyricsTranslationEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationEntity, bool>> ToExpression()
    {
        return translation => translation.Id == id;
    }
}

/// <summary>
/// Specification that matches a lyrics translation by its lyrics page and language pair —
/// the unique key backing the idempotent AI-translation request flow.
/// </summary>
public class TranslationByLyricsAndLanguageSpecification(Guid lyricsId, string language)
    : Specification<LyricsTranslationEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationEntity, bool>> ToExpression()
    {
        return translation => translation.LyricsId == lyricsId && translation.Language == language;
    }
}

/// <summary>
/// Specification that matches every translation belonging to a lyrics page, across all
/// languages.
/// </summary>
public class TranslationByLyricsIdSpecification(Guid lyricsId) : Specification<LyricsTranslationEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationEntity, bool>> ToExpression()
    {
        return translation => translation.LyricsId == lyricsId;
    }
}

/// <summary>
/// Specification that matches a translation revision by its unique identifier.
/// </summary>
public class TranslationRevisionByIdSpecification(Guid id) : Specification<LyricsTranslationRevisionEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationRevisionEntity, bool>> ToExpression()
    {
        return revision => revision.Id == id;
    }
}

/// <summary>
/// Specification that matches every revision — pending, accepted, or rejected — proposed
/// against a given translation. Used to render a translation's full review history.
/// </summary>
public class TranslationRevisionByTranslationIdSpecification(Guid translationId)
    : Specification<LyricsTranslationRevisionEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationRevisionEntity, bool>> ToExpression()
    {
        return revision => revision.TranslationId == translationId;
    }
}

/// <summary>
/// Specification that matches a translation vote by its revision and voter, used to pre-check
/// for a duplicate vote before the DB-level unique constraint backstop rejects it.
/// </summary>
public class TranslationVoteByRevisionAndUserSpecification(Guid revisionId, Guid userId)
    : Specification<LyricsTranslationVoteEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationVoteEntity, bool>> ToExpression()
    {
        return vote => vote.RevisionId == revisionId && vote.UserId == userId;
    }
}

/// <summary>
/// Specification that matches every vote cast on a given translation revision, used to tally
/// net approvals.
/// </summary>
public class TranslationVoteByRevisionIdSpecification(Guid revisionId) : Specification<LyricsTranslationVoteEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsTranslationVoteEntity, bool>> ToExpression()
    {
        return vote => vote.RevisionId == revisionId;
    }
}
