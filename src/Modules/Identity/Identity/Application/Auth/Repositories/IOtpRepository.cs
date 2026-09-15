using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;

namespace _116.Identity.Application.Auth.Repositories;

/// <summary>
/// Repository interface for managing OTP entities and verification operations.
/// Provides methods for OTP creation, validation, and cleanup.
/// </summary>
public interface IOtpRepository
{
    /// <summary>
    /// Adds a new OTP entity to the repository.
    /// </summary>
    /// <param name="otp">The OTP entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method only adds the OTP to the context. Call UnitOfWork.CommitAsync() to persist changes.
    /// </remarks>
    Task AddAsync(OtpEntity otp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the most recent outstanding OTP for a user and purpose. Judging a presented code
    /// against it is the caller's job through <see cref="OtpEntity.Verify" />.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The outstanding OTP entity.</returns>
    /// <exception cref="NotFoundException">Thrown when no outstanding OTP exists.</exception>
    Task<OtpEntity> GetLatestOutstandingOtpOrThrowAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Validates that an OTP code was previously used for a specific user and purpose.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="code">The OTP code to validate.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The used OTP entity if validation succeeds.</returns>
    /// <exception cref="NotFoundException">Thrown when no matching OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid or not yet used.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <remarks>
    /// This method loads the most recently consumed OTP for the user and purpose and compares the
    /// supplied code against the stored hash, confirming that the OTP was already successfully used.
    /// Useful for operations that require prior OTP verification (e.g., password reset after OTP verification).
    /// </remarks>
    Task<OtpEntity> ValidateUsedOtpAsync(
        Guid userId,
        string code,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the codes issued to an account for a purpose inside the resend window, so a caller
    /// can refuse to mint more than the cap allows.
    /// </summary>
    /// <param name="userId">The account to count for.</param>
    /// <param name="purpose">The OTP purpose to count.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of codes issued inside the window.</returns>
    Task<int> CountRecentOtpsAsync(Guid userId, EnumOtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks every outstanding OTP for the user and purpose consumed, so a superseded code can
    /// never be presented again.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="purpose">The purpose of the OTPs to invalidate.</param>
    /// <param name="exceptOtpId">
    /// A code to leave untouched, used by verification so the code being redeemed is not consumed
    /// alongside the ones it supersedes.
    /// </param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task InvalidateExistingOtpsAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        Guid? exceptOtpId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes expired OTPs from the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of expired OTPs removed.</returns>
    /// <remarks>
    /// This method performs cleanup of expired OTPs to maintain database performance.
    /// Should be called periodically via a background service.
    /// </remarks>
    Task<int> CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default);
}
