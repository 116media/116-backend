using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPaymentsResponse"/>.
/// </summary>
public class AdminGetAllPaymentsEndpointV1Tests
{
    [Fact]
    public void AdminGetAllPaymentsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginatedResult = new PaginatedResult<PaymentSummaryDto>(0, 10, 0, []);

        // Act
        var response = new AdminGetAllPaymentsResponse(Payments: paginatedResult);

        // Assert
        response.Payments.Should().NotBeNull();
        response.Payments.Should().Be(paginatedResult);
    }
}
