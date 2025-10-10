using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Enums;

namespace _116.Auth.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing OTP entities and verification operations.
/// Provides methods for OTP creation, validation, and cleanup.
/// </summary>
public interface IOtpRepository : IRepository<OtpEntity>
{
    /// <summary>
    /// Adds a new OTP entity to the repository.
    /// </summary>
    /// <param name="otp">The OTP entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method only adds the OTP to the context. Call <see cref="SaveChangesAsync"/> to persist changes.
    /// </remarks>
    Task AddAsync(OtpEntity otp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest valid OTP for a user and specific purpose.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The latest valid OTP entity if found; otherwise, null.</returns>
    /// <remarks>
    /// This method returns the most recently created valid OTP that hasn't expired or been used.
    /// </remarks>
    Task<OtpEntity?> GetLatestValidOtpAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an OTP code for a specific user and purpose.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="code">The OTP code to validate.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The valid OTP entity if validation succeeds.</returns>
    /// <exception cref="NotFoundException">Thrown when no valid OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    /// <remarks>
    /// This method finds an OTP that matches the code, user, and purpose, validates it,
    /// and throws appropriate exceptions for different failure scenarios.
    /// </remarks>
    Task<OtpEntity> ValidateOtpAsync(Guid userId, string code, OtpPurpose purpose, CancellationToken cancellationToken = default);

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
    /// This method verifies that an OTP was already successfully used by the user.
    /// Useful for operations that require prior OTP verification (e.g., password reset after OTP verification).
    /// </remarks>
    Task<OtpEntity> ValidateUsedOtpAsync(
        Guid userId,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Invalidates all existing OTPs for a user and specific purpose.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="purpose">The purpose of the OTPs to invalidate.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method marks all existing OTPs for the user and purpose as used to prevent reuse.
    /// Useful when generating a new OTP to replace existing ones.
    /// </remarks>
    Task InvalidateExistingOtpsAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Updates an existing OTP entity in the repository.
    /// </summary>
    /// <param name="otp">The OTP entity to update.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method marks the OTP as modified in the context. Call UnitOfWork.CommitAsync() to persist changes.
    /// </remarks>
    Task UpdateAsync(OtpEntity otp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes in the repository to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method commits all pending changes (adds, updates, deletes) to the database.
    /// Should be called after performing repository operations that need persistence.
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
