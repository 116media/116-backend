using System.Data.Common;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Identity module.
/// Coordinates saving changes across all repositories that share the IdentityDbContext.
/// </summary>
/// <param name="context">The identity database context.</param>
/// <param name="participants">The module contexts sharing this scope's connection.</param>
public class IdentityUnitOfWork(IdentityDbContext context, IEnumerable<DbContext> participants)
    : UnitOfWorkBase<IdentityDbContext>(context, participants),
        IIdentityUnitOfWork;
