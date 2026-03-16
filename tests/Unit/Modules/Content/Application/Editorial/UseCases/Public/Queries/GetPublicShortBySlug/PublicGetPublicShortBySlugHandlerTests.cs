using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetPublicShortBySlugHandler"/>.
/// </summary>
public class PublicGetPublicShortBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly PublicGetPublicShortBySlugHandler _handler;

    public PublicGetPublicShortBySlugHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _handler = new PublicGetPublicShortBySlugHandler(_shortVideoRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenActiveShortVideoExists_ShouldReturnShortVideo()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create(); // active by default
        string slug = shortVideo.Slug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, shortVideo);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Content.Editorial.ShortVideo.ValidSlug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenShortVideoExistsButInactive_ShouldThrowNotFoundException()
    {
        // Arrange
        ShortVideoEntity inactiveShortVideo = ShortVideoFactory.CreateInactive();
        string slug = inactiveShortVideo.Slug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, inactiveShortVideo);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
