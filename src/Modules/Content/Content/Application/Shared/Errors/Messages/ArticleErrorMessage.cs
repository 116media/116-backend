using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Article</c> domain.
/// Covers conflict situations and validation failures related to article operations.
/// </summary>
public class ArticleErrorMessage(IStringLocalizer<ArticleErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when an article with the given slug already exists.
    /// </summary>
    /// <param name="slug">The article slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that an article with the specified slug already exists.
    /// </returns>
    public string SlugAlreadyExists(string slug)
    {
        return string.Format(localizer["SlugAlreadyExists"], slug);
    }

    /// <summary>
    /// Gets an error message for when an article title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the article title is required.
    /// </returns>
    public string TitleRequired()
    {
        return localizer["TitleRequired"];
    }

    /// <summary>
    /// Gets an error message for when an article slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the article slug is required.
    /// </returns>
    public string SlugRequired()
    {
        return localizer["SlugRequired"];
    }

    /// <summary>
    /// Gets an error message for when an invalid status transition is attempted.
    /// </summary>
    /// <param name="from">The current status of the article.</param>
    /// <param name="to">The target status that the transition was attempted towards.</param>
    /// <returns>
    /// A formatted error message indicating that the transition from the current status to the target status is not allowed.
    /// </returns>
    public string InvalidStatusTransition(string from, string to)
    {
        return string.Format(localizer["InvalidStatusTransition"], from, to);
    }

    /// <summary>
    /// Gets an error message for when an article is already pending payment.
    /// </summary>
    public string AlreadySubmitted()
    {
        return localizer["AlreadySubmitted"];
    }

    /// <summary>
    /// Gets an error message for when an article is already pending review.
    /// </summary>
    public string AlreadyPendingReview()
    {
        return localizer["AlreadyPendingReview"];
    }

    /// <summary>
    /// Gets an error message for when an article is already approved.
    /// </summary>
    public string AlreadyApproved()
    {
        return localizer["AlreadyApproved"];
    }

    /// <summary>
    /// Gets an error message for when an article is already published.
    /// </summary>
    public string AlreadyPublished()
    {
        return localizer["AlreadyPublished"];
    }

    /// <summary>
    /// Gets an error message for when an article is already rejected.
    /// </summary>
    public string AlreadyRejected()
    {
        return localizer["AlreadyRejected"];
    }

    /// <summary>
    /// Gets an error message for when an article is already archived.
    /// </summary>
    public string AlreadyArchived()
    {
        return localizer["AlreadyArchived"];
    }

    /// <summary>
    /// Gets an error message for when a hard delete is attempted on a published or approved article.
    /// </summary>
    /// <returns>
    /// An error message indicating that only articles in Draft or Rejected status can be permanently deleted.
    /// </returns>
    public string CannotDeletePublishedArticle()
    {
        return localizer["CannotDeletePublishedArticle"];
    }

    /// <summary>
    /// Gets an error message for when an article title exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string TitleTooLong(int max) => string.Format(localizer["TitleTooLong"], max);

    /// <summary>
    /// Gets an error message for when an article slug exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SlugTooLong(int max) => string.Format(localizer["SlugTooLong"], max);

    /// <summary>
    /// Gets an error message for when an article slug has an invalid format.
    /// </summary>
    public string SlugInvalidFormat() => localizer["SlugInvalidFormat"];

    /// <summary>
    /// Gets an error message for when an article headline is required.
    /// </summary>
    public string HeadlineRequired() => localizer["HeadlineRequired"];

    /// <summary>
    /// Gets an error message for when an article headline is too short.
    /// </summary>
    /// <param name="min">The minimum required length.</param>
    public string HeadlineTooShort(int min) => string.Format(localizer["HeadlineTooShort"], min);

    /// <summary>
    /// Gets an error message for when an article headline exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string HeadlineTooLong(int max) => string.Format(localizer["HeadlineTooLong"], max);

    /// <summary>
    /// Gets an error message for when an article body is required.
    /// </summary>
    public string BodyRequired() => localizer["BodyRequired"];

    /// <summary>
    /// Gets an error message for when an article rejection reason is required.
    /// </summary>
    public string RejectionReasonRequired() => localizer["RejectionReasonRequired"];

    /// <summary>
    /// Gets an error message for when an article rejection reason exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string RejectionReasonTooLong(int max) => string.Format(localizer["RejectionReasonTooLong"], max);

    /// <summary>
    /// Gets an error message for when a meta title is too short.
    /// </summary>
    /// <param name="min">The minimum required length.</param>
    public string MetaTitleTooShort(int min) => string.Format(localizer["MetaTitleTooShort"], min);

    /// <summary>
    /// Gets an error message for when a meta title exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string MetaTitleTooLong(int max) => string.Format(localizer["MetaTitleTooLong"], max);

    /// <summary>
    /// Gets an error message for when a meta description is too short.
    /// </summary>
    /// <param name="min">The minimum required length.</param>
    public string MetaDescriptionTooShort(int min) => string.Format(localizer["MetaDescriptionTooShort"], min);

    /// <summary>
    /// Gets an error message for when a meta description exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string MetaDescriptionTooLong(int max) => string.Format(localizer["MetaDescriptionTooLong"], max);

    /// <summary>
    /// Gets an error message for when an article image file is required.
    /// </summary>
    public string FileRequired() => localizer["FileRequired"];

    /// <summary>
    /// Gets an error message for when a category ID is required.
    /// </summary>
    public string CategoryIdRequired() => localizer["CategoryIdRequired"];

    /// <summary>
    /// Gets an error message for when the popular-articles limit is outside the accepted range.
    /// </summary>
    /// <param name="min">The smallest accepted limit value.</param>
    /// <param name="max">The largest accepted limit value.</param>
    public string PopularLimitOutOfRange(int min, int max) =>
        string.Format(localizer["PopularLimitOutOfRange"], min, max);

    /// <summary>
    /// Gets an error message for when an article carries no active promotion.
    /// </summary>
    public string NotPromoted() => localizer["NotPromoted"];
}
