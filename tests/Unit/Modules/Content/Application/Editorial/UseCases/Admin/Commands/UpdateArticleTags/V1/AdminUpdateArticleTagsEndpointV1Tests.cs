using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleTagsResponse"/>.
/// </summary>
public class AdminUpdateArticleTagsEndpointV1Tests
{
    [Fact]
    public void AdminUpdateArticleTagsResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminUpdateArticleTagsResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
