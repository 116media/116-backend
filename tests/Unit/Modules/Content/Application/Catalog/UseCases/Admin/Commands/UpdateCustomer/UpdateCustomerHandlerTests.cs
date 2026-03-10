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
/// Unit tests for <see cref="UpdateCustomerHandler"/>.
/// </summary>
public class UpdateCustomerHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCustomerHandler _handler;

    public UpdateCustomerHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new UpdateCustomerHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCustomerExists_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var customer = CustomerFactory.CreateDefault();
        string newFullName = "Jane Smith";

        var command = new UpdateCustomerCommand(
            Id: customer.Id,
            FullName: newFullName,
            Phone: "+123456789",
            Company: "New Company",
            Notes: "Updated notes"
        );

        _customerRepositoryMock.SetupGetByIdOrThrow(customer);

        // Act
        UpdateCustomerResult result = await _handler.Handle(command, CancellationToken.None);

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
        Guid nonExistentId = Guid.NewGuid();

        var command = new UpdateCustomerCommand(
            Id: nonExistentId,
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
