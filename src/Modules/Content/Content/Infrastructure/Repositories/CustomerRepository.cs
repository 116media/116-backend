using _116.Content.Application.Catalog.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ICustomerRepository" /> for managing B2B customer entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class CustomerRepository(ContentDbContext context) : ICustomerRepository
{
    /// <inheritdoc />
    public async Task<(List<CustomerEntity> Customers, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        int totalCount = await context.Customers.CountAsync(cancellationToken);

        List<CustomerEntity> customers = await context
            .Customers.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (customers, totalCount);
    }

    /// <inheritdoc />
    public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new CustomerByIdSpecification(id: id);
        return await context.Customers.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<CustomerEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new CustomerByIdSpecification(id: id);
        return await context
            .Customers.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CustomerEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var specification = new CustomerByEmailSpecification(email: email);
        return await context.Customers.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddAsync(CustomerEntity customer, CancellationToken cancellationToken = default)
    {
        await context.Customers.AddAsync(customer, cancellationToken);
    }
}
