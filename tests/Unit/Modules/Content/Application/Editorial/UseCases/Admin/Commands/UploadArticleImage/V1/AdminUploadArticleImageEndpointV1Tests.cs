using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage.V1;

/// <summary>
/// Unit tests for <see cref="AdminUploadArticleImageResponse"/>.
/// </summary>
public class AdminUploadArticleImageEndpointV1Tests
{
    [Fact]
    public void AdminUploadArticleImageResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ArticleImageDto dto = CreateArticleImageDto();

        // Act
        var response = new AdminUploadArticleImageResponse(Image: dto);

        // Assert
        response.Should().NotBeNull();
        response.Image.Should().Be(dto);
    }

    private static ArticleImageDto CreateArticleImageDto() =>
        new(Id: Guid.NewGuid(), Url: "Test", StorageKey: "Test", ImageType: EnumArticleImageType.Cover);
}
