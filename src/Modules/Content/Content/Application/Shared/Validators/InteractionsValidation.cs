using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// Shared validation extension methods for Interactions use cases.
/// </summary>
public static class InteractionsValidation
{
    /// <summary>
    /// Validates a comment body — required, max length enforced.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCommentBody<T>(this IRuleBuilderInitial<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Comment body is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxCommentBodyLength)
            .WithMessage($"Comment body must not exceed {ContentConstants.MaxCommentBodyLength} characters.");
    }

    /// <summary>
    /// Validates a playlist name — required, max length enforced.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPlaylistName<T>(this IRuleBuilderInitial<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Playlist name is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxPlaylistNameLength)
            .WithMessage($"Playlist name must not exceed {ContentConstants.MaxPlaylistNameLength} characters.");
    }

    /// <summary>
    /// Validates a video star rating — must be between 1 and 5 inclusive.
    /// </summary>
    public static IRuleBuilderOptions<T, short> ValidVideoStarRating<T>(this IRuleBuilder<T, short> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(from: (short)1, to: (short)5)
            .WithMessage("Star rating must be between 1 and 5.");
    }
}
