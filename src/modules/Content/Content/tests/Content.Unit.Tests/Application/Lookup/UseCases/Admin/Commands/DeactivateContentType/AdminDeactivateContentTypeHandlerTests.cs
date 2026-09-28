using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateContentTypeHandler"/>.
/// </summary>
public class AdminDeactivateContentTypeHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivateContentTypeHandler _handler;

    public AdminDeactivateContentTypeHandlerTests()
    {
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeactivateContentTypeHandler(
            _contentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenActive_ShouldDeactivateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity active = ContentTypeFactory.CreateDefault();
        var command = new AdminDeactivateContentTypeCommand(Id: active.Id.ToString());

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(active);

        // Act
        AdminDeactivateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(inactive);

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

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
