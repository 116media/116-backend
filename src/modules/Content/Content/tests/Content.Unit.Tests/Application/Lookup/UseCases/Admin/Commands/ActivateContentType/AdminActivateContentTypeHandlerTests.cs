using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Unit tests for <see cref="AdminActivateContentTypeHandler"/>.
/// </summary>
public class AdminActivateContentTypeHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminActivateContentTypeHandler _handler;

    public AdminActivateContentTypeHandlerTests()
    {
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminActivateContentTypeHandler(
            _contentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldActivateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity inactive = ContentTypeFactory.CreateInactive();
        var command = new AdminActivateContentTypeCommand(Id: inactive.Id.ToString());

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(inactive);

        // Act
        AdminActivateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ContentType.IsActive.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity active = ContentTypeFactory.CreateDefault();
        var command = new AdminActivateContentTypeCommand(Id: active.Id.ToString());

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(active);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldNotCommit()
    {
        // Arrange
        ContentTypeEntity active = ContentTypeFactory.CreateDefault();
        var command = new AdminActivateContentTypeCommand(Id: active.Id.ToString());

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(active);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AdminActivateContentTypeCommand(Id: nonExistentId.ToString());

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
