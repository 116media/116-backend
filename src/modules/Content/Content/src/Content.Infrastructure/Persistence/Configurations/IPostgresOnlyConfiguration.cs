namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Marks a configuration that maps PostgreSQL-specific types and so can only join the
/// model on that provider. Tests running the context on another provider skip it.
/// </summary>
public interface IPostgresOnlyConfiguration;
