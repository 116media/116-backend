using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateContentTypeHandler"/>.
/// </summary>
public class AdminDeactivateContentTypeHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivateContentTypeHandler _handler;

    public AdminDeactivateContentTypeHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeactivateContentTypeHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenActive_ShouldDeactivateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity active = ContentTypeFactory.CreateDefault();
        var command = new AdminDeactivateContentTypeCommand(Id: active.Id.ToString());

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(active);

        // Act
        AdminDeactivateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ContentType.IsActive.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity inactive = ContentTypeFactory.CreateInactive();
        var command = new AdminDeactivateContentTypeCommand(Id: inactive.Id.ToString());

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(inactive);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AdminDeactivateContentTypeCommand(Id: nonExistentId.ToString());

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
