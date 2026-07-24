using System.Reflection;
using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using _116.Content.Application.Editorial.Constants;
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
    /// <param name="i18n">The article error message provider.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.TitleRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage(i18n.TitleTooLong(ContentConstants.MaxTitleLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage(i18n.TitleTooLong(ContentConstants.MaxTitleLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates article slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The article error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
                .Matches(SlugRegex())
                .WithMessage(i18n.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(i18n.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates article headline with minimum and maximum length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the headline property.</param>
    /// <param name="i18n">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleHeadline<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.HeadlineRequired())
            .MinimumLength(minimumLength: ContentConstants.MinHeadlineLength)
            .WithMessage(i18n.HeadlineTooShort(ContentConstants.MinHeadlineLength))
            .MaximumLength(maximumLength: ContentConstants.MaxHeadlineLength)
            .WithMessage(i18n.HeadlineTooLong(ContentConstants.MaxHeadlineLength));
    }

    /// <summary>
    /// Validates that the article body is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the body property.</param>
    /// <param name="i18n">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArticleBody<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ArticleErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.BodyRequired());
    }

    /// <summary>
    /// Validates that a video description is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the description property.</param>
    /// <param name="i18n">The video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        VideoErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.DescriptionRequired());
    }

    /// <summary>
    /// Validates that the order item ID is not empty. Intended for use inside a
    /// <c>When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the order item ID property.</param>
    /// <param name="i18n">The content order error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid?> ValidOrderItemId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        ContentOrderErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.OrderItemIdRequired());
    }

    /// <summary>
    /// Validates that the customer ID is not empty. Intended for use inside a
    /// <c>When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId())</c> block.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the customer ID property.</param>
    /// <param name="i18n">The customer error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid?> ValidCustomerId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        CustomerErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.CustomerIdRequired());
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
    /// <param name="i18n">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRejectionReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArticleErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.RejectionReasonRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage(i18n.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength));
    }

    /// <summary>
    /// Validates a rejection reason with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the rejection reason property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRejectionReason<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.RejectionReasonRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxRejectionReasonLength)
            .WithMessage(i18n.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength));
    }

    /// <summary>
    /// Validates a YouTube video URL.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the YouTube video URL property.</param>
    /// <param name="i18n">The video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidYoutubeVideoUrl<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        VideoErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.YoutubeUrlRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxYoutubeVideoUrlLength)
            .WithMessage(i18n.YoutubeUrlTooLong(ContentConstants.MaxYoutubeVideoUrlLength))
            .Must(url =>
                url is not null
                && (
                    url.Contains("youtube.com/watch")
                    || url.Contains("youtu.be/")
                    || url.Contains("youtube.com/embed/")
                    || url.Contains("youtube.com/shorts/")
                )
            )
            .WithMessage(i18n.YoutubeUrlInvalidFormat());
    }

    /// <summary>
    /// Validates video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="i18n">The video error message provider.</param>
    /// <param name="isRequired">Whether the title is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        VideoErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.TitleRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
                .WithMessage(i18n.TitleTooLong(ContentConstants.MaxTitleLength));
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxTitleLength)
            .WithMessage(i18n.TitleTooLong(ContentConstants.MaxTitleLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Title")));
    }

    /// <summary>
    /// Validates video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The video error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        VideoErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
                .Matches(SlugRegex())
                .WithMessage(i18n.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(i18n.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates short video slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The short video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.SlugRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(i18n.SlugInvalidFormat());
    }

    /// <summary>
    /// Validates short video title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the title property.</param>
    /// <param name="i18n">The short video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidShortVideoTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ShortVideoErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.TitleRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxShortVideoTitleLength)
            .WithMessage(i18n.TitleTooLong(ContentConstants.MaxShortVideoTitleLength));
    }

    /// <summary>
    /// Validates a short video file: required, non-empty, within the caller-supplied size limit, and an allowed extension.
    /// The calling validator is responsible for computing the human-readable size and extension strings from
    /// <see cref="FileConstants"/> before passing them as error messages.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the video file property.</param>
    /// <param name="i18n">The short video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidShortVideoFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        ShortVideoErrorMessage i18n
    )
    {
        return ruleBuilder
            .NotNull()
            .WithMessage(i18n.FileRequired())
            .Must(file => file is null || file.Length > 0)
            .WithMessage(i18n.FileEmpty())
            .Must(file => file is null || file.Length <= FileConstants.MaxVideoFileSizeBytes)
            .WithMessage(i18n.FileTooLarge(FileConstants.MaxVideoFileSizeBytes / (1024 * 1024)))
            .Must(file =>
            {
                if (file is null)
                {
                    return true;
                }

                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                return FileConstants.AllowedVideoExtensions.Contains(ext);
            })
            .WithMessage(i18n.FileInvalidExtension(string.Join(", ", FileConstants.AllowedVideoExtensions)));
    }

    /// <summary>
    /// Validates song title with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the song title property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidSongTitle<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.SongTitleRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxSongTitleLength)
            .WithMessage(i18n.SongTitleTooLong(ContentConstants.MaxSongTitleLength));
    }

    /// <summary>
    /// Validates artist name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the artist name property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArtistName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.ArtistNameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxArtistNameLength)
            .WithMessage(i18n.ArtistNameTooLong(ContentConstants.MaxArtistNameLength));
    }

    /// <summary>
    /// Validates that the lyrics text is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the lyrics text property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsText<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.LyricsTextRequired());
    }

    /// <summary>
    /// Validates lyrics language code with length constraints (ISO 639-1 / BCP-47).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the language property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsLanguage<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.LanguageRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxLyricsLanguageLength)
            .WithMessage(i18n.LanguageTooLong(ContentConstants.MaxLyricsLanguageLength));
    }

    /// <summary>
    /// Validates that a lyrics category ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the category ID property.</param>
    /// <param name="categoryIdRequired">Error message used when the category ID is empty.</param>
    public static void ValidLyricsCategoryId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string categoryIdRequired)
    {
        ruleBuilder.NotEmpty().WithMessage(categoryIdRequired);
    }

    /// <summary>
    /// Validates lyrics slug with length and format constraints (lowercase, letters, numbers, hyphens).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <param name="isRequired">Whether the slug is required (default: true).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLyricsSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n,
        bool isRequired = true
    )
    {
        if (isRequired)
        {
            return ruleBuilder
                .Cascade(cascadeMode: CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(i18n.SlugRequired())
                .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
                .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
                .Matches(SlugRegex())
                .WithMessage(i18n.SlugInvalidFormat());
        }

        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
            .Matches(SlugRegex())
            .WithMessage(i18n.SlugInvalidFormat())
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Slug")));
    }

    /// <summary>
    /// Validates a lyrics release year: must fall between 1900 and one year past the current
    /// year (inclusive). Only applied when a value is present — release year is an optional
    /// song-credit field.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the release year property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, short?> ValidReleaseYear<T>(this IRuleBuilder<T, short?> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween((short)1900, (short)(DateTimeOffset.UtcNow.Year + 1))
            .When(x => GetNullableShortPropertyValue(instance: x, propertyName: "ReleaseYear") is not null);
    }

    /// <summary>
    /// Gets a nullable <see cref="short" /> property value from an instance using reflection.
    /// <see cref="ValidationUtils.GetPropertyValue{T}" /> only supports <see cref="string" />
    /// properties (it always returns null for value types like <c>short?</c>), so release-year
    /// gating needs its own typed accessor.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="instance">The instance to read the property from.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The property value, or null if the property is not found or is null.</returns>
    private static short? GetNullableShortPropertyValue<T>(T instance, string propertyName)
    {
        PropertyInfo? property = typeof(T).GetProperty(name: propertyName);
        return property?.GetValue(obj: instance) as short?;
    }

    /// <summary>
    /// Validates an optional lyrics album name with a maximum length constraint. Only applied
    /// when a non-blank value is present — album is an optional song-credit field.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the album property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidAlbum<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxAlbumNameLength)
            .WithMessage(i18n.AlbumTooLong(ContentConstants.MaxAlbumNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Album")));
    }

    /// <summary>
    /// Validates an optional lyrics record label name with a maximum length constraint. Only
    /// applied when a non-blank value is present — label is an optional song-credit field.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the label property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidLabel<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxLabelNameLength)
            .WithMessage(i18n.LabelTooLong(ContentConstants.MaxLabelNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Label")));
    }

    /// <summary>
    /// Validates an optional lyrics songwriter credit with a maximum length constraint. Only
    /// applied when a non-blank value is present — songwriter is an optional song-credit field.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the songwriter property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidSongwriter<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCreditNameLength)
            .WithMessage(i18n.SongwriterTooLong(ContentConstants.MaxCreditNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Songwriter")));
    }

    /// <summary>
    /// Validates an optional lyrics producer credit with a maximum length constraint. Only
    /// applied when a non-blank value is present — producer is an optional song-credit field.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the producer property.</param>
    /// <param name="i18n">The lyrics error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidProducer<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        LyricsErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .MaximumLength(maximumLength: ContentConstants.MaxCreditNameLength)
            .WithMessage(i18n.ProducerTooLong(ContentConstants.MaxCreditNameLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Producer")));
    }

    /// <summary>
    /// Validates that the shooting scheduled date is in the future.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the shooting scheduled date property.</param>
    /// <param name="i18n">The video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, DateTimeOffset> ValidShootingScheduledAt<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder,
        VideoErrorMessage i18n
    )
    {
        return ruleBuilder.GreaterThan(DateTimeOffset.UtcNow).WithMessage(i18n.ShootingScheduledDateMustBeInFuture());
    }

    /// <summary>
    /// Validates that an article image file is not null.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the file property.</param>
    /// <param name="i18n">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidArticleImageFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        ArticleErrorMessage i18n
    )
    {
        return ruleBuilder.NotNull().WithMessage(i18n.FileRequired());
    }

    /// <summary>
    /// Validates the popular-articles limit — must stay within the bounds defined by
    /// <see cref="PopularArticlesLimits" />.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the limit property.</param>
    /// <param name="i18n">The article error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidPopularArticlesLimit<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        ArticleErrorMessage i18n
    )
    {
        return ruleBuilder
            .InclusiveBetween(from: PopularArticlesLimits.MinLimit, to: PopularArticlesLimits.MaxLimit)
            .WithMessage(i18n.PopularLimitOutOfRange(PopularArticlesLimits.MinLimit, PopularArticlesLimits.MaxLimit));
    }

    /// <summary>
    /// Validates the popular-videos limit — must stay within the bounds defined by
    /// <see cref="PopularVideosLimits" />.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the limit property.</param>
    /// <param name="i18n">The video error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, int> ValidPopularVideosLimit<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        VideoErrorMessage i18n
    )
    {
        return ruleBuilder
            .InclusiveBetween(from: PopularVideosLimits.MinLimit, to: PopularVideosLimits.MaxLimit)
            .WithMessage(i18n.PopularLimitOutOfRange(PopularVideosLimits.MinLimit, PopularVideosLimits.MaxLimit));
    }

    /// <summary>
    /// Validates an artist profile's display name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="i18n">The artist error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArtistName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArtistErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.NameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxArtistNameLength);
    }

    /// <summary>
    /// Validates an artist profile's URL-safe slug with length and format constraints
    /// (lowercase letters, numbers, and hyphens only).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the slug property.</param>
    /// <param name="i18n">The artist error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidArtistSlug<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ArtistErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.SlugRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
            .Matches(SlugRegex());
    }

    /// <summary>
    /// Validates an album's display name with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the name property.</param>
    /// <param name="i18n">The album error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidAlbumName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        AlbumErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.NameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxAlbumNameLength);
    }

    /// <summary>
    /// Generated regex for slug validation — lowercase letters, numbers, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
