using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminSubmitArticleResponse"/>.
/// </summary>
public class AdminSubmitArticleEndpointV1Tests
{
    [Fact]
    public void AdminSubmitArticleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminSubmitArticleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
