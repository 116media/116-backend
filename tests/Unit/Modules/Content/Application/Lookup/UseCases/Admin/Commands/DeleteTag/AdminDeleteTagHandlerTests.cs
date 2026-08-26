using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Unit tests for <see cref="AdminDeleteTagHandler"/>.
/// </summary>
public class AdminDeleteTagHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteTagHandler _handler;

    public AdminDeleteTagHandlerTests()
    {
        _tagRepositoryMock = MockTagRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeleteTagHandler(_tagRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenTagExists_ShouldRemoveAndReturnSuccess()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var command = new AdminDeleteTagCommand(Id: tag.Id.ToString());

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _tagRepositoryMock.VerifyRemoveTagCalled(tag: tag);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteTagCommand(Id: nonExistentId.ToString());

        _tagRepositoryMock.SetupGetTagByIdOrThrowNotFound(id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldNotRemoveCommitOrInvalidate()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteTagCommand(Id: nonExistentId.ToString());

        _tagRepositoryMock.SetupGetTagByIdOrThrowNotFound(id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _tagRepositoryMock.VerifyRemoveTagNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var command = new AdminDeleteTagCommand(Id: tag.Id.ToString());

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _tagRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(tag.Id, cts.Token), Times.Once);
    }

    #endregion
}
