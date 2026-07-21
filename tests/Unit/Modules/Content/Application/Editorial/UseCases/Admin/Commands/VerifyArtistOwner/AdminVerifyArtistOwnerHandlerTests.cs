using _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;

/// <summary>
/// Unit tests for <see cref="AdminVerifyArtistOwnerHandler"/>.
/// </summary>
public class AdminVerifyArtistOwnerHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminVerifyArtistOwnerHandler _handler;

    public AdminVerifyArtistOwnerHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminVerifyArtistOwnerHandler(
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenArtistUnclaimed_ShouldClaimOwnership()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        Guid userId = Guid.NewGuid();
        var command = new AdminVerifyArtistOwnerCommand(artist.Id, userId);

        // Act
        AdminVerifyArtistOwnerResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        artist.UserId.Should().Be(userId);
        artist.VerifiedAt.Should().NotBeNull();
        result.Artist.Id.Should().Be(artist.Id);
        _artistRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistAlreadyClaimed_ShouldThrowConflictException()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateClaimed(Guid.NewGuid());
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminVerifyArtistOwnerCommand(artist.Id, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);
        var command = new AdminVerifyArtistOwnerCommand(nonExistentId, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
