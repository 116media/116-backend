using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Unit tests for <see cref="CreateContentTypeHandler"/>.
/// </summary>
public class CreateContentTypeHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly CreateContentTypeHandler _handler;

    public CreateContentTypeHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new CreateContentTypeHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNameDoesNotExist_ShouldCreateAndReturnDto()
    {
        // Arrange
        string name = TestConstants.Content.ContentType.ValidName;
        var command = new CreateContentTypeCommand(Name: name);

        _lookupRepositoryMock.SetupContentTypeExistsByName(name, false);

        // Act
        CreateContentTypeResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ContentType.Should().NotBeNull();
        result.ContentType.Name.Should().Be(name);
        result.ContentType.IsActive.Should().BeTrue();

        _lookupRepositoryMock.VerifyAddContentTypeCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string name = TestConstants.Content.ContentType.ValidName;
        var command = new CreateContentTypeCommand(Name: name);

        _lookupRepositoryMock.SetupContentTypeExistsByName(name, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        string name = TestConstants.Content.ContentType.ValidName;
        var command = new CreateContentTypeCommand(Name: name);

        _lookupRepositoryMock.SetupContentTypeExistsByName(name, true);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _lookupRepositoryMock.VerifyAddContentTypeNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string name = TestConstants.Content.ContentType.ValidName;
        var command = new CreateContentTypeCommand(Name: name);

        _lookupRepositoryMock.SetupContentTypeExistsByName(name, false);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _lookupRepositoryMock.Verify(x => x.ContentTypeExistsByNameAsync(name, cts.Token), Times.Once);
    }

    #endregion
}
