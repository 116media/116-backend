using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateShortVideoHandler"/>.
/// </summary>
public class AdminUpdateShortVideoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateShortVideoHandler _handler;

    public AdminUpdateShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        _handler = new AdminUpdateShortVideoHandler(
            _shortVideoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminUpdateShortVideoCommand BuildCommand(
        string? id = null,
        string? title = null,
        Guid? videoId = null
    ) =>
        new(Id: id ?? Guid.NewGuid().ToString(), Title: title ?? TestConstants.ShortVideo.ValidTitle, VideoId: videoId);

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateAndReturnShortVideo()
    {
        // Arrange
        ShortVideoEntity existing = ShortVideoFactory.Create();
        var command = BuildCommand(id: existing.Id.ToString());

        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        AdminUpdateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.Should().NotBeNull();
        result.ShortVideo.Title.Should().Be(command.Title);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithVideoId_ShouldLinkToParentVideo()
    {
        // Arrange
        ShortVideoEntity existing = ShortVideoFactory.Create();
        Guid parentVideoId = Guid.NewGuid();
        var command = BuildCommand(id: existing.Id.ToString(), videoId: parentVideoId);

        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        AdminUpdateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.HasFullVideo.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithNullVideoId_ShouldMakeStandalone()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        ShortVideoEntity existing = ShortVideoFactory.CreateTeaser(videoId);
        var command = BuildCommand(id: existing.Id.ToString(), videoId: null);

        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        AdminUpdateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.HasFullVideo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldNotUploadAnyVideoFile()
    {
        // Arrange
        ShortVideoEntity existing = ShortVideoFactory.Create();
        var command = BuildCommand(id: existing.Id.ToString());

        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileRepositoryMock.VerifyUploadAndStoreVideoFileNotCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var command = BuildCommand(id: id.ToString());

        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ShortVideo.NotFound", "Short video not found"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
