using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Article domain error factory providing simple, readable exception creation.
/// Usage: ArticleErrors.NotFound(id) or ArticleErrors.SlugAlreadyExists(slug)
/// </summary>
public static class ArticleErrors
{
    /// <summary>
    /// Throws when an article is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Article", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when an article with the given slug already exists.
    /// </summary>
    public static ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(ArticleErrorMessage.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when an article title is required but not provided.
    /// </summary>
    public static BadRequestException TitleRequired()
    {
        return new BadRequestException(ArticleErrorMessage.TitleRequired());
    }

    /// <summary>
    /// Throws when an article slug is required but not provided.
    /// </summary>
    public static BadRequestException SlugRequired()
    {
        return new BadRequestException(ArticleErrorMessage.SlugRequired());
    }

    /// <summary>
    /// Throws when an invalid status transition is attempted.
    /// </summary>
    public static BadRequestException InvalidStatusTransition(string from, string to)
    {
        return new BadRequestException(ArticleErrorMessage.InvalidStatusTransition(from: from, to: to));
    }

    /// <summary>
    /// Throws when a hard delete is attempted on a published or approved article.
    /// </summary>
    public static BadRequestException CannotDeletePublishedArticle()
    {
        return new BadRequestException(ArticleErrorMessage.CannotDeletePublishedArticle());
    }
}
