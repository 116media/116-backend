using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IContentTypeRepository"/>.
/// </summary>
public static class MockContentTypeRepository
{
    /// <summary>
    /// Creates a new mock instance of IContentTypeRepository with default setups.
    /// </summary>
    public static Mock<IContentTypeRepository> Create()
    {
        Mock<IContentTypeRepository> mock = new();
        mock.Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        mock.Setup(x => x.AddAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentTypeEntity>());
        return mock;
    }

    public static Mock<IContentTypeRepository> SetupContentTypeExistsByName(
        this Mock<IContentTypeRepository> mock,
        string name,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<IContentTypeRepository> SetupGetContentTypeByIdOrThrow(
        this Mock<IContentTypeRepository> mock,
        ContentTypeEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IContentTypeRepository> SetupGetContentTypeByIdOrThrowNotFound(
        this Mock<IContentTypeRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"ContentType with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IContentTypeRepository> SetupGetAllContentTypes(
        this Mock<IContentTypeRepository> mock,
        IReadOnlyList<ContentTypeEntity> list
    )
    {
        mock.Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static Mock<IContentTypeRepository> SetupGetActiveContentTypes(
        this Mock<IContentTypeRepository> mock,
        IReadOnlyList<ContentTypeEntity> list
    )
    {
        mock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddContentTypeCalled(this Mock<IContentTypeRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddContentTypeNotCalled(this Mock<IContentTypeRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
