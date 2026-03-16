using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateCustomerResponse"/> and <see cref="AdminCreateCustomerRequest"/>.
/// </summary>
public class AdminCreateCustomerEndpointV1Tests
{
    [Fact]
    public void AdminCreateCustomerResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CustomerDto customer = CreateCustomerDto();

        // Act
        var response = new AdminCreateCustomerResponse(Customer: customer);

        // Assert
        response.Customer.Should().NotBeNull();
        response.Customer.Should().Be(customer);
    }

    [Fact]
    public void AdminCreateCustomerRequest_WithAllValues_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminCreateCustomerRequest(
            FullName: "John Doe",
            Email: "john@example.com",
            Phone: "+1234567890",
            Company: "Acme Corp",
            Notes: "VIP customer"
        );

        // Assert
        request.FullName.Should().Be("John Doe");
        request.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void AdminCreateCustomerRequest_WithNullableFieldsNull_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminCreateCustomerRequest(
            FullName: "John Doe",
            Email: "john@example.com",
            Phone: null,
            Company: null,
            Notes: null
        );

        // Assert
        request.Phone.Should().BeNull();
        request.Company.Should().BeNull();
        request.Notes.Should().BeNull();
    }

    private static CustomerDto CreateCustomerDto() =>
        new(Guid.NewGuid(), "John Doe", "john@example.com", null, null, null);
}
