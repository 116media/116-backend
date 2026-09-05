using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the artist aggregate.
/// </summary>
public sealed class ArtistRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.ArtistNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ArtistErrorMessage>().NameRequired()
            ),
            [ContentRuleCodes.ArtistSlugRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ArtistErrorMessage>().SlugRequired()
            ),
            [ContentRuleCodes.ArtistAliasTooLong] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ArtistErrorMessage>().AliasTooLong()
            ),
            [ContentRuleCodes.ArtistTooManyAliases] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ArtistErrorMessage>().TooManyAliases()
            ),
            [ContentRuleCodes.ArtistBirthdateInFuture] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ArtistErrorMessage>().BirthdateInFuture()
            ),
            [ContentRuleCodes.ArtistAlreadyClaimed] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ArtistErrorMessage>().AlreadyClaimed()
            ),
        };
}
