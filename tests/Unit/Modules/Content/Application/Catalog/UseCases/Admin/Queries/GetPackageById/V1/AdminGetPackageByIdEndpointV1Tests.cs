using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetPackageByIdResponse"/>.
/// </summary>
public class AdminGetPackageByIdEndpointV1Tests
{
    [Fact]
    public void AdminGetPackageByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PackageDto package = CreatePackageDto();

        // Act
        var response = new AdminGetPackageByIdResponse(Package: package);

        // Assert
        response.Package.Should().NotBeNull();
        response.Package.Should().Be(package);
    }

    private static PackageDto CreatePackageDto() => new(Guid.NewGuid(), "Starter Pack", null, 49.99m, true, []);
}
