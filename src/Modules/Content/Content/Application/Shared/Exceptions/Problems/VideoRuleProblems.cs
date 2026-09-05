using System.Globalization;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the video aggregate.
/// </summary>
public sealed class VideoRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.VideoTitleRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<VideoErrorMessage>().TitleRequired()
            ),
            [ContentRuleCodes.VideoSlugRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<VideoErrorMessage>().SlugRequired()
            ),
            [ContentRuleCodes.VideoNotPromoted] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<VideoErrorMessage>().NotPromoted()
            ),
            [ContentRuleCodes.CannotAttachYoutubeUrlBeforeShoot] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, args) =>
                    ctx.Resolve<VideoErrorMessage>()
                        .CannotAttachYoutubeUrlBeforeShoot(
                            shootingScheduledAt: DateTimeOffset.Parse(
                                args[0],
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind
                            )
                        )
            ),
        };
}
