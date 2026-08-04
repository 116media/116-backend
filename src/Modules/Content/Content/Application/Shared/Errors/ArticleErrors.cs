using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Article domain error factory providing simple, readable exception creation.
/// Usage: ArticleErrors.NotFound(id) or ArticleErrors.SlugAlreadyExists(slug)
/// </summary>
public class ArticleErrors(ArticleErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public ArticleErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when an article is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Article", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when an article with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when an article title is required but not provided.
    /// </summary>
    public BadRequestException TitleRequired()
    {
        return new BadRequestException(i18n.TitleRequired());
    }

    /// <summary>
    /// Throws when an article slug is required but not provided.
    /// </summary>
    public BadRequestException SlugRequired()
    {
        return new BadRequestException(i18n.SlugRequired());
    }

    /// <summary>
    /// Throws when an article is already pending payment.
    /// </summary>
    public ConflictException AlreadySubmitted()
    {
        return new ConflictException(i18n.AlreadySubmitted());
    }

    /// <summary>
    /// Throws when an article is already pending review.
    /// </summary>
    public ConflictException AlreadyPendingReview()
    {
        return new ConflictException(i18n.AlreadyPendingReview());
    }

    /// <summary>
    /// Throws when an article is already approved.
    /// </summary>
    public ConflictException AlreadyApproved()
    {
        return new ConflictException(i18n.AlreadyApproved());
    }

    /// <summary>
    /// Throws when an article is already published.
    /// </summary>
    public ConflictException AlreadyPublished()
    {
        return new ConflictException(i18n.AlreadyPublished());
    }

    /// <summary>
    /// Throws when an article is already rejected.
    /// </summary>
    public ConflictException AlreadyRejected()
    {
        return new ConflictException(i18n.AlreadyRejected());
    }

    /// <summary>
    /// Throws when an article is already archived.
    /// </summary>
    public ConflictException AlreadyArchived()
    {
        return new ConflictException(i18n.AlreadyArchived());
    }

    /// <summary>
    /// Throws when an invalid status transition is attempted.
    /// </summary>
    public BadRequestException InvalidStatusTransition(string from, string to)
    {
        return new BadRequestException(i18n.InvalidStatusTransition(from: from, to: to));
    }

    /// <summary>
    /// Throws when a hard delete is attempted on a published or approved article.
    /// </summary>
    public BadRequestException CannotDeletePublishedArticle()
    {
        return new BadRequestException(i18n.CannotDeletePublishedArticle());
    }

    /// <summary>
    /// Throws when an article carries no active promotion.
    /// </summary>
    public BadRequestException NotPromoted()
    {
        return new BadRequestException(i18n.NotPromoted());
    }
}
