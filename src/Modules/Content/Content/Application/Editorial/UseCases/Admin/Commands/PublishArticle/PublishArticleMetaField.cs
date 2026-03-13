using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle;

/// <summary>
/// Contains metadata information for the publish article route.
/// </summary>
public static class PublishArticleMetaField
{
    public static readonly RouteMetadata PublishArticle = new(
        "PublishArticle",
        "Publish an approved article",
        """
            Publishes an article that has been approved by the editorial team,
            making it live and visible to all public visitors.
            \n
            Only articles in <c>Approved</c> status can be published.
            Attempting to publish an article in any other status will return a 400 Bad Request.
            \n
            The <c>publishedAt</c> timestamp is automatically stamped at the moment of publication.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the article is not in Approved status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
