using Microsoft.Extensions.Logging;
using _116.Shared.Infrastructure.Seed;
using _116.Auth.Application.Shared.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace _116.Auth.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Orchestrator for Super Admin seeding operations.
/// Implements the Facade pattern to provide a simplified interface for complex seeding operations.
/// Uses Dependency Injection and follows SOLID principles.
/// </summary>
public class SuperAdminSeeder : IDataSeeder
{
    private readonly SuperAdminRepositoryManager _repositoryManager;
    private readonly SuperAdminSeedingStrategy _seedingStrategy;
    private readonly ILogger<SuperAdminSeeder> _logger;

    public SuperAdminSeeder(
        AuthDbContext context,
        IPasswordService passwordService,
        ILogger<SuperAdminSeeder> logger,
        ILogger<SuperAdminRepositoryManager> repositoryLogger,
        ILogger<SuperAdminSeedingStrategy> strategyLogger
    )
    {
        _logger = logger;
        _repositoryManager = new SuperAdminRepositoryManager(context, repositoryLogger);

        var entityFactory = new SuperAdminEntityFactory(passwordService);
        _seedingStrategy = new SuperAdminSeedingStrategy(entityFactory, _repositoryManager, strategyLogger);
    }

    /// <inheritdoc />
    /// <summary>
    /// Executes the Super Admin seeding process using the orchestrated components.
    /// Implements the Template Method pattern with proper error handling and transaction management.
    /// </summary>
    public async Task SeedAllAsync()
    {
        try
        {
            _logger.LogInformation("Starting Super Admin seeding process...");

            // Check if seeding is necessary
            if (await _repositoryManager.SuperAdminExistsAsync())
            {
                _logger.LogInformation("Super Admin user already exists. Skipping seeding.");
                return;
            }

            // Execute seeding within a transaction (UnitOfWork pattern)
            await ExecuteSeedingWithTransactionAsync();

            _logger.LogInformation("Super Admin seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed Super Admin data");
            throw;
        }
    }

    /// <summary>
    /// Executes the seeding process within a database transaction.
    /// Implements UnitOfWork pattern to ensure data consistency.
    /// </summary>
    private async Task ExecuteSeedingWithTransactionAsync()
    {
        await using IDbContextTransaction transaction = await _repositoryManager.BeginTransactionAsync();

        try
        {
            // Execute the seeding strategy
            await _seedingStrategy.ExecuteSeedingAsync();

            // Commit all changes
            await _repositoryManager.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Super Admin seeding transaction committed successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred during Super Admin seeding. Transaction rolled back.");
            throw;
        }
    }
}
