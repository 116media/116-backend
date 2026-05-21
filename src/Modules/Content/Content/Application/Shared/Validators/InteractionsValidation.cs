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
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the comment body property.</param>
    /// <param name="commentBodyRequired">Error message used when the comment body is empty.</param>
    /// <param name="commentBodyTooLong">Error message used when the comment body exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCommentBody<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string commentBodyRequired,
        string commentBodyTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(commentBodyRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxCommentBodyLength)
            .WithMessage(commentBodyTooLong);
    }

    /// <summary>
    /// Validates a playlist name — required, max length enforced.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the playlist name property.</param>
    /// <param name="nameRequired">Error message used when the playlist name is empty.</param>
    /// <param name="nameTooLong">Error message used when the playlist name exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPlaylistName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string nameRequired,
        string nameTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(nameRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxPlaylistNameLength)
            .WithMessage(nameTooLong);
    }

    /// <summary>
    /// Validates a video star rating — must be between 1 and 5 inclusive.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the star rating property.</param>
    /// <param name="invalidStarRating">Error message used when the star rating is outside the allowed range.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, short> ValidVideoStarRating<T>(
        this IRuleBuilder<T, short> ruleBuilder,
        string invalidStarRating
    )
    {
        return ruleBuilder.InclusiveBetween(from: (short)1, to: (short)5).WithMessage(invalidStarRating);
    }
}
