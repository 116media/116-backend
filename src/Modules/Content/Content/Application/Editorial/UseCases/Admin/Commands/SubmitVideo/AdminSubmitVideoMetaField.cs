using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Contains metadata information for the submit video route.
/// </summary>
public static class AdminSubmitVideoMetaField
{
    public static readonly RouteMetadata AdminSubmitVideo = new(
        "SubmitVideo",
        "Submit a video for review or payment",
        """
            Submits a video draft for review or payment.
            \n
            Free videos transition from <c>Draft</c> to <c>PendingReview</c>.
            Paid videos (linked to a customer and order item) transition from
            <c>Draft</c> to <c>PendingPayment</c>.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
