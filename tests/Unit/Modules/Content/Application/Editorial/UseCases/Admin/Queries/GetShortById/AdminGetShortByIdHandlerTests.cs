using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Unit tests for <see cref="AdminGetShortByIdHandler"/>.
/// </summary>
public class AdminGetShortByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly AdminGetShortByIdHandler _handler;

    public AdminGetShortByIdHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _handler = new AdminGetShortByIdHandler(_shortVideoRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenShortVideoExists_ShouldReturnShortVideoDetail()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new AdminGetShortByIdQuery(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminGetShortByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.Should().NotBeNull();
        result.ShortVideo.Id.Should().Be(shortVideo.Id);
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var query = new AdminGetShortByIdQuery(Id: nonExistentId.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
