using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;

/// <summary>
/// Unit tests for <see cref="RemovePackageSlotResponse"/>.
/// </summary>
public class RemovePackageSlotEndpointV1Tests
{
    [Fact]
    public void RemovePackageSlotResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PackageDto package = CreatePackageDto();

        // Act
        var response = new RemovePackageSlotResponse(Package: package, IsSuccess: true);

        // Assert
        response.Package.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }

    private static PackageDto CreatePackageDto() => new(Guid.NewGuid(), "Starter Pack", null, 49.99m, true, []);
}
