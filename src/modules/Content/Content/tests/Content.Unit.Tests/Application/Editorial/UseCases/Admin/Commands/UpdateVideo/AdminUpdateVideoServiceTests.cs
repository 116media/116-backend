using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoService"/>: the category and slug gates and the verbs applied.
/// </summary>
public class AdminUpdateVideoServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly AdminUpdateVideoService _service;

    public AdminUpdateVideoServiceTests()
    {
        _service = new AdminUpdateVideoService(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminUpdateVideoCommand Command(VideoEntity video, string slug)
    {
        return new AdminUpdateVideoCommand(
            video.Id.ToString(),
            video.CategoryId,
            "New title",
            slug,
            "Description",
            null,
            null,
            false,
            null,
            null
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldApplyTheVerbs()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        VideoEntity video = VideoFactory.CreateWithSlug(category.Id, "old-slug");
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug("new-slug", null);

        // Act
        VideoEntity result = await _service.UpdateAsync(Command(video, "new-slug"), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(video);
        video.Title.Should().Be("New title");
        video.Slug.Value.Should().Be("new-slug");
    }

    [Fact]
    public async Task UpdateAsync_WhenTheNewSlugBelongsToAnotherVideo_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        VideoEntity video = VideoFactory.CreateWithSlug(category.Id, "old-slug");
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug("new-slug", VideoFactory.CreateWithSlug(category.Id, "new-slug"));

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(Command(video, "new-slug"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }
}
