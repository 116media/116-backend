namespace _116.Mailer.Contracts.Domain;

/// <summary>
/// Catalog of every in-app notification the platform writes. Each member maps
/// to a title/body resource pair in the Mailer module's notification
/// resources. The enum is append-only: members are added when a real flow
/// needs them and never renumbered or removed.
/// </summary>
public enum EnumNotificationType
{
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
    /// Security alert after the account email address changed.
    /// </summary>
    EmailChanged,

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
    /// Engagement notification that someone replied to the user's comment.
    /// </summary>
    CommentReply,

    /// <summary>
    /// Notice that a community lyrics submission was decided — approved,
    /// rejected, or returned for revision.
    /// </summary>
    SubmissionDecided,

    /// <summary>
    /// Notice that a lyrics or translation correction revision was decided —
    /// accepted or rejected.
    /// </summary>
    RevisionDecided,

    /// <summary>
    /// Notice that an artist profile ownership claim was verified.
    /// </summary>
    ArtistVerified,
}
