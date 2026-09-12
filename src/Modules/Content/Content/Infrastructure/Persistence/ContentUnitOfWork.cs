using System.Data.Common;
using _116.Content.Application.Shared.Persistence;
using _116.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Content module.
/// Delegates commit operations to the underlying <see cref="ContentDbContext" />.
/// </summary>
/// <param name="context">The content database context.</param>
/// <param name="participants">The module contexts sharing this scope's connection.</param>
public class ContentUnitOfWork(ContentDbContext context, IEnumerable<DbContext> participants)
    : UnitOfWorkBase<ContentDbContext>(context, participants),
        IContentUnitOfWork;
