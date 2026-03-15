using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPackagesResponse"/>.
/// </summary>
public class AdminGetAllPackagesEndpointV1Tests
{
    [Fact]
    public void AdminGetAllPackagesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<PackageDto>(1, 10, 1, [CreatePackageDto()]);

        // Act
        var response = new AdminGetAllPackagesResponse(Packages: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Packages.Should().Be(paginated);
    }

    private static PackageDto CreatePackageDto() => new(Guid.NewGuid(), "Starter Pack", null, 49.99m, true, []);
}
