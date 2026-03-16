using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Unit tests for <see cref="AdminGetCustomerByIdHandler"/>.
/// </summary>
public class AdminGetCustomerByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly AdminGetCustomerByIdHandler _handler;

    public AdminGetCustomerByIdHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _handler = new AdminGetCustomerByIdHandler(_customerRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCustomerFound_ShouldReturnDto()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.CreateDefault();
        _customerRepositoryMock.SetupGetByIdOrThrow(customer);

        var query = new AdminGetCustomerByIdQuery(Id: customer.Id.ToString());

        // Act
        AdminGetCustomerByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Customer.Should().NotBeNull();
        result.Customer.Id.Should().Be(customer.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _customerRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        var query = new AdminGetCustomerByIdQuery(Id: nonExistentId.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
