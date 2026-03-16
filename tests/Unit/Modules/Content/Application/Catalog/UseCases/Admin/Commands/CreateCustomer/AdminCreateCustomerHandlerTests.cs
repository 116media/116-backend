using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Unit tests for <see cref="AdminCreateCustomerHandler"/>.
/// </summary>
public class AdminCreateCustomerHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateCustomerHandler _handler;

    public AdminCreateCustomerHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateCustomerHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEmailDoesNotExist_ShouldCreateAndReturnCustomer()
    {
        // Arrange
        string email = TestConstants.Content.Customer.ValidEmail;
        string fullName = TestConstants.Content.Customer.ValidFullName;

        var command = new AdminCreateCustomerCommand(
            FullName: fullName,
            Email: email,
            Phone: TestConstants.Content.Customer.ValidPhone,
            Company: TestConstants.Content.Customer.ValidCompany,
            Notes: TestConstants.Content.Customer.ValidNotes
        );

        _customerRepositoryMock.SetupGetByEmail(email, null);

        // Act
        AdminCreateCustomerResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Customer.Should().NotBeNull();
        result.Customer.Email.Should().Be(email);
        result.Customer.FullName.Should().Be(fullName);

        _customerRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string email = TestConstants.Content.Customer.ValidEmail;

        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: email,
            Phone: null,
            Company: null,
            Notes: null
        );

        CustomerEntity existing = CustomerFactory.Create(email);
        _customerRepositoryMock.SetupGetByEmail(email, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenEmailConflicts_ShouldNotAddOrCommit()
    {
        // Arrange
        string email = TestConstants.Content.Customer.ValidEmail;

        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: email,
            Phone: null,
            Company: null,
            Notes: null
        );

        CustomerEntity existing = CustomerFactory.Create(email);
        _customerRepositoryMock.SetupGetByEmail(email, existing);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _customerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
