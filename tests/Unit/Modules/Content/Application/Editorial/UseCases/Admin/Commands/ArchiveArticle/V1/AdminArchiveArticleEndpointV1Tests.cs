using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminArchiveArticleResponse"/>.
/// </summary>
public class AdminArchiveArticleEndpointV1Tests
{
    [Fact]
    public void AdminArchiveArticleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminArchiveArticleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
