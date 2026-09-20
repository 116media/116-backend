using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IPromotionLevelRepository"/>.
/// </summary>
public static class MockPromotionLevelRepository
{
    /// <summary>
    /// Creates a new mock instance of IPromotionLevelRepository with default setups.
    /// </summary>
    public static Mock<IPromotionLevelRepository> Create()
    {
        Mock<IPromotionLevelRepository> mock = new();
        mock.Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        mock.Setup(x => x.AddAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PromotionLevelEntity>());
        mock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PromotionLevelEntity>());
        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PromotionLevelEntity>());
        return mock;
    }

    /// <summary>
    /// Arranges the batch lookup to resolve exactly the supplied rows, keyed by id.
    /// </summary>
    public static Mock<IPromotionLevelRepository> SetupGetByIds(
        this Mock<IPromotionLevelRepository> mock,
        params PromotionLevelEntity[] entities
    )
    {
        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities.ToDictionary(entity => entity.Id));
        return mock;
    }

    public static Mock<IPromotionLevelRepository> SetupPromotionLevelExistsByName(
        this Mock<IPromotionLevelRepository> mock,
        string name,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<IPromotionLevelRepository> SetupGetPromotionLevelByIdOrThrow(
        this Mock<IPromotionLevelRepository> mock,
        PromotionLevelEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IPromotionLevelRepository> SetupGetPromotionLevelByIdOrThrowNotFound(
        this Mock<IPromotionLevelRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"PromotionLevel with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IPromotionLevelRepository> SetupGetAllPromotionLevels(
        this Mock<IPromotionLevelRepository> mock,
        IReadOnlyList<PromotionLevelEntity> list
    )
    {
        mock.Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static Mock<IPromotionLevelRepository> SetupGetActivePromotionLevels(
        this Mock<IPromotionLevelRepository> mock,
        IReadOnlyList<PromotionLevelEntity> list
    )
    {
        mock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddPromotionLevelCalled(this Mock<IPromotionLevelRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddPromotionLevelNotCalled(this Mock<IPromotionLevelRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
