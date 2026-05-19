using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The article error message provider.</param>
    public static void ValidArticleCategoryId<T>(this IRuleBuilder<T, Guid> ruleBuilder, ArticleErrorMessage msg)
    {
        ruleBuilder.NotEmpty().WithMessage(msg.TitleRequired());
    }

    /// <summary>
    /// Validates article title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.TitleRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage(msg.TitleTooLong(ContentConstants.MaxTitleLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxTitleLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates article slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(msg.SlugTooLong(ContentConstants.MaxSlugLength))
                .Matches(SlugRegex())
                .WithMessage(msg.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(msg.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(msg.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates article headline with minimum and maximum length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the headline property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleHeadline<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.HeadlineRequired())
            .MinimumLength(minimumLength: ContentConstants.MinHeadlineLength)
            .WithMessage(msg.HeadlineTooShort(ContentConstants.MinHeadlineLength))
            .MaximumLength(maximumLength: ContentConstants.MaxHeadlineLength)
            .WithMessage(msg.HeadlineTooLong(ContentConstants.MaxHeadlineLength));
    }

    /// <summary>
    /// Validates that the article body is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the body property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleBody<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(msg.BodyRequired());
    }

    /// <summary>
    /// Validates that a video description is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="msg">The video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        VideoErrorMessage msg
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(msg.DescriptionRequired());
    }

    /// <summary>
    /// Validates that the order item ID is not empty. Intended for use inside a
    /// <c>When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the order item ID property.</param>
    /// <param name="msg">The content order error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid?> ValidOrderItemId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        ContentOrderErrorMessage msg
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(msg.AdminUserIdRequired());
    }

    /// <summary>
    /// Validates that the customer ID is not empty. Intended for use inside a
    /// <c>When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the customer ID property.</param>
    /// <param name="msg">The customer error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid?> ValidCustomerId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        CustomerErrorMessage msg
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(msg.FullNameRequired());
    }

    /// <summary>
    /// Validates an unpromote reason: required and at most 500 characters.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the reason property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidUnpromoteReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.BodyRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxRejectionReasonLength));
    }

    /// <summary>
    /// Validates an SEO meta title: between
    /// <see cref="ContentConstants.MinMetaTitleLength"/> and <see cref="ContentConstants.MaxMetaTitleLength"/>
    /// characters. Intended for use inside a
    /// <c>When(x => x.MetaTitle is not null, () => RuleFor(x => x.MetaTitle).ValidMetaTitle())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the meta title property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidMetaTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MinimumLength(minimumLength: ContentConstants.MinMetaTitleLength)
            .WithMessage(msg.HeadlineTooShort(ContentConstants.MinMetaTitleLength))
            .MaximumLength(maximumLength: ContentConstants.MaxMetaTitleLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxMetaTitleLength));
    }

    /// <summary>
    /// Validates an SEO meta description: between
    /// <see cref="ContentConstants.MinMetaDescriptionLength"/> and <see cref="ContentConstants.MaxMetaDescriptionLength"/>
    /// characters. Intended for use inside a
    /// <c>When(x => x.MetaDescription is not null, () => RuleFor(x => x.MetaDescription).ValidMetaDescription())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the meta description property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidMetaDescription<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MinimumLength(minimumLength: ContentConstants.MinMetaDescriptionLength)
            .WithMessage(msg.HeadlineTooShort(ContentConstants.MinMetaDescriptionLength))
            .MaximumLength(maximumLength: ContentConstants.MaxMetaDescriptionLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxMetaDescriptionLength));
    }

    /// <summary>
    /// Validates a rejection reason with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the rejection reason property.</param>
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRejectionReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.BodyRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxRejectionReasonLength));
    }

    /// <summary>
    /// Validates a YouTube video URL.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the YouTube video URL property.</param>
    /// <param name="msg">The video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidYoutubeVideoUrl<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        VideoErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.YoutubeUrlRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxYoutubeVideoUrlLength)
            .WithMessage(msg.YoutubeUrlTooLong(ContentConstants.MaxYoutubeVideoUrlLength))
            .Must(url =>
                url is not null
                && (
                    url.Contains("youtube.com/watch")
                    || url.Contains("youtu.be/")
                    || url.Contains("youtube.com/embed/")
                    || url.Contains("youtube.com/shorts/")
                )
            )
            .WithMessage(msg.YoutubeUrlInvalidFormat());
    }

    /// <summary>
    /// Validates video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="msg">The video error message provider.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        VideoErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.TitleRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage(msg.TitleTooLong(ContentConstants.MaxTitleLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxTitleLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="msg">The video error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        VideoErrorMessage msg,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(msg.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(msg.SlugTooLong(ContentConstants.MaxSlugLength))
                .Matches(SlugRegex())
                .WithMessage(msg.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(msg.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(msg.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates short video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="msg">The short video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.SlugRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(msg.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(msg.SlugInvalidFormat());
    }

    /// <summary>
    /// Validates short video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="msg">The short video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.TitleRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxShortVideoTitleLength)
            .WithMessage(msg.TitleTooLong(ContentConstants.MaxShortVideoTitleLength));
    }

    /// <summary>
    /// Validates a short video file: required, non-empty, max 100 MB, video formats only.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video file property.</param>
    /// <param name="msg">The short video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidShortVideoFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        ShortVideoErrorMessage msg
    )
    {
        long maxMb = FileConstants.MaxVideoFileSizeBytes / (1024 * 1024);
        string allowedExts = string.Join(", ", FileConstants.AllowedVideoExtensions);

        return ruleBuilder
            .NotNull()
            .WithMessage(msg.FileRequired())
            .Must(file => file is null || file.Length > 0)
            .WithMessage(msg.FileEmpty())
            .Must(file => file is null || file.Length <= FileConstants.MaxVideoFileSizeBytes)
            .WithMessage(msg.FileTooLarge(maxMb))
            .Must(file =>
            {
                if (file is null)
                {
                    return true;
                }

                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                return FileConstants.AllowedVideoExtensions.Contains(ext);
            })
            .WithMessage(msg.FileInvalidExtension(allowedExts));
    }

    /// <summary>
    /// Validates song title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the song title property.</param>
    /// <param name="msg">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidSongTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.SongTitleRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxSongTitleLength)
            .WithMessage(msg.SongTitleTooLong(ContentConstants.MaxSongTitleLength));
    }

    /// <summary>
    /// Validates artist name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the artist name property.</param>
    /// <param name="msg">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArtistName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.ArtistNameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxArtistNameLength)
            .WithMessage(msg.ArtistNameTooLong(ContentConstants.MaxArtistNameLength));
    }

    /// <summary>
    /// Validates that the lyrics text is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics text property.</param>
    /// <param name="msg">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsText<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        LyricsErrorMessage msg
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(msg.LyricsTextRequired());
    }

    /// <summary>
    /// Validates lyrics language code with length constraints (ISO 639-1 / BCP-47).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the language property.</param>
    /// <param name="msg">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsLanguage<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.LanguageRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxLyricsLanguageLength)
            .WithMessage(msg.LanguageTooLong(ContentConstants.MaxLyricsLanguageLength));
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
    /// <param name="msg">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidArticleImageFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        ArticleErrorMessage msg
    )
    {
        return ruleBuilder.NotNull().WithMessage(msg.BodyRequired());
    }

    /// <summary>
    /// Validates that an article ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the article ID property.</param>
    /// <param name="msg">The article error message provider.</param>
    public static void ValidArticleId<T>(this IRuleBuilder<T, Guid> ruleBuilder, ArticleErrorMessage msg)
    {
        ruleBuilder.NotEmpty().WithMessage(msg.TitleRequired());
    }

    /// <summary>
    /// Validates that a video ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video ID property.</param>
    /// <param name="msg">The video error message provider.</param>
    public static void ValidVideoId<T>(this IRuleBuilder<T, Guid> ruleBuilder, VideoErrorMessage msg)
    {
        ruleBuilder.NotEmpty().WithMessage(msg.TitleRequired());
    }

    /// <summary>
    /// Validates that a lyrics ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics ID property.</param>
    /// <param name="msg">The lyrics error message provider.</param>
    public static void ValidLyricsId<T>(this IRuleBuilder<T, Guid> ruleBuilder, LyricsErrorMessage msg)
    {
        ruleBuilder.NotEmpty().WithMessage(msg.SongTitleRequired());
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
