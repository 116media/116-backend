using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminApproveArticleResponse"/>.
/// </summary>
public class AdminApproveArticleEndpointV1Tests
{
    [Fact]
    public void AdminApproveArticleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminApproveArticleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
