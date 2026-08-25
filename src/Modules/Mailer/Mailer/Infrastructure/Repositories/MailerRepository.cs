using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Repositories;

namespace _116.Mailer.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IMailerRepository{TEntity}" />, registered
/// open-generic behind the interface.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <param name="context">The Mailer module database context.</param>
public class MailerRepository<TEntity>(MailerDbContext context)
    : RepositoryBase<MailerDbContext, TEntity, Guid>(context),
        IMailerRepository<TEntity>
    where TEntity : class, IEntity<Guid>;
