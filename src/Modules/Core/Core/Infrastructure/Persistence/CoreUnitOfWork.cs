using System.Data.Common;
using _116.Core.Application.Shared.Persistence;
using _116.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Core.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Core module.
/// Coordinates saving changes across all repositories that share the CoreDbContext.
/// </summary>
/// <param name="context">The core database context.</param>
/// <param name="participants">The module contexts sharing this scope's connection.</param>
public class CoreUnitOfWork(CoreDbContext context, IEnumerable<DbContext> participants)
    : UnitOfWorkBase<CoreDbContext>(context, participants),
        ICoreUnitOfWork;
