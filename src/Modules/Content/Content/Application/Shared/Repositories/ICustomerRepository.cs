using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for B2B customer data access operations.
/// </summary>
public interface ICustomerRepository : IRepository<CustomerEntity>
{
    /// <summary>
    /// Retrieves a paginated list of customers ordered by most recently created first.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of customers and the total count.</returns>
    Task<(List<CustomerEntity> Customers, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a customer by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a customer by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the customer is not found.</exception>
    Task<CustomerEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a customer by email address. Returns null if not found.
    /// </summary>
    Task<CustomerEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Adds a new customer to the repository.</summary>
    Task AddAsync(CustomerEntity customer, CancellationToken cancellationToken = default);
}
