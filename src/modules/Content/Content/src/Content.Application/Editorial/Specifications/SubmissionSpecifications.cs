using System.Linq.Expressions;
using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.Specifications;

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
