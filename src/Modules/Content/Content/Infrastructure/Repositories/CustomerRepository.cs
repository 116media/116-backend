using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ICustomerRepository" /> for managing B2B customer entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class CustomerRepository(ContentDbContext context)
    : ContentRepository<CustomerEntity>(context),
        ICustomerRepository
{
    /// <inheritdoc />
    public async Task<(List<CustomerEntity> Customers, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        int totalCount = await Context.Customers.CountAsync(cancellationToken);

        List<CustomerEntity> customers = await Context
            .Customers.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (customers, totalCount);
    }

    /// <inheritdoc />
    public async Task<CustomerEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Customers.FirstOrDefaultAsync(
            customer => EF.Functions.ILike(customer.Email, email),
            cancellationToken
        );
    }
}
