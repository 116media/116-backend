using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeleteArticleResponse"/>.
/// </summary>
public class AdminDeleteArticleEndpointV1Tests
{
    [Fact]
    public void AdminDeleteArticleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminDeleteArticleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
