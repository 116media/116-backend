using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Contains metadata information for the schedule shoot route.
/// </summary>
public static class AdminScheduleShootMetaField
{
    public static readonly RouteMetadata AdminScheduleShoot = new(
        "ScheduleShoot",
        "Schedule a video shoot",
        """
            Schedules or updates the shooting date for a video production.
            \n
            Used for pre-booked productions where the client pays before the shoot
            takes place. The shooting date must be in the future.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the date is not in the future\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
