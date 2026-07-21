using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Core.Application.Shared.Errors;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Errors.Messages;
using CoreConflictMsg = _116.Core.Application.Shared.Errors.Messages.ConflictErrorMessage;
using CoreInternalServerMsg = _116.Core.Application.Shared.Errors.Messages.InternalServerErrorMessage;
using CoreValidationMsg = _116.Core.Application.Shared.Errors.Messages.ValidationErrorMessage;
using IdentityConflictMsg = _116.Identity.Application.Shared.Errors.Messages.ConflictErrorMessage;
using IdentityValidationMsg = _116.Identity.Application.Shared.Errors.Messages.ValidationErrorMessage;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Builds real <c>*Errors</c> instances backed by embedded .resx resources for use in test builders.
/// Each method creates the required <c>*ErrorMessage</c> dependencies via <see cref="LocalizerFactory"/>
/// and returns a fully functional errors factory for the given domain.
/// </summary>
public static class TestErrorsFactory
{
    /// <summary>
    /// Creates a real <see cref="UserErrors"/> instance for use in Identity entity builders.
    /// </summary>
    public static UserErrors CreateUserErrors()
    {
        return new UserErrors(
            LocalizerFactory.CreateMessage<IdentityConflictMsg>(),
            LocalizerFactory.CreateMessage<IdentityValidationMsg>(),
            LocalizerFactory.CreateMessage<AuthenticationErrorMessage>(),
            LocalizerFactory.CreateMessage<AuthorizationErrorMessage>()
        );
    }

