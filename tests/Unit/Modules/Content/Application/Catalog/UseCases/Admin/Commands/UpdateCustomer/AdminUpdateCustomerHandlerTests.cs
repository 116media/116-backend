using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCustomerHandler"/>.
/// </summary>
public class AdminUpdateCustomerHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateCustomerHandler _handler;

    public AdminUpdateCustomerHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateCustomerHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCustomerExists_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var customer = CustomerFactory.CreateDefault();
        string newFullName = "Jane Smith";

        var command = new AdminUpdateCustomerCommand(
            Id: customer.Id.ToString(),
            FullName: newFullName,
            Phone: "+123456789",
            Company: "New Company",
            Notes: "Updated notes"
        );

        _customerRepositoryMock.SetupGetByIdOrThrow(customer);

        // Act
        AdminUpdateCustomerResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Customer.Should().NotBeNull();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var command = new AdminUpdateCustomerCommand(
            Id: nonExistentId.ToString(),
            FullName: TestConstants.Content.Customer.ValidFullName,
            Phone: null,
            Company: null,
            Notes: null
        );

        _customerRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
