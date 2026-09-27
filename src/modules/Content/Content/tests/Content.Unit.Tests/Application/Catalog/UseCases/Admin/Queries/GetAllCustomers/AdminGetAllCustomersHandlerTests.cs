using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;

/// <summary>
/// Unit tests for <see cref="AdminGetAllCustomersHandler"/>.
/// </summary>
public class AdminGetAllCustomersHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly AdminGetAllCustomersHandler _handler;

    public AdminGetAllCustomersHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _handler = new AdminGetAllCustomersHandler(_customerRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultipleCustomers_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<CustomerEntity> customers = CustomerFactory.CreateMany(3);
        int totalCount = 3;

        _customerRepositoryMock.SetupGetAllAsync(customers, totalCount);

        var query = new AdminGetAllCustomersQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        AdminGetAllCustomersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Customers.Items.Should().HaveCount(3);
        result.Customers.Count.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _customerRepositoryMock.SetupGetAllAsync(new List<CustomerEntity>(), 0);

        var query = new AdminGetAllCustomersQuery(PaginatedRequest: new PaginatedRequest(0, 10));

        // Act
        AdminGetAllCustomersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Customers.Items.Should().BeEmpty();
        result.Customers.Count.Should().Be(0);
    }

    #endregion
}
