using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Persistence;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Core;

namespace _116.Integration.Tests.Shared.Infrastructure.Persistence;

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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<ICoreUnitOfWork>();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
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
        await using CoreDbContext verifyCore = CreateDbContext<CoreDbContext>();
        await using ContentDbContext verifyContent = CreateDbContext<ContentDbContext>();

        (await verifyCore.Files.AnyAsync(f => f.Id == file.Id)).Should().BeTrue();
        (await verifyContent.Tags.AnyAsync(t => t.Id == tag.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationThrows_ShouldRollBackBothContexts()
    {
        // Arrange
        using IServiceScope scope = Api.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<ICoreUnitOfWork>();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
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

        await using CoreDbContext verifyCore = CreateDbContext<CoreDbContext>();
        await using ContentDbContext verifyContent = CreateDbContext<ContentDbContext>();

        (await verifyCore.Files.AnyAsync(f => f.Id == file.Id)).Should().BeFalse();
        (await verifyContent.Tags.AnyAsync(t => t.Id == tag.Id)).Should().BeFalse();
    }
}
