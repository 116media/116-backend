using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;
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
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Unit tests for <see cref="AdminUpdateTagHandler"/>.
/// </summary>
public class AdminUpdateTagHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateTagHandler _handler;

    public AdminUpdateTagHandlerTests()
    {
        _tagRepositoryMock = MockTagRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateTagHandler(
            _tagRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenTagExistsAndSlugIsAvailable_ShouldUpdateAndReturnDto()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string newName = TestConstants.Tag.AnotherValidName;
        string newSlug = TestConstants.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(Id: tag.Id.ToString(), Name: newName, Slug: newSlug);

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: newSlug, tag: null);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Tag.Name.Should().Be(newName);
        result.Tag.Slug.Should().Be(newSlug);

        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNameChangedButSlugUnchanged_ShouldSucceed()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string newName = TestConstants.Tag.AnotherValidName;
        string currentSlug = TestConstants.Tag.ValidSlug;
        var command = new AdminUpdateTagCommand(Id: tag.Id.ToString(), Name: newName, Slug: currentSlug);

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: currentSlug, tag: tag);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
            Name: TestConstants.Tag.ValidName,
            Slug: TestConstants.Tag.ValidSlug
        );

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: TestConstants.Tag.ValidSlug, tag: tag);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Tag.Slug.Should().Be(TestConstants.Tag.ValidSlug);
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
            Name: TestConstants.Tag.ValidName,
            Slug: TestConstants.Tag.ValidSlug
        );

        _tagRepositoryMock.SetupGetTagByIdOrThrowNotFound(id: nonExistentId);

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
        string conflictingSlug = TestConstants.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Tag.AnotherValidName,
            Slug: conflictingSlug
        );

        TagEntity existingTag = TagFactory.Create(name: TestConstants.Tag.AnotherValidName, slug: conflictingSlug);

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: conflictingSlug, tag: existingTag);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugConflict_ShouldNotCommitOrInvalidate()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string conflictingSlug = TestConstants.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Tag.AnotherValidName,
            Slug: conflictingSlug
        );

        TagEntity existingTag = TagFactory.Create(name: TestConstants.Tag.AnotherValidName, slug: conflictingSlug);

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: conflictingSlug, tag: existingTag);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        string newSlug = TestConstants.Tag.AnotherValidSlug;
        var command = new AdminUpdateTagCommand(
            Id: tag.Id.ToString(),
            Name: TestConstants.Tag.AnotherValidName,
            Slug: newSlug
        );

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: newSlug, tag: null);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _tagRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(tag.Id, cts.Token), Times.Once);
        _tagRepositoryMock.Verify(x => x.GetBySlugAsync(newSlug, cts.Token), Times.Once);
    }

    #endregion
}
