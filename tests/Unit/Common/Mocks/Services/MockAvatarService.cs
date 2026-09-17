using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Application.User.Services;
using _116.Tests.Fixtures.Factories.Core;
using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Mock for <see cref="IAvatarService" />, Identity's avatar workflow.
/// </summary>
public static class MockAvatarService
{
    /// <summary>
    /// Creates a service mock that resolves no avatar.
    /// </summary>
    /// <returns>The configured mock.</returns>
    public static Mock<IAvatarService> Create()
    {
        Mock<IAvatarService> mock = new();
        mock.Setup(x => x.GetAvatarAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileDto?)null);
        return mock;
    }

    /// <summary>
    /// Makes the service resolve this avatar.
    /// </summary>
    /// <param name="mock">The service mock.</param>
    /// <param name="avatarFileId">The avatar in use.</param>
    /// <param name="avatar">The DTO to resolve to.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IAvatarService> SetupGetAvatar(
        this Mock<IAvatarService> mock,
        Guid? avatarFileId,
        FileDto avatar
    )
    {
        mock.Setup(x => x.GetAvatarAsync(avatarFileId, It.IsAny<CancellationToken>())).ReturnsAsync(avatar);
        return mock;
    }

    /// <summary>
    /// Makes the service resolve no avatar for this id.
    /// </summary>
    /// <param name="mock">The service mock.</param>
    /// <param name="avatarFileId">The id that resolves to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IAvatarService> SetupGetAvatarReturnsNull(this Mock<IAvatarService> mock, Guid? avatarFileId)
    {
        mock.Setup(x => x.GetAvatarAsync(avatarFileId, It.IsAny<CancellationToken>())).ReturnsAsync((FileDto?)null);
        return mock;
    }

    /// <summary>
    /// Makes an avatar upload return this handle and record to its reference.
    /// </summary>
    /// <param name="mock">The service mock.</param>
    /// <param name="stored">The handle uploads return.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IAvatarService> SetupUpload(this Mock<IAvatarService> mock, StoredFile stored)
    {
        mock.Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        mock.Setup(x => x.UploadFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        mock.Setup(x => x.RecordAsync(It.IsAny<StoredFile>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored.Reference);
        return mock;
    }
}
