using _116.Content.Application.Shared.Errors;

namespace _116.Content.Application.Shared.Errors.Facade;

/// <summary>
/// Single i18n entry point for the Content module.
/// Inject this in every Content validator and handler instead of individual
/// <c>*Errors</c> classes.
/// </summary>
public class ContentI18n(
    ArticleErrors article,
    VideoErrors video,
    ShortVideoErrors shortVideo,
    LyricsErrors lyrics,
    CategoryErrors category,
    TagErrors tag,
    ContentTypeErrors contentType,
    PricingTierErrors pricingTier,
    PackageErrors package,
    CustomerErrors customer,
    ContentOrderErrors contentOrder,
    PlaylistErrors playlist,
    ArticleInteractionErrors articleInteraction,
    ShortVideoInteractionErrors shortVideoInteraction,
    PromotionLevelErrors promotionLevel
)
{
    /// <summary>Article domain errors and messages.</summary>
    public ArticleErrors Article => article;

    /// <summary>Video domain errors and messages.</summary>
    public VideoErrors Video => video;

    /// <summary>Short video domain errors and messages.</summary>
    public ShortVideoErrors ShortVideo => shortVideo;

    /// <summary>Lyrics domain errors and messages.</summary>
    public LyricsErrors Lyrics => lyrics;

    /// <summary>Category domain errors and messages.</summary>
    public CategoryErrors Category => category;

    /// <summary>Tag domain errors and messages.</summary>
    public TagErrors Tag => tag;

    /// <summary>Content type domain errors and messages.</summary>
    public ContentTypeErrors ContentType => contentType;

    /// <summary>Pricing tier domain errors and messages.</summary>
    public PricingTierErrors PricingTier => pricingTier;

    /// <summary>Package domain errors and messages.</summary>
    public PackageErrors Package => package;

    /// <summary>Customer domain errors and messages.</summary>
    public CustomerErrors Customer => customer;

    /// <summary>Content order domain errors and messages.</summary>
    public ContentOrderErrors ContentOrder => contentOrder;

    /// <summary>Playlist domain errors and messages.</summary>
    public PlaylistErrors Playlist => playlist;

    /// <summary>Article interaction domain errors and messages.</summary>
    public ArticleInteractionErrors ArticleInteraction => articleInteraction;

    /// <summary>Short video interaction domain errors and messages.</summary>
    public ShortVideoInteractionErrors ShortVideoInteraction => shortVideoInteraction;

    /// <summary>Promotion level domain errors and messages.</summary>
    public PromotionLevelErrors PromotionLevel => promotionLevel;
}
