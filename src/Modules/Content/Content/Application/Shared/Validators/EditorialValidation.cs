using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
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
    /// <param name="categoryIdRequired">Error message used when the category ID is empty.</param>
    public static void ValidArticleCategoryId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string categoryIdRequired)
    {
        ruleBuilder.NotEmpty().WithMessage(categoryIdRequired);
    }

    /// <summary>
    /// Validates article title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="titleRequired">Error message used when the title is empty.</param>
    /// <param name="titleTooLong">Error message used when the title exceeds the maximum length.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string titleRequired,
        string titleTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(titleRequired)
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage(titleTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage(titleTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates article slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="slugRequired">Error message used when the slug is empty.</param>
    /// <param name="slugTooLong">Error message used when the slug exceeds the maximum length.</param>
    /// <param name="slugInvalidFormat">Error message used when the slug does not match the expected format.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string slugRequired,
        string slugTooLong,
        string slugInvalidFormat,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(slugRequired)
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(slugTooLong)
                .Matches(SlugRegex())
                .WithMessage(slugInvalidFormat);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(slugTooLong)
            .Matches(SlugRegex())
            .WithMessage(slugInvalidFormat)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates article headline with minimum and maximum length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the headline property.</param>
    /// <param name="headlineRequired">Error message used when the headline is empty.</param>
    /// <param name="headlineTooShort">Error message used when the headline is below the minimum length.</param>
    /// <param name="headlineTooLong">Error message used when the headline exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleHeadline<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string headlineRequired,
        string headlineTooShort,
        string headlineTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(headlineRequired)
            .MinimumLength(minimumLength: ContentConstants.MinHeadlineLength)
            .WithMessage(headlineTooShort)
            .MaximumLength(maximumLength: ContentConstants.MaxHeadlineLength)
            .WithMessage(headlineTooLong);
    }

    /// <summary>
    /// Validates that the article body is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the body property.</param>
    /// <param name="bodyRequired">Error message used when the body is empty.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleBody<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string bodyRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(bodyRequired);
    }

    /// <summary>
    /// Validates that a video description is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="descriptionRequired">Error message used when the description is empty.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string descriptionRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(descriptionRequired);
    }

    /// <summary>
    /// Validates that the order item ID is not empty. Intended for use inside a
    /// <c>When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the order item ID property.</param>
    /// <param name="orderItemIdRequired">Error message used when the order item ID is empty.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid?> ValidOrderItemId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        string orderItemIdRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(orderItemIdRequired);
    }

    /// <summary>
    /// Validates that the customer ID is not empty. Intended for use inside a
    /// <c>When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the customer ID property.</param>
    /// <param name="customerIdRequired">Error message used when the customer ID is empty.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid?> ValidCustomerId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        string customerIdRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(customerIdRequired);
    }

    /// <summary>
    /// Validates an unpromote reason: required and at most 500 characters.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the reason property.</param>
    /// <param name="reasonRequired">Error message used when the reason is empty.</param>
    /// <param name="reasonTooLong">Error message used when the reason exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidUnpromoteReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string reasonRequired,
        string reasonTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(reasonRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage(reasonTooLong);
    }

    /// <summary>
    /// Validates an SEO meta title: between
    /// <see cref="ContentConstants.MinMetaTitleLength"/> and <see cref="ContentConstants.MaxMetaTitleLength"/>
    /// characters. Intended for use inside a
    /// <c>When(x => x.MetaTitle is not null, () => RuleFor(x => x.MetaTitle).ValidMetaTitle())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the meta title property.</param>
    /// <param name="metaTitleTooShort">Error message used when the meta title is below the minimum length.</param>
    /// <param name="metaTitleTooLong">Error message used when the meta title exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidMetaTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string metaTitleTooShort,
        string metaTitleTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MinimumLength(minimumLength: ContentConstants.MinMetaTitleLength)
            .WithMessage(metaTitleTooShort)
            .MaximumLength(maximumLength: ContentConstants.MaxMetaTitleLength)
            .WithMessage(metaTitleTooLong);
    }

    /// <summary>
    /// Validates an SEO meta description: between
    /// <see cref="ContentConstants.MinMetaDescriptionLength"/> and <see cref="ContentConstants.MaxMetaDescriptionLength"/>
    /// characters. Intended for use inside a
    /// <c>When(x => x.MetaDescription is not null, () => RuleFor(x => x.MetaDescription).ValidMetaDescription())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the meta description property.</param>
    /// <param name="metaDescriptionTooShort">Error message used when the meta description is below the minimum length.</param>
    /// <param name="metaDescriptionTooLong">Error message used when the meta description exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidMetaDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string metaDescriptionTooShort,
        string metaDescriptionTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MinimumLength(minimumLength: ContentConstants.MinMetaDescriptionLength)
            .WithMessage(metaDescriptionTooShort)
            .MaximumLength(maximumLength: ContentConstants.MaxMetaDescriptionLength)
            .WithMessage(metaDescriptionTooLong);
    }

    /// <summary>
    /// Validates a rejection reason with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the rejection reason property.</param>
    /// <param name="reasonRequired">Error message used when the reason is empty.</param>
    /// <param name="reasonTooLong">Error message used when the reason exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRejectionReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string reasonRequired,
        string reasonTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(reasonRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage(reasonTooLong);
    }

    /// <summary>
    /// Validates a YouTube video URL.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the YouTube video URL property.</param>
    /// <param name="youtubeUrlRequired">Error message used when the URL is empty.</param>
    /// <param name="youtubeUrlTooLong">Error message used when the URL exceeds the maximum length.</param>
    /// <param name="youtubeUrlInvalidFormat">Error message used when the URL is not a recognised YouTube URL.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidYoutubeVideoUrl<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string youtubeUrlRequired,
        string youtubeUrlTooLong,
        string youtubeUrlInvalidFormat
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(youtubeUrlRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxYoutubeVideoUrlLength)
            .WithMessage(youtubeUrlTooLong)
            .Must(url =>
                url is not null
                && (
                    url.Contains("youtube.com/watch")
                    || url.Contains("youtu.be/")
                    || url.Contains("youtube.com/embed/")
                    || url.Contains("youtube.com/shorts/")
                )
            )
            .WithMessage(youtubeUrlInvalidFormat);
    }

    /// <summary>
    /// Validates video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="titleRequired">Error message used when the title is empty.</param>
    /// <param name="titleTooLong">Error message used when the title exceeds the maximum length.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string titleRequired,
        string titleTooLong,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(titleRequired)
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage(titleTooLong);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage(titleTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="slugRequired">Error message used when the slug is empty.</param>
    /// <param name="slugTooLong">Error message used when the slug exceeds the maximum length.</param>
    /// <param name="slugInvalidFormat">Error message used when the slug does not match the expected format.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string slugRequired,
        string slugTooLong,
        string slugInvalidFormat,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(slugRequired)
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(slugTooLong)
                .Matches(SlugRegex())
                .WithMessage(slugInvalidFormat);
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(slugTooLong)
            .Matches(SlugRegex())
            .WithMessage(slugInvalidFormat)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates short video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="slugRequired">Error message used when the slug is empty.</param>
    /// <param name="slugTooLong">Error message used when the slug exceeds the maximum length.</param>
    /// <param name="slugInvalidFormat">Error message used when the slug does not match the expected format.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string slugRequired,
        string slugTooLong,
        string slugInvalidFormat
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(slugRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(slugTooLong)
            .Matches(SlugRegex())
            .WithMessage(slugInvalidFormat);
    }

    /// <summary>
    /// Validates short video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="titleRequired">Error message used when the title is empty.</param>
    /// <param name="titleTooLong">Error message used when the title exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string titleRequired,
        string titleTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(titleRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxShortVideoTitleLength)
            .WithMessage(titleTooLong);
    }

    /// <summary>
    /// Validates a short video file: required, non-empty, within the caller-supplied size limit, and an allowed extension.
    /// The calling validator is responsible for computing the human-readable size and extension strings from
    /// <see cref="FileConstants"/> before passing them as error messages.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video file property.</param>
    /// <param name="fileRequired">Error message used when the file is null.</param>
    /// <param name="fileEmpty">Error message used when the file has zero bytes.</param>
    /// <param name="fileTooLarge">Error message used when the file exceeds the maximum allowed size.</param>
    /// <param name="fileInvalidExtension">Error message used when the file extension is not allowed.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidShortVideoFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        string fileRequired,
        string fileEmpty,
        string fileTooLarge,
        string fileInvalidExtension
    )
    {
        return ruleBuilder
            .NotNull()
            .WithMessage(fileRequired)
            .Must(file => file is null || file.Length > 0)
            .WithMessage(fileEmpty)
            .Must(file => file is null || file.Length <= FileConstants.MaxVideoFileSizeBytes)
            .WithMessage(fileTooLarge)
            .Must(file =>
            {
                if (file is null)
                {
                    return true;
                }

                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                return FileConstants.AllowedVideoExtensions.Contains(ext);
            })
            .WithMessage(fileInvalidExtension);
    }

    /// <summary>
    /// Validates song title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the song title property.</param>
    /// <param name="songTitleRequired">Error message used when the song title is empty.</param>
    /// <param name="songTitleTooLong">Error message used when the song title exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidSongTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string songTitleRequired,
        string songTitleTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(songTitleRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxSongTitleLength)
            .WithMessage(songTitleTooLong);
    }

    /// <summary>
    /// Validates artist name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the artist name property.</param>
    /// <param name="artistNameRequired">Error message used when the artist name is empty.</param>
    /// <param name="artistNameTooLong">Error message used when the artist name exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArtistName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string artistNameRequired,
        string artistNameTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(artistNameRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxArtistNameLength)
            .WithMessage(artistNameTooLong);
    }

    /// <summary>
    /// Validates that the lyrics text is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics text property.</param>
    /// <param name="lyricsTextRequired">Error message used when the lyrics text is empty.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsText<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string lyricsTextRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(lyricsTextRequired);
    }

    /// <summary>
    /// Validates lyrics language code with length constraints (ISO 639-1 / BCP-47).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the language property.</param>
    /// <param name="languageRequired">Error message used when the language code is empty.</param>
    /// <param name="languageTooLong">Error message used when the language code exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsLanguage<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string languageRequired,
        string languageTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(languageRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxLyricsLanguageLength)
            .WithMessage(languageTooLong);
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
    /// <param name="fileRequired">Error message used when the image file is null.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidArticleImageFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        string fileRequired
    )
    {
        return ruleBuilder.NotNull().WithMessage(fileRequired);
    }

    /// <summary>
    /// Validates that an article ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the article ID property.</param>
    /// <param name="articleIdRequired">Error message used when the article ID is empty.</param>
    public static void ValidArticleId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string articleIdRequired)
    {
        ruleBuilder.NotEmpty().WithMessage(articleIdRequired);
    }

    /// <summary>
    /// Validates that a video ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video ID property.</param>
    /// <param name="videoIdRequired">Error message used when the video ID is empty.</param>
    public static void ValidVideoId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string videoIdRequired)
    {
        ruleBuilder.NotEmpty().WithMessage(videoIdRequired);
    }

    /// <summary>
    /// Validates that a lyrics ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics ID property.</param>
    /// <param name="lyricsIdRequired">Error message used when the lyrics ID is empty.</param>
    public static void ValidLyricsId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string lyricsIdRequired)
    {
        ruleBuilder.NotEmpty().WithMessage(lyricsIdRequired);
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
