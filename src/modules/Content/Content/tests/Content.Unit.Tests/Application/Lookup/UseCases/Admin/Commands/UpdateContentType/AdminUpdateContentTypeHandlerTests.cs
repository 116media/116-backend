using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Unit tests for <see cref="AdminUpdateContentTypeHandler"/>.
/// </summary>
public class AdminUpdateContentTypeHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateContentTypeHandler _handler;

    public AdminUpdateContentTypeHandlerTests()
    {
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateContentTypeHandler(
            _contentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldUpdateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity existing = ContentTypeFactory.Create(TestConstants.ContentType.ValidName);
        var command = new AdminUpdateContentTypeCommand(
            Id: existing.Id.ToString(),
            Name: TestConstants.ContentType.AnotherValidName
        );

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(existing);
        _contentTypeRepositoryMock.SetupContentTypeExistsByName(TestConstants.ContentType.AnotherValidName, false);

        // Act
        AdminUpdateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ContentType.Name.Should().Be(TestConstants.ContentType.AnotherValidName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNewNameIsSameAsCurrentName_ShouldAllowUpdate()
    {
        // Arrange
        ContentTypeEntity existing = ContentTypeFactory.Create(TestConstants.ContentType.ValidName);
        var command = new AdminUpdateContentTypeCommand(
            Id: existing.Id.ToString(),
            Name: TestConstants.ContentType.ValidName
        );

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(existing);
        _contentTypeRepositoryMock.SetupContentTypeExistsByName(TestConstants.ContentType.ValidName, true);

        // Act
        AdminUpdateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ContentType.Name.Should().Be(TestConstants.ContentType.ValidName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateContentTypeCommand(
            Id: nonExistentId.ToString(),
            Name: TestConstants.ContentType.AnotherValidName
        );

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNewNameConflictsWithDifferentEntity_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity existing = ContentTypeFactory.Create(TestConstants.ContentType.ValidName);
        string conflictingName = TestConstants.ContentType.AnotherValidName;
        var command = new AdminUpdateContentTypeCommand(Id: existing.Id.ToString(), Name: conflictingName);

        _contentTypeRepositoryMock.SetupGetContentTypeByIdOrThrow(existing);
        _contentTypeRepositoryMock.SetupContentTypeExistsByName(conflictingName, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
