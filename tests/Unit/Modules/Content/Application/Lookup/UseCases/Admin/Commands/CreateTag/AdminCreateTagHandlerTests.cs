using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;
using _116.Content.Application.Shared.Cache;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Unit tests for <see cref="AdminCreateTagHandler"/>.
/// </summary>
public class AdminCreateTagHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPopularTagsCacheInvalidator> _cacheInvalidatorMock;
    private readonly AdminCreateTagHandler _handler;

    public AdminCreateTagHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _cacheInvalidatorMock = MockPopularTagsCacheInvalidator.Create();
        _handler = new AdminCreateTagHandler(
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheInvalidatorMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenSlugDoesNotExist_ShouldCreateAndReturnDto()
    {
        // Arrange
        string name = TestConstants.Content.Tag.ValidName;
        string slug = TestConstants.Content.Tag.ValidSlug;
        var command = new AdminCreateTagCommand(Name: name, Slug: slug);

        _lookupRepositoryMock.SetupGetTagBySlug(slug, null);

        // Act
        AdminCreateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tag.Name.Should().Be(name);
        result.Tag.Slug.Should().Be(slug);

        _lookupRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string slug = TestConstants.Content.Tag.ValidSlug;
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: slug);

        TagEntity existingTag = TagFactory.Create(TestConstants.Content.Tag.AnotherValidName, slug);
        _lookupRepositoryMock.SetupGetTagBySlug(slug, existingTag);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldNotAddCommitOrInvalidate()
    {
        // Arrange
        string slug = TestConstants.Content.Tag.ValidSlug;
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: slug);

        TagEntity existingTag = TagFactory.Create(TestConstants.Content.Tag.AnotherValidName, slug);
        _lookupRepositoryMock.SetupGetTagBySlug(slug, existingTag);

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
        _lookupRepositoryMock.VerifyAddTagNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
        _cacheInvalidatorMock.VerifyInvalidateNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string slug = TestConstants.Content.Tag.ValidSlug;
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: slug);

        _lookupRepositoryMock.SetupGetTagBySlug(slug, null);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _lookupRepositoryMock.Verify(x => x.GetTagBySlugAsync(slug, cts.Token), Times.Once);
    }

    #endregion
}
