using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteVideoService"/>: the slug gate and the clocked unpromotion.
/// </summary>
public class AdminForceUnpromoteVideoServiceTests
{
    private readonly string _adminId = Guid.NewGuid().ToString();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly AdminForceUnpromoteVideoService _service;

    public AdminForceUnpromoteVideoServiceTests()
    {
        ICurrentActor currentActor = Mock.Of<ICurrentActor>(actor => actor.UserId == _adminId);
        _service = new AdminForceUnpromoteVideoService(
            _videoRepositoryMock.Object,
            currentActor,
            TestErrorsFactory.CreateContentI18n(),
            TimeProvider.System
        );
    }

    [Fact]
    public async Task UnpromoteAsync_WithAPromotedVideo_ShouldUnpromoteItOnBehalfOfTheAdmin()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(Guid.NewGuid());
        _videoRepositoryMock.SetupGetBySlug(video.Slug, video);

        // Act
        VideoEntity result = await _service.UnpromoteAsync(video.Slug, "Policy breach", CancellationToken.None);

        // Assert
        result.Should().BeSameAs(video);
        video.UnpromotedAt.Should().NotBeNull();
        video.UnpromotedBy.Should().Be(_adminId);
    }

    [Fact]
    public async Task UnpromoteAsync_WhenTheSlugIsUnknown_ShouldThrowNotFoundException()
    {
        // Arrange
        _videoRepositoryMock.SetupGetBySlug("missing", null);

        // Act
        Func<Task> act = async () => await _service.UnpromoteAsync("missing", "Policy breach", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
