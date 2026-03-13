using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminRejectArticleResponse"/>.
/// </summary>
public class AdminRejectArticleEndpointV1Tests
{
    [Fact]
    public void AdminRejectArticleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminRejectArticleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
