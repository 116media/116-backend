using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">The article interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCommentBody<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleInteractionErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.CommentBodyRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxCommentBodyLength)
            .WithMessage(i18n.CommentBodyTooLong(ContentConstants.MaxCommentBodyLength));
    }

    /// <summary>
    /// Validates a playlist name — required, max length enforced.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the playlist name property.</param>
    /// <param name="i18n">The playlist error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidPlaylistName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        PlaylistErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.NameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxPlaylistNameLength)
            .WithMessage(i18n.NameTooLong(ContentConstants.MaxPlaylistNameLength));
    }

    /// <summary>
    /// Validates a video star rating — must be between 1 and 5 inclusive.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the star rating property.</param>
    /// <param name="i18n">The article interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, short> ValidVideoStarRating<T>(
        this IRuleBuilder<T, short> ruleBuilder,
        ArticleInteractionErrorMessage i18n
    )
    {
        return ruleBuilder.InclusiveBetween(from: (short)1, to: (short)5).WithMessage(i18n.InvalidStarRating());
    }
}
