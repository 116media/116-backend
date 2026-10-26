using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;
using _116.Content.Application.Shared.Helpers;
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

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Unit tests for <see cref="AdminCreateTagHandler"/>.
/// </summary>
public class AdminCreateTagHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateTagHandler _handler;

    public AdminCreateTagHandlerTests()
    {
        _tagRepositoryMock = MockTagRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateTagHandler(_tagRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenSlugDoesNotExist_ShouldCreateAndReturnDto()
    {
        // Arrange
        string name = TestConstants.Tag.ValidName;
        var command = new AdminCreateTagCommand(Name: name);

        _tagRepositoryMock.SetupGetTagBySlug(It.IsAny<string>(), null);

        // Act
        AdminCreateTagResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Tag.Name.Should().Be(name);
        result.Tag.Slug.Should().MatchRegex($"^{SlugHelper.ToSlug(command.Name)}-[a-z0-9]{{8}}$");

        _tagRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases
    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: TestConstants.Tag.ValidName);

        _tagRepositoryMock.SetupGetTagBySlug(It.IsAny<string>(), null);
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _tagRepositoryMock.Verify(x => x.GetBySlugAsync(It.IsAny<string>(), cts.Token), Times.Once);
    }

    #endregion
}
