using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;
using _116.Content.Application.Shared.Persistence;
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
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

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
        _handler = new AdminCreateCustomerHandler(
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEmailDoesNotExist_ShouldCreateAndReturnCustomer()
    {
        // Arrange
        string email = TestConstants.Customer.ValidEmail;
        string fullName = TestConstants.Customer.ValidFullName;

        var command = new AdminCreateCustomerCommand(
            FullName: fullName,
            Email: email,
            Phone: TestConstants.Customer.ValidPhone,
            Company: TestConstants.Customer.ValidCompany,
            Notes: TestConstants.Customer.ValidNotes
        );

        _customerRepositoryMock.SetupGetByEmail(email, null);

        // Act
        AdminCreateCustomerResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        string email = TestConstants.Customer.ValidEmail;

        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
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
        string email = TestConstants.Customer.ValidEmail;

        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
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
        _customerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
