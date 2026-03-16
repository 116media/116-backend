using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminPublishArticleResponse"/>.
/// </summary>
public class AdminPublishArticleEndpointV1Tests
{
    [Fact]
    public void AdminPublishArticleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminPublishArticleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
