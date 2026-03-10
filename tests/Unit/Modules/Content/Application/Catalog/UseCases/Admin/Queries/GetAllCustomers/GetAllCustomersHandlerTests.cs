using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;

/// <summary>
/// Unit tests for <see cref="GetAllCustomersHandler"/>.
/// </summary>
public class GetAllCustomersHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly GetAllCustomersHandler _handler;

    public GetAllCustomersHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _handler = new GetAllCustomersHandler(_customerRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultipleCustomers_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<CustomerEntity> customers = CustomerFactory.CreateMany(3);
        int totalCount = 3;

        _customerRepositoryMock.SetupGetAllAsync(customers, totalCount);

        var query = new GetAllCustomersQuery(PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10));

        // Act
        GetAllCustomersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().NotBeNull();
        result.Customers.Items.Should().HaveCount(3);
        result.Customers.Count.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _customerRepositoryMock.SetupGetAllAsync(new List<CustomerEntity>(), 0);

        var query = new GetAllCustomersQuery(PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10));

        // Act
        GetAllCustomersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Customers.Items.Should().BeEmpty();
        result.Customers.Count.Should().Be(0);
    }

    #endregion
}
