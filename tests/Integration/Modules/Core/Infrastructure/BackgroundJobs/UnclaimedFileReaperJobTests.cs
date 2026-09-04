using _116.Core.Domain.Constants;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.BackgroundJobs;
using _116.Core.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Tests.Fixtures.Factories.Core;

namespace _116.Integration.Tests.Modules.Core.Infrastructure.BackgroundJobs;

/// <summary>
/// Integration tests for <see cref="UnclaimedFileReaperJob" />: an upload no referencing write
/// ever claimed is swept once the grace period passes, while claimed and recent uploads stay.
/// </summary>
[Collection("Database")]
public class UnclaimedFileReaperJobTests(PostgresFixture db) : BaseRepositoryTest(db)
{
    /// <summary>
    /// Runs the reaper once through its real entry point.
    /// </summary>
    private async Task RunReaperAsync()
    {
        using IServiceScope scope = Api.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<UnclaimedFileReaperJob>();

        await job.Execute(new TestJobExecutionContext());
    }

    /// <summary>
    /// Writes a file row, optionally aged past the grace period and optionally claimed.
    /// </summary>
    /// <param name="stale">Whether the row predates the grace period.</param>
    /// <param name="claimed">Whether a referencing write claimed it.</param>
    /// <returns>The persisted file id.</returns>
    private async Task<Guid> SeedFileAsync(bool stale, bool claimed)
    {
        await using CoreDbContext context = CreateDbContext<CoreDbContext>();

        FileEntity file = FileFactory.CreateImage();
        if (claimed)
        {
            file.Claim(DateTime.UtcNow);
        }

        context.Files.Add(file);
        await context.SaveChangesAsync();

        if (stale)
        {
            DateTime createdAt = DateTime.UtcNow - CoreConstants.UnclaimedFileGracePeriod - TimeSpan.FromHours(1);
            await context
                .Files.Where(candidate => candidate.Id == file.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(candidate => candidate.CreatedAt, createdAt));
        }

        return file.Id;
    }

    /// <summary>
    /// Reads whether a file row is still live.
    /// </summary>
    /// <param name="fileId">The file to check.</param>
    /// <returns>True when the row is present and not soft-deleted.</returns>
    private async Task<bool> IsLiveAsync(Guid fileId)
    {
        await using CoreDbContext context = CreateDbContext<CoreDbContext>();

        return await context.Files.AnyAsync(file => file.Id == fileId);
    }

    [Fact]
    public async Task Execute_WithAnUnclaimedUploadPastTheGracePeriod_ShouldReapIt()
    {
        // Arrange
        // Nothing ever referenced this upload, which is the state a failed referencing write
        // leaves behind.
        Guid abandoned = await SeedFileAsync(stale: true, claimed: false);

        // Act
        await RunReaperAsync();

        // Assert
        (await IsLiveAsync(abandoned))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task Execute_WithAClaimedUpload_ShouldLeaveItAlone()
    {
        // Arrange
        // The claim is what marks an upload as referenced; reaping it would delete a live asset.
        Guid claimed = await SeedFileAsync(stale: true, claimed: true);

        // Act
        await RunReaperAsync();

        // Assert
        (await IsLiveAsync(claimed))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Execute_WithAnUnclaimedUploadInsideTheGracePeriod_ShouldLeaveItAlone()
    {
        // Arrange
        // A referencing write still in flight has not claimed yet, so the grace period is the
        // only thing keeping the reaper off it.
        Guid recent = await SeedFileAsync(stale: false, claimed: false);

        // Act
        await RunReaperAsync();

        // Assert
        (await IsLiveAsync(recent))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Execute_WithNothingAbandoned_ShouldLeaveEveryFileLive()
    {
        // Arrange
        Guid claimed = await SeedFileAsync(stale: true, claimed: true);
        Guid recent = await SeedFileAsync(stale: false, claimed: false);

        // Act
        await RunReaperAsync();

        // Assert
        (await IsLiveAsync(claimed))
            .Should()
            .BeTrue();
        (await IsLiveAsync(recent)).Should().BeTrue();
    }
}