    /// <summary>
    /// Creates a real <see cref="SessionErrors"/> instance for use in Identity session builders.
    /// </summary>
    public static SessionErrors CreateSessionErrors()
    {
        return new SessionErrors(LocalizerFactory.CreateMessage<AuthenticationErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="TagErrors"/> instance for use in Content tag entity builders.
    /// </summary>
    public static TagErrors CreateTagErrors()
    {
        return new TagErrors(LocalizerFactory.CreateMessage<TagErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ArticleErrors"/> instance for use in Content article entity builders.
    /// </summary>
    public static ArticleErrors CreateArticleErrors()
    {
        return new ArticleErrors(LocalizerFactory.CreateMessage<ArticleErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="VideoErrors"/> instance for use in Content video entity builders.
    /// </summary>
    public static VideoErrors CreateVideoErrors()
    {
        return new VideoErrors(LocalizerFactory.CreateMessage<VideoErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ShortVideoErrors"/> instance for use in Content short-video entity builders.
    /// </summary>
    public static ShortVideoErrors CreateShortVideoErrors()
    {
        return new ShortVideoErrors(LocalizerFactory.CreateMessage<ShortVideoErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="LyricsErrors"/> instance for use in Content lyrics entity builders.
    /// </summary>
    public static LyricsErrors CreateLyricsErrors()
    {
        return new LyricsErrors(LocalizerFactory.CreateMessage<LyricsErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="CategoryErrors"/> instance for use in Content category entity builders.
    /// </summary>
    public static CategoryErrors CreateCategoryErrors()
    {
        return new CategoryErrors(LocalizerFactory.CreateMessage<CategoryErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ContentOrderErrors"/> instance for use in Content order entity builders.
    /// </summary>
    public static ContentOrderErrors CreateContentOrderErrors()
    {
        return new ContentOrderErrors(LocalizerFactory.CreateMessage<ContentOrderErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ContentTypeErrors"/> instance for use in Content type entity builders.
    /// </summary>
    public static ContentTypeErrors CreateContentTypeErrors()
    {
        return new ContentTypeErrors(LocalizerFactory.CreateMessage<ContentTypeErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="CustomerErrors"/> instance for use in Content customer entity builders.
    /// </summary>
    public static CustomerErrors CreateCustomerErrors()
    {
        return new CustomerErrors(LocalizerFactory.CreateMessage<CustomerErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="PackageErrors"/> instance for use in Content package entity builders.
    /// </summary>
    public static PackageErrors CreatePackageErrors()
    {
        return new PackageErrors(LocalizerFactory.CreateMessage<PackageErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="PricingTierErrors"/> instance for use in Content pricing-tier entity builders.
    /// </summary>
    public static PricingTierErrors CreatePricingTierErrors()
    {
        return new PricingTierErrors(LocalizerFactory.CreateMessage<PricingTierErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="PromotionLevelErrors"/> instance for use in Content promotion-level entity builders.
    /// </summary>
    public static PromotionLevelErrors CreatePromotionLevelErrors()
    {
        return new PromotionLevelErrors(LocalizerFactory.CreateMessage<PromotionLevelErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ArticleInteractionErrors"/> instance for use in Content article-interaction builders.
    /// </summary>
    public static ArticleInteractionErrors CreateArticleInteractionErrors()
    {
        return new ArticleInteractionErrors(LocalizerFactory.CreateMessage<ArticleInteractionErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ShortVideoInteractionErrors"/> instance for use in Content short-video-interaction builders.
    /// </summary>
    public static ShortVideoInteractionErrors CreateShortVideoInteractionErrors()
    {
        return new ShortVideoInteractionErrors(LocalizerFactory.CreateMessage<ShortVideoInteractionErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="LyricsInteractionErrors"/> instance for use in Content lyrics-interaction builders.
    /// </summary>
    public static LyricsInteractionErrors CreateLyricsInteractionErrors()
    {
        return new LyricsInteractionErrors(LocalizerFactory.CreateMessage<LyricsInteractionErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="PlaylistErrors"/> instance for use in Content playlist entity builders.
    /// </summary>
    public static PlaylistErrors CreatePlaylistErrors()
    {
        return new PlaylistErrors(LocalizerFactory.CreateMessage<PlaylistErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="ArtistErrors"/> instance for use in Content artist entity builders.
    /// </summary>
    public static ArtistErrors CreateArtistErrors()
    {
        return new ArtistErrors(LocalizerFactory.CreateMessage<ArtistErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="AlbumErrors"/> instance for use in Content album entity builders.
    /// </summary>
    public static AlbumErrors CreateAlbumErrors()
    {
        return new AlbumErrors(LocalizerFactory.CreateMessage<AlbumErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="TranslationErrors"/> instance for use in Content lyrics
    /// translation entity builders.
    /// </summary>
    public static TranslationErrors CreateTranslationErrors()
    {
        return new TranslationErrors(LocalizerFactory.CreateMessage<TranslationErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="SubmissionErrors"/> instance for use in Content lyrics
    /// submission entity builders.
    /// </summary>
    public static SubmissionErrors CreateSubmissionErrors()
    {
        return new SubmissionErrors(LocalizerFactory.CreateMessage<SubmissionErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="LyricsRevisionErrors"/> instance for use in Content lyrics
    /// revision entity builders.
    /// </summary>
    public static LyricsRevisionErrors CreateLyricsRevisionErrors()
    {
        return new LyricsRevisionErrors(LocalizerFactory.CreateMessage<LyricsRevisionErrorMessage>());
    }

    /// <summary>
    /// Creates a real <see cref="FileErrors"/> instance for use in Core file entity builders.
    /// </summary>
    public static FileErrors CreateFileErrors()
    {
        return new FileErrors(
            LocalizerFactory.CreateMessage<CoreConflictMsg>(),
            LocalizerFactory.CreateMessage<CoreValidationMsg>(),
            LocalizerFactory.CreateMessage<CoreInternalServerMsg>()
        );
    }

    /// <summary>
    /// Creates a real <see cref="ContentI18n"/> instance for use in Content handler tests.
    /// </summary>
    public static ContentI18n CreateContentI18n()
    {
        return new ContentI18n(
            CreateArticleErrors(),
            CreateVideoErrors(),
            CreateShortVideoErrors(),
            CreateLyricsErrors(),
            CreateCategoryErrors(),
            CreateTagErrors(),
            CreateContentTypeErrors(),
            CreatePricingTierErrors(),
            CreatePackageErrors(),
            CreateCustomerErrors(),
            CreateContentOrderErrors(),
            CreatePlaylistErrors(),
            CreateArticleInteractionErrors(),
            CreateShortVideoInteractionErrors(),
            CreateLyricsInteractionErrors(),
            CreatePromotionLevelErrors(),
            CreateArtistErrors(),
            CreateAlbumErrors(),
            CreateTranslationErrors(),
            CreateSubmissionErrors(),
            CreateLyricsRevisionErrors()
        );
    }

    /// <summary>
    /// Creates a real <see cref="IdentityI18n"/> instance for use in Identity handler tests.
    /// </summary>
    public static IdentityI18n CreateIdentityI18n()
    {
        return new IdentityI18n(CreateUserErrors(), CreateSessionErrors());
    }

    /// <summary>
    /// Creates a real <see cref="CoreI18n"/> instance for use in Core handler tests.
    /// </summary>
    public static CoreI18n CreateCoreI18n()
    {
        return new CoreI18n(CreateFileErrors());
    }
}
