namespace _116.Content.Domain.StateMachines;

/// <summary>
/// Stable identifiers for the content domain rules reported through
/// <see cref="Exceptions.ContentRuleException" />, scoped <c>content.&lt;entity&gt;.&lt;rule&gt;</c>.
/// </summary>
public static class ContentRuleCodes
{
    /// <summary>
    /// The requested publication-state move is not in the transition table.
    /// Args: [0] content type, [1] source status, [2] target status.
    /// </summary>
    public const string InvalidStatusTransition = "content.invalid-status-transition";

    /// <summary>
    /// The content has moved past review and can no longer be edited.
    /// Args: [0] content type, [1] current status.
    /// </summary>
    public const string NotEditable = "content.not-editable";

    /// <summary>
    /// A video cannot publish without a YouTube URL attached. Args: none.
    /// </summary>
    public const string PublicationRequiresYoutubeUrl = "content.publication-requires-youtube-url";

    /// <summary>
    /// A required article title was blank. Args: none.
    /// </summary>
    public const string ArticleTitleRequired = "content.article.title-required";

    /// <summary>
    /// A required article slug was blank. Args: none.
    /// </summary>
    public const string ArticleSlugRequired = "content.article.slug-required";

    /// <summary>
    /// The article carries no promotion to clear. Args: none.
    /// </summary>
    public const string ArticleNotPromoted = "content.article.not-promoted";

    /// <summary>
    /// A required video title was blank. Args: none.
    /// </summary>
    public const string VideoTitleRequired = "content.video.title-required";

    /// <summary>
    /// A required video slug was blank. Args: none.
    /// </summary>
    public const string VideoSlugRequired = "content.video.slug-required";

    /// <summary>
    /// The video carries no promotion to clear. Args: none.
    /// </summary>
    public const string VideoNotPromoted = "content.video.not-promoted";

    /// <summary>
    /// A YouTube URL cannot attach before the scheduled shoot date.
    /// Args: [0] scheduled shoot instant, ISO-8601 round-trip format.
    /// </summary>
    public const string CannotAttachYoutubeUrlBeforeShoot = "content.video.cannot-attach-youtube-url-before-shoot";

    /// <summary>
    /// A required lyrics slug was blank. Args: none.
    /// </summary>
    public const string LyricsSlugRequired = "content.lyrics.slug-required";

    /// <summary>
    /// A required song title was blank. Args: none.
    /// </summary>
    public const string SongTitleRequired = "content.lyrics.song-title-required";

    /// <summary>
    /// A required lyrics artist name was blank. Args: none.
    /// </summary>
    public const string LyricsArtistNameRequired = "content.lyrics.artist-name-required";

    /// <summary>
    /// A required lyrics text was blank. Args: none.
    /// </summary>
    public const string LyricsTextRequired = "content.lyrics.lyrics-text-required";

    /// <summary>
    /// The lyrics page carries no promotion to clear. Args: none.
    /// </summary>
    public const string LyricsNotPromoted = "content.lyrics.not-promoted";

    /// <summary>
    /// A required short video title was blank. Args: none.
    /// </summary>
    public const string ShortVideoTitleRequired = "content.short-video.title-required";

    /// <summary>
    /// A short video cannot activate without an uploaded video file. Args: none.
    /// </summary>
    public const string ShortVideoFileRequired = "content.short-video.video-file-required";

    /// <summary>
    /// A required album name was blank. Args: none.
    /// </summary>
    public const string AlbumNameRequired = "content.album.name-required";

    /// <summary>
    /// A required artist name was blank. Args: none.
    /// </summary>
    public const string ArtistNameRequired = "content.artist.name-required";

    /// <summary>
    /// A required artist slug was blank. Args: none.
    /// </summary>
    public const string ArtistSlugRequired = "content.artist.slug-required";

    /// <summary>
    /// An artist alias exceeded the maximum length. Args: none.
    /// </summary>
    public const string ArtistAliasTooLong = "content.artist.alias-too-long";

    /// <summary>
    /// The artist has reached the maximum number of aliases. Args: none.
    /// </summary>
    public const string ArtistTooManyAliases = "content.artist.too-many-aliases";

    /// <summary>
    /// An artist birthdate lies in the future. Args: none.
    /// </summary>
    public const string ArtistBirthdateInFuture = "content.artist.birthdate-in-future";

    /// <summary>
    /// The artist profile is already claimed by an owner. Args: none.
    /// </summary>
    public const string ArtistAlreadyClaimed = "content.artist.already-claimed";

    /// <summary>
    /// A required category name was blank. Args: none.
    /// </summary>
    public const string CategoryNameRequired = "content.category.name-required";

