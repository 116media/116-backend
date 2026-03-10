using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;

/// <summary>
/// Unit tests for <see cref="AddPackageSlotResponse"/>.
/// </summary>
public class AddPackageSlotEndpointV1Tests
{
    [Fact]
    public void AddPackageSlotResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PackageDto package = CreatePackageDto();

        // Act
        var response = new AddPackageSlotResponse(Package: package);

        // Assert
        response.Package.Should().NotBeNull();
        response.Package.Should().Be(package);
    }

    private static PackageDto CreatePackageDto() => new(Guid.NewGuid(), "Starter Pack", null, 49.99m, true, []);
}
