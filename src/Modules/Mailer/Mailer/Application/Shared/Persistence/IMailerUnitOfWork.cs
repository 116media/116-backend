using _116.Shared.Application.Persistence;

namespace _116.Mailer.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Mailer module.
/// Coordinates saving changes across all repositories that share the MailerDbContext.
/// </summary>
public interface IMailerUnitOfWork : IUnitOfWork { }
