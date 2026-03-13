using System.Text.RegularExpressions;
using _116.Content.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for editorial field validation (articles, videos, short videos, lyrics).
/// </summary>
public static partial class EditorialValidation
{
    /// <summary>
    /// Validates that an article or video category ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the category ID property.</param>
    public static void ValidArticleCategoryId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Category ID is required.");
    }

    /// <summary>
    /// Validates article title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Article title is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage($"Article title must not exceed {ContentConstants.MaxTitleLength} characters.");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage($"Article title must not exceed {ContentConstants.MaxTitleLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates article slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Article slug is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage($"Article slug must not exceed {ContentConstants.MaxSlugLength} characters.")
                .Matches(SlugRegex())
                .WithMessage("Article slug must be lowercase and contain only letters, numbers, and hyphens.");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage($"Article slug must not exceed {ContentConstants.MaxSlugLength} characters.")
            .Matches(SlugRegex())
            .WithMessage("Article slug must be lowercase and contain only letters, numbers, and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates article headline with minimum and maximum length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the headline property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleHeadline<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Article headline is required.")
            .MinimumLength(minimumLength: ContentConstants.MinHeadlineLength)
            .WithMessage($"Article headline must be at least {ContentConstants.MinHeadlineLength} characters.")
            .MaximumLength(maximumLength: ContentConstants.MaxHeadlineLength)
            .WithMessage($"Article headline must not exceed {ContentConstants.MaxHeadlineLength} characters.");
    }

    /// <summary>
    /// Validates that the article body is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the body property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleBody<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage("Article body is required.");
    }

    /// <summary>
    /// Validates a rejection reason with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the rejection reason property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRejectionReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Rejection reason is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage($"Rejection reason must not exceed {ContentConstants.MaxRejectionReasonLength} characters.");
    }

    /// <summary>
    /// Validates a YouTube video ID with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the YouTube video ID property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidYoutubeVideoId<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("YouTube video ID is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxYoutubeVideoIdLength)
            .WithMessage($"YouTube video ID must not exceed {ContentConstants.MaxYoutubeVideoIdLength} characters.");
    }

    /// <summary>
    /// Validates video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Video title is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage($"Video title must not exceed {ContentConstants.MaxTitleLength} characters.");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage($"Video title must not exceed {ContentConstants.MaxTitleLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Video slug is required.")
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage($"Video slug must not exceed {ContentConstants.MaxSlugLength} characters.")
                .Matches(SlugRegex())
                .WithMessage("Video slug must be lowercase and contain only letters, numbers, and hyphens.");
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage($"Video slug must not exceed {ContentConstants.MaxSlugLength} characters.")
            .Matches(SlugRegex())
            .WithMessage("Video slug must be lowercase and contain only letters, numbers, and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates short video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Short video title is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxShortVideoTitleLength)
            .WithMessage($"Short video title must not exceed {ContentConstants.MaxShortVideoTitleLength} characters.");
    }

    /// <summary>
    /// Validates that a short video file is not null.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video file property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidShortVideoFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder
    )
    {
        return ruleBuilder.NotNull().WithMessage("Short video file is required.");
    }

    /// <summary>
    /// Validates song title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the song title property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidSongTitle<T>(this IRuleBuilderInitial<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Song title is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxSongTitleLength)
            .WithMessage($"Song title must not exceed {ContentConstants.MaxSongTitleLength} characters.");
    }

    /// <summary>
    /// Validates artist name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the artist name property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArtistName<T>(this IRuleBuilderInitial<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Artist name is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxArtistNameLength)
            .WithMessage($"Artist name must not exceed {ContentConstants.MaxArtistNameLength} characters.");
    }

    /// <summary>
    /// Validates that the lyrics text is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics text property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsText<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage("Lyrics text is required.");
    }

    /// <summary>
    /// Validates lyrics language code with length constraints (ISO 639-1 / BCP-47).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the language property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsLanguage<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Lyrics language is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxLyricsLanguageLength)
            .WithMessage($"Lyrics language must not exceed {ContentConstants.MaxLyricsLanguageLength} characters.");
    }

    /// <summary>
    /// Validates that the shooting scheduled date is in the future.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the shooting scheduled date property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, DateTimeOffset> ValidShootingScheduledAt<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder
    )
    {
        return ruleBuilder
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Shooting scheduled date must be in the future.");
    }

    /// <summary>
    /// Validates that an article image file is not null.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the file property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidArticleImageFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder
    )
    {
        return ruleBuilder.NotNull().WithMessage("Article image file is required.");
    }

    /// <summary>
    /// Validates that an article ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the article ID property.</param>
    public static void ValidArticleId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Article ID is required.");
    }

    /// <summary>
    /// Validates that a video ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video ID property.</param>
    public static void ValidVideoId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Video ID is required.");
    }

    /// <summary>
    /// Validates that a lyrics ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics ID property.</param>
    public static void ValidLyricsId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        ruleBuilder.NotEmpty().WithMessage("Lyrics ID is required.");
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
