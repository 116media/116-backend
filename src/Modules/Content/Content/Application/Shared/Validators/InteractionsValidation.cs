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

    /// <summary>
    /// Validates a caller-supplied device identifier on a view event — optional, capped below the
    /// dedup-key column, which stores this value behind a prefix.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the device identifier property.</param>
    /// <param name="i18n">The short video interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidViewDeviceId<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoInteractionErrorMessage i18n
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxViewDeviceIdLength)
            .WithMessage(i18n.ViewDeviceIdTooLong(ContentConstants.MaxViewDeviceIdLength));
    }

    /// <summary>
    /// Validates a caller-supplied IP address on a view event — optional, max length enforced.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the IP address property.</param>
    /// <param name="i18n">The short video interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidViewIpAddress<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoInteractionErrorMessage i18n
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxViewIpAddressLength)
            .WithMessage(i18n.ViewIpAddressTooLong(ContentConstants.MaxViewIpAddressLength));
    }

    /// <summary>
    /// Validates a caller-supplied user agent on a view event — optional, max length enforced.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the user agent property.</param>
    /// <param name="i18n">The short video interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidViewUserAgent<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoInteractionErrorMessage i18n
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxViewUserAgentLength)
            .WithMessage(i18n.ViewUserAgentTooLong(ContentConstants.MaxViewUserAgentLength));
    }

    /// <summary>
    /// Validates the dwell time reported on a view event — never negative.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the dwell property.</param>
    /// <param name="i18n">The lyrics interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidViewDwellMs<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        LyricsInteractionErrorMessage i18n
    )
    {
        return ruleBuilder.GreaterThanOrEqualTo(0).WithMessage(i18n.DwellMsNegative());
    }

    /// <summary>
    /// Validates the scroll depth reported on a view event — a ratio between 0 and 1.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the scroll depth property.</param>
    /// <param name="i18n">The lyrics interaction error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, double> ValidViewScrollDepthRatio<T>(
        this IRuleBuilder<T, double> ruleBuilder,
        LyricsInteractionErrorMessage i18n
    )
    {
        return ruleBuilder.InclusiveBetween(0, 1).WithMessage(i18n.ScrollDepthOutOfRange());
    }
}
