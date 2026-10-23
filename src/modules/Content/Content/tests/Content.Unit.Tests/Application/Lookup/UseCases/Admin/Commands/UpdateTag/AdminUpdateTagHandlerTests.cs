using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;
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
        var command = new AdminUpdateTagCommand(Id: tag.Id.ToString(), Name: newName);

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: It.IsAny<string>(), tag: null);

        // Act
        AdminUpdateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Tag.Name.Should().Be(newName);
        result.Tag.Slug.Should().Be(tag.Slug.Value);

        _unitOfWorkMock.VerifyCommitCalled();
    }
    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateTagCommand(Id: nonExistentId.ToString(), Name: TestConstants.Tag.ValidName);

        _tagRepositoryMock.SetupGetTagByIdOrThrowNotFound(id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var command = new AdminUpdateTagCommand(Id: tag.Id.ToString(), Name: TestConstants.Tag.AnotherValidName);

        _tagRepositoryMock.SetupGetTagByIdOrThrow(entity: tag);
        _tagRepositoryMock.SetupGetTagBySlug(slug: It.IsAny<string>(), tag: null);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _tagRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(tag.Id, cts.Token), Times.Once);
    }

    #endregion
}