    /// <summary>
    /// A required category slug was blank. Args: none.
    /// </summary>
    public const string CategorySlugRequired = "content.category.slug-required";

    /// <summary>
    /// The category was not found. Args: [0] category id.
    /// </summary>
    public const string CategoryNotFound = "content.category.not-found";

    /// <summary>
    /// A category price must not be negative. Args: none.
    /// </summary>
    public const string CategoryPriceMustBeNonNegative = "content.category-pricing.price-must-be-non-negative";

    /// <summary>
    /// A required content type name was blank. Args: none.
    /// </summary>
    public const string ContentTypeNameRequired = "content.content-type.name-required";

    /// <summary>
    /// A required customer full name was blank. Args: none.
    /// </summary>
    public const string CustomerFullNameRequired = "content.customer.full-name-required";

    /// <summary>
    /// A required customer email was blank. Args: none.
    /// </summary>
    public const string CustomerEmailRequired = "content.customer.email-required";

    /// <summary>
    /// A required package name was blank. Args: none.
    /// </summary>
    public const string PackageNameRequired = "content.package.name-required";

    /// <summary>
    /// A package slot quantity must be positive. Args: none.
    /// </summary>
    public const string PackageSlotQuantityMustBePositive = "content.package-slot.quantity-must-be-positive";

    /// <summary>
    /// A required pricing tier name was blank. Args: none.
    /// </summary>
    public const string PricingTierNameRequired = "content.pricing-tier.name-required";

    /// <summary>
    /// A required promotion level name was blank. Args: none.
    /// </summary>
    public const string PromotionLevelNameRequired = "content.promotion-level.name-required";

    /// <summary>
    /// A promotion duration must be positive. Args: none.
    /// </summary>
    public const string PromotionLevelDurationMustBePositive = "content.promotion-level.duration-must-be-positive";

    /// <summary>
    /// A promotion price must not be negative. Args: none.
    /// </summary>
    public const string PromotionLevelPriceMustBeNonNegative = "content.promotion-level.price-must-be-non-negative";

    /// <summary>
    /// A promotion spot priority was out of range. Args: none.
    /// </summary>
    public const string PromotionLevelInvalidSpotPriority = "content.promotion-level.invalid-spot-priority";

    /// <summary>
    /// The promotion level was not found. Args: [0] promotion level id.
    /// </summary>
    public const string PromotionLevelNotFound = "content.promotion-level.not-found";

    /// <summary>
    /// The order was already submitted. Args: none.
    /// </summary>
    public const string OrderAlreadySubmitted = "content.order.already-submitted";

    /// <summary>
    /// The order was already paid. Args: none.
    /// </summary>
    public const string OrderAlreadyPaid = "content.order.already-paid";

    /// <summary>
    /// The order was already cancelled. Args: none.
    /// </summary>
    public const string OrderAlreadyCancelled = "content.order.already-cancelled";

    /// <summary>
    /// A paid order cannot be cancelled. Args: none.
    /// </summary>
    public const string CannotCancelPaidOrder = "content.order.cannot-cancel-paid-order";

    /// <summary>
    /// Items can only be added to a draft order. Args: none.
    /// </summary>
    public const string CannotAddItemToNonDraftOrder = "content.order.cannot-add-item-to-non-draft-order";

    /// <summary>
    /// A purchased promotion level's duration could not be resolved. Args: none.
    /// </summary>
    public const string PromotionDurationUnavailable = "content.order.promotion-duration-unavailable";

    /// <summary>
    /// The payment was already decided. Args: none.
    /// </summary>
    public const string PaymentAlreadyDecided = "content.payment.already-decided";

    /// <summary>
    /// The payment was already verified. Args: none.
    /// </summary>
    public const string PaymentAlreadyVerified = "content.payment.already-verified";

    /// <summary>
    /// The payment was already rejected. Args: none.
    /// </summary>
    public const string PaymentAlreadyRejected = "content.payment.already-rejected";

    /// <summary>
    /// A payment cannot verify without an uploaded proof. Args: none.
    /// </summary>
    public const string PaymentProofRequired = "content.payment.proof-required";

    /// <summary>
    /// A required tag name was blank. Args: none.
    /// </summary>
    public const string TagNameRequired = "content.tag.name-required";

    /// <summary>
    /// A required tag slug was blank. Args: none.
    /// </summary>
    public const string TagSlugRequired = "content.tag.slug-required";

    /// <summary>
    /// A share channel value is not a known channel. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidShareChannel = "content.share.invalid-channel";
}
