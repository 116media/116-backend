using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Storage.Application.Shared.Persistence;
using _116.Storage.Domain.Entities;
using _116.Storage.Infrastructure.Persistence;
using _116.Storage.TestData.Factories;

namespace _116.Shared.Integration.Tests.Infrastructure.Persistence;

/// <summary>
/// Integration tests proving one transaction covers writes to more than one module context,
/// which is what lets an upload and the row referencing it commit together.
/// </summary>
[Collection("Database")]
public class CrossContextTransactionTests(PostgresFixture db) : BaseRepositoryTest(db)
{
    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldCommitWritesAcrossBothContexts()
    {
        // Arrange
        using IServiceScope scope = Api.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IStorageUnitOfWork>();
        var coreContext = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        var contentContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        FileEntity file = FileFactory.CreateImage();
        TagEntity tag = TagFactory.Create();

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            coreContext.Files.Add(file);
            contentContext.Tags.Add(tag);

            return Task.CompletedTask;
        });

        // Assert
        await using StorageDbContext verifyStorage = CreateDbContext<StorageDbContext>();
        await using ContentDbContext verifyContent = CreateDbContext<ContentDbContext>();

        (await verifyStorage.Files.AnyAsync(f => f.Id == file.Id)).Should().BeTrue();
        (await verifyContent.Tags.AnyAsync(t => t.Id == tag.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationThrows_ShouldRollBackBothContexts()
    {
        // Arrange
        using IServiceScope scope = Api.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IStorageUnitOfWork>();
        var coreContext = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        var contentContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        FileEntity file = FileFactory.CreateImage();
        TagEntity tag = TagFactory.Create();

        // Act
        Func<Task> act = () =>
            unitOfWork.ExecuteInTransactionAsync(_ =>
            {
                coreContext.Files.Add(file);
                contentContext.Tags.Add(tag);

                throw new InvalidOperationException("the referencing write failed");
            });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        await using StorageDbContext verifyStorage = CreateDbContext<StorageDbContext>();
        await using ContentDbContext verifyContent = CreateDbContext<ContentDbContext>();

        (await verifyStorage.Files.AnyAsync(f => f.Id == file.Id)).Should().BeFalse();
        (await verifyContent.Tags.AnyAsync(t => t.Id == tag.Id)).Should().BeFalse();
    }
}
