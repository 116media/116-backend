namespace _116.Mailer.Contracts.Domain;

/// <summary>
/// Catalog of every templated email the platform sends. Each member maps to a
/// subject/html/text resource triple in the Mailer module's template resources.
/// The enum is append-only: members are added when a real flow needs them and
/// never renumbered or removed.
/// </summary>
public enum EnumEmailTemplate
{
    /// <summary>
    /// OTP delivery for email verification during public signup.
    /// </summary>
    EmailVerificationOtp,

    /// <summary>
    /// OTP delivery for a password reset request.
    /// </summary>
    PasswordResetOtp,

    /// <summary>
    /// Welcome email after the first successful email verification.
    /// </summary>
    Welcome,

    /// <summary>
    /// Security alert for a successful login from a new device or address.
    /// </summary>
    LoginAlert,

    /// <summary>
    /// Newsletter double opt-in confirmation request.
    /// </summary>
    NewsletterConfirm,

    /// <summary>
    /// Welcome email after a confirmed newsletter subscription, carrying the
    /// unsubscribe link.
    /// </summary>
    NewsletterWelcome,

    /// <summary>
    /// Security confirmation after a password change with the old password known.
    /// </summary>
    PasswordChanged,

    /// <summary>
    /// Security confirmation after a completed OTP-driven password reset.
    /// </summary>
    PasswordResetCompleted,

    /// <summary>
    /// Security confirmation after a social-login account gains a local password.
    /// </summary>
    LocalPasswordAdded,

    /// <summary>
    /// Security alert to the previous address after the account email changed.
    /// </summary>
    EmailChangedAlertOld,

    /// <summary>
    /// Confirmation to the new address after the account email changed.
    /// </summary>
    EmailChangedConfirmNew,

    /// <summary>
    /// Security confirmation after every session was terminated by the user.
    /// </summary>
    SignedOutAllDevices,

    /// <summary>
    /// Notice that an administrator terminated every session on the account.
    /// </summary>
    AccountForceLoggedOut,

    /// <summary>
    /// Notice that a role was granted to or revoked from the account.
    /// </summary>
    RoleChanged,

    /// <summary>
    /// Invoice-style payment request when an order is submitted for payment.
    /// </summary>
    OrderInvoice,

    /// <summary>
    /// Receipt after a payment is verified and the order marked paid.
    /// </summary>
    PaymentReceipt,

    /// <summary>
    /// Notice that a submitted payment proof was rejected, with the review notes.
    /// </summary>
    PaymentRejected,

    /// <summary>
    /// Notice that an order was cancelled.
    /// </summary>
    OrderCancelled,

    /// <summary>
    /// Notice that a paid promoted placement was removed early, with the reason.
    /// </summary>
    PromotionForceRemoved,

    /// <summary>
    /// Fulfillment notice that commissioned content went live, with its public URL.
    /// </summary>
    CommissionedContentPublished,

    /// <summary>
    /// Notice that commissioned content was rejected in editorial review.
    /// </summary>
    CommissionedContentRejected,

    /// <summary>
    /// Notice of the scheduled shoot date for a pre-booked video production.
    /// </summary>
    ShootScheduled,

    /// <summary>
    /// Engagement notification that someone replied to the user's comment.
    /// </summary>
    CommentReply,

    /// <summary>
    /// Security alert after a revoked refresh token was presented again; every session on the
    /// account was terminated as a precaution.
    /// </summary>
    RefreshTokenReplayAlert,

    /// <summary>
    /// Notice to the proposer that a lyrics or translation correction revision was decided,
    /// with the decision and a link to the corrected page.
    /// </summary>
    RevisionDecided,

    /// <summary>
    /// Notice to the submitter that a community lyrics submission was decided, carrying the
    /// moderator's review note.
    /// </summary>
    SubmissionDecided,

    /// <summary>
    /// Confirmation to the new owner that an artist profile ownership claim was verified.
    /// </summary>
    ArtistVerified,
}
