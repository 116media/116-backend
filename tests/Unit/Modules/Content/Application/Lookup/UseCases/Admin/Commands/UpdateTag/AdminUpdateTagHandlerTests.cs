using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Unit tests for <see cref="AdminUpdateTagHandler"/>.
/// </summary>
public class AdminUpdateTagHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateTagHandler _handler;

    public AdminUpdateTagHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateTagHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenTagExistsAndSlugIsAvailable_ShouldUpdateAndReturnDto()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string newName = TestConstants.Content.Tag.AnotherValidName;
        string newSlug = TestConstants.Content.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(Id: tag.Id.ToString(), Name: newName, Slug: newSlug);

        _lookupRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _lookupRepositoryMock.SetupGetTagBySlug(slug: newSlug, tag: null);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tag.Name.Should().Be(newName);
        result.Tag.Slug.Should().Be(newSlug);

        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNameChangedButSlugUnchanged_ShouldSucceed()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string newName = TestConstants.Content.Tag.AnotherValidName;
        string currentSlug = TestConstants.Content.Tag.ValidSlug;
        var command = new AdminUpdateTagCommand(Id: tag.Id.ToString(), Name: newName, Slug: currentSlug);

        _lookupRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _lookupRepositoryMock.SetupGetTagBySlug(slug: currentSlug, tag: tag);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tag.Name.Should().Be(newName);
        result.Tag.Slug.Should().Be(currentSlug);

        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSlugSameAsCurrent_ShouldNotThrowConflict()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Content.Tag.ValidName,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        _lookupRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _lookupRepositoryMock.SetupGetTagBySlug(slug: TestConstants.Content.Tag.ValidSlug, tag: tag);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateTagCommand(
            Id: nonExistentId.ToString(),
            Name: TestConstants.Content.Tag.ValidName,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        _lookupRepositoryMock.SetupGetTagByIdOrThrowNotFound(id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyTakenByDifferentTag_ShouldThrowConflictException()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string conflictingSlug = TestConstants.Content.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Content.Tag.AnotherValidName,
            Slug: conflictingSlug
        );

        TagEntity existingTag = TagFactory.Create(
            name: TestConstants.Content.Tag.AnotherValidName,
            slug: conflictingSlug
        );

        _lookupRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _lookupRepositoryMock.SetupGetTagBySlug(slug: conflictingSlug, tag: existingTag);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugConflict_ShouldNotCommit()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string conflictingSlug = TestConstants.Content.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Content.Tag.AnotherValidName,
            Slug: conflictingSlug
        );

        TagEntity existingTag = TagFactory.Create(
            name: TestConstants.Content.Tag.AnotherValidName,
            slug: conflictingSlug
        );

        _lookupRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _lookupRepositoryMock.SetupGetTagBySlug(slug: conflictingSlug, tag: existingTag);

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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string newSlug = TestConstants.Content.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Content.Tag.AnotherValidName,
            Slug: newSlug
        );

        _lookupRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _lookupRepositoryMock.SetupGetTagBySlug(slug: newSlug, tag: null);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _lookupRepositoryMock.Verify(x => x.GetTagByIdOrThrowAsync(tag.Id, cts.Token), Times.Once);
        _lookupRepositoryMock.Verify(x => x.GetTagBySlugAsync(newSlug, cts.Token), Times.Once);
    }

    #endregion
}
