using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateVideoService"/>: the category and slug gates and the staged video.
/// </summary>
public class AdminCreateVideoServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly AdminCreateVideoService _service;

    public AdminCreateVideoServiceTests()
    {
        _service = new AdminCreateVideoService(_categoryRepositoryMock.Object, _videoRepositoryMock.Object);
    }

    private static AdminCreateVideoCommand Command(Guid categoryId, DateTimeOffset? shootingScheduledAt = null)
    {
        return new AdminCreateVideoCommand(
            categoryId,
            "Title",
            Guid.NewGuid(),
            null,
            null,
            "Description",
            shootingScheduledAt
        );
    }

    [Fact]
    public async Task CreateAsync_WithAShootingDate_ShouldScheduleTheShoot()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        DateTimeOffset shootingAt = DateTimeOffset.UtcNow.AddDays(3);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug("video-slug", null);

        // Act
        VideoEntity video = await _service.CreateAsync(Command(category.Id, shootingAt), CancellationToken.None);

        // Assert
        video.ShootingScheduledAt.Should().Be(shootingAt);
    }
}
