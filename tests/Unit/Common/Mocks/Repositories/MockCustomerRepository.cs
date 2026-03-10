using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ICustomerRepository"/>.
/// </summary>
public static class MockCustomerRepository
{
    /// <summary>
    /// Creates a new mock instance of ICustomerRepository with default setups.
    /// </summary>
    public static Mock<ICustomerRepository> Create()
    {
        Mock<ICustomerRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ICustomerRepository> SetupGetByIdOrThrow(
        this Mock<ICustomerRepository> mock,
        CustomerEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ICustomerRepository> SetupGetByIdOrThrowNotFound(this Mock<ICustomerRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Customer with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ICustomerRepository> SetupGetByEmail(
        this Mock<ICustomerRepository> mock,
        string email,
        CustomerEntity? customer
    )
    {
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        return mock;
    }

    public static Mock<ICustomerRepository> SetupGetAllAsync(
        this Mock<ICustomerRepository> mock,
        List<CustomerEntity> list,
        int totalCount
    )
    {
        mock.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((list, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ICustomerRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetupDefaults(Mock<ICustomerRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);
        mock.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CustomerEntity>(), 0));
    }
}
