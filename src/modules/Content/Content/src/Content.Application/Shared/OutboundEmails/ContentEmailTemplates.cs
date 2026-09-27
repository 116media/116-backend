namespace _116.Content.Application.Shared.OutboundEmails;

/// <summary>
/// The message templates this module sends. Owning the names here keeps the module off a
/// shared cross-module enum.
/// </summary>
public static class ContentEmailTemplates
{
    /// <summary>
    /// The ArtistVerified template name, resolved from this module's resources.
    /// </summary>
    public const string ArtistVerified = "ArtistVerified";

    /// <summary>
    /// The CommentReply template name, resolved from this module's resources.
    /// </summary>
    public const string CommentReply = "CommentReply";

    /// <summary>
    /// The CommissionedContentPublished template name, resolved from this module's resources.
    /// </summary>
    public const string CommissionedContentPublished = "CommissionedContentPublished";

    /// <summary>
    /// The CommissionedContentRejected template name, resolved from this module's resources.
    /// </summary>
    public const string CommissionedContentRejected = "CommissionedContentRejected";

    /// <summary>
    /// The OrderCancelled template name, resolved from this module's resources.
    /// </summary>
    public const string OrderCancelled = "OrderCancelled";

    /// <summary>
    /// The OrderInvoice template name, resolved from this module's resources.
    /// </summary>
    public const string OrderInvoice = "OrderInvoice";

    /// <summary>
    /// The PaymentReceipt template name, resolved from this module's resources.
    /// </summary>
    public const string PaymentReceipt = "PaymentReceipt";

    /// <summary>
    /// The PaymentRejected template name, resolved from this module's resources.
    /// </summary>
    public const string PaymentRejected = "PaymentRejected";

    /// <summary>
    /// The PromotionForceRemoved template name, resolved from this module's resources.
    /// </summary>
    public const string PromotionForceRemoved = "PromotionForceRemoved";

    /// <summary>
    /// The RevisionDecided template name, resolved from this module's resources.
    /// </summary>
    public const string RevisionDecided = "RevisionDecided";

    /// <summary>
    /// The ShootScheduled template name, resolved from this module's resources.
    /// </summary>
    public const string ShootScheduled = "ShootScheduled";

    /// <summary>
    /// The SubmissionDecided template name, resolved from this module's resources.
    /// </summary>
    public const string SubmissionDecided = "SubmissionDecided";
}
