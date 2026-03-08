using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers.V1;

/// <summary>
/// Unit tests for <see cref="GetAllCustomersResponse"/>.
/// </summary>
public class GetAllCustomersEndpointV1Tests
{
    [Fact]
    public void GetAllCustomersResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<CustomerDto>(1, 10, 1, [CreateCustomerDto()]);

        // Act
        var response = new GetAllCustomersResponse(Customers: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().Be(paginated);
    }

    private static CustomerDto CreateCustomerDto() =>
        new(Guid.NewGuid(), "John Doe", "john@example.com", null, null, null);
}
