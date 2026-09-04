using System.Data.Common;
using _116.Mailer.Application.Shared.Persistence;
using _116.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Mailer.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Mailer module.
/// Delegates commit operations to the underlying <see cref="MailerDbContext" />.
/// </summary>
/// <param name="context">The mailer database context.</param>
/// <param name="participants">The module contexts sharing this scope's connection.</param>
public class MailerUnitOfWork(MailerDbContext context, IEnumerable<DbContext> participants)
    : UnitOfWorkBase<MailerDbContext>(context, participants),
        IMailerUnitOfWork;
