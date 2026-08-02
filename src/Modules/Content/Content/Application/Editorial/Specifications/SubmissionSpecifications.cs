using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a lyrics submission by its unique identifier.
/// </summary>
public class SubmissionByIdSpecification(Guid id) : Specification<LyricsSubmissionEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsSubmissionEntity, bool>> ToExpression()
    {
        return submission => submission.Id == id;
    }
}

/// <summary>
/// Specification that matches every lyrics submission currently in the given moderation
/// status. Used by the admin review queue's status filter.
/// </summary>
public class SubmissionByStatusSpecification(EnumSubmissionStatus status) : Specification<LyricsSubmissionEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsSubmissionEntity, bool>> ToExpression()
    {
        return submission => submission.Status == status;
    }
}
