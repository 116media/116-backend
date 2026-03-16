using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetCustomerByIdResponse"/>.
/// </summary>
public class AdminGetCustomerByIdEndpointV1Tests
{
    [Fact]
    public void AdminGetCustomerByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CustomerDto customer = CreateCustomerDto();

        // Act
        var response = new AdminGetCustomerByIdResponse(Customer: customer);

        // Assert
        response.Customer.Should().NotBeNull();
        response.Customer.Should().Be(customer);
    }

    private static CustomerDto CreateCustomerDto() =>
        new(Guid.NewGuid(), "John Doe", "john@example.com", null, null, null);
}
