using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Persistence;
using _116.Storage.Domain.Entities;
using _116.Storage.Infrastructure.Persistence;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

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
        TagEntity tag = TagEntity.Create(Guid.NewGuid(), $"tag-{Guid.NewGuid():N}", $"slug-{Guid.NewGuid():N}");

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            coreContext.Files.Add(file);
            contentContext.Tags.Add(tag);

            return Task.CompletedTask;
        });

        // Assert
        await using StorageDbContext verifyCore = CreateDbContext<StorageDbContext>();
        await using ContentDbContext verifyContent = CreateDbContext<ContentDbContext>();

        (await verifyCore.Files.AnyAsync(f => f.Id == file.Id)).Should().BeTrue();
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
        TagEntity tag = TagEntity.Create(Guid.NewGuid(), $"tag-{Guid.NewGuid():N}", $"slug-{Guid.NewGuid():N}");

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

        await using StorageDbContext verifyCore = CreateDbContext<StorageDbContext>();
        await using ContentDbContext verifyContent = CreateDbContext<ContentDbContext>();

        (await verifyCore.Files.AnyAsync(f => f.Id == file.Id)).Should().BeFalse();
        (await verifyContent.Tags.AnyAsync(t => t.Id == tag.Id)).Should().BeFalse();
    }
}
