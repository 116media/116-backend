using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Unit tests for <see cref="AdminUpdateContentTypeHandler"/>.
/// </summary>
public class AdminUpdateContentTypeHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateContentTypeHandler _handler;

    public AdminUpdateContentTypeHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateContentTypeHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldUpdateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity existing = ContentTypeFactory.Create(TestConstants.Content.ContentType.ValidName);
        var command = new AdminUpdateContentTypeCommand(
            Id: existing.Id.ToString(),
            Name: TestConstants.Content.ContentType.AnotherValidName
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(existing);
        _lookupRepositoryMock.SetupContentTypeExistsByName(TestConstants.Content.ContentType.AnotherValidName, false);

        // Act
        AdminUpdateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ContentType.Name.Should().Be(TestConstants.Content.ContentType.AnotherValidName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNewNameIsSameAsCurrentName_ShouldAllowUpdate()
    {
        // Arrange
        ContentTypeEntity existing = ContentTypeFactory.Create(TestConstants.Content.ContentType.ValidName);
        var command = new AdminUpdateContentTypeCommand(
            Id: existing.Id.ToString(),
            Name: TestConstants.Content.ContentType.ValidName
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(existing);
        // Same name exists — but it belongs to the same entity so allowed
        _lookupRepositoryMock.SetupContentTypeExistsByName(TestConstants.Content.ContentType.ValidName, true);

        // Act
        AdminUpdateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
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
            Name: TestConstants.Content.ContentType.AnotherValidName
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNewNameConflictsWithDifferentEntity_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity existing = ContentTypeFactory.Create(TestConstants.Content.ContentType.ValidName);
        string conflictingName = TestConstants.Content.ContentType.AnotherValidName;
        var command = new AdminUpdateContentTypeCommand(Id: existing.Id.ToString(), Name: conflictingName);

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(existing);
        _lookupRepositoryMock.SetupContentTypeExistsByName(conflictingName, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
