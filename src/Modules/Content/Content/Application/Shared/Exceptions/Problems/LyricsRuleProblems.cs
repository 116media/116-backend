using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the lyrics aggregate.
/// </summary>
public sealed class LyricsRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.LyricsSlugRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<LyricsErrorMessage>().SlugRequired()
            ),
            [ContentRuleCodes.SongTitleRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<LyricsErrorMessage>().SongTitleRequired()
            ),
            [ContentRuleCodes.LyricsArtistNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<LyricsErrorMessage>().ArtistNameRequired()
            ),
            [ContentRuleCodes.LyricsTextRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<LyricsErrorMessage>().LyricsTextRequired()
            ),
            [ContentRuleCodes.LyricsNotPromoted] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<LyricsErrorMessage>().NotPromoted()
            ),
        };
}
