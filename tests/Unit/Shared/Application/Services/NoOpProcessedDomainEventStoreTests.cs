using _116.Shared.Application.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Services;

/// <summary>
/// Unit tests for <see cref="NoOpProcessedDomainEventStore" />: the fallback used by hosts with
/// no module registered, which treats every invocation as a first run.
/// </summary>
public class NoOpProcessedDomainEventStoreTests
{
    private readonly NoOpProcessedDomainEventStore _store = new();

    [Fact]
    public async Task TryRecordAsync_ShouldTreatTheInvocationAsAFirstRun()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        bool fresh = await _store.TryRecordAsync(eventId, "SomeHandler", CancellationToken.None);

        // Assert
        fresh.Should().BeTrue();
    }

    [Fact]
    public async Task TryRecordAsync_ForTheSamePairTwice_ShouldStillReportAFirstRun()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        await _store.TryRecordAsync(eventId, "SomeHandler", CancellationToken.None);

        // Act
        bool second = await _store.TryRecordAsync(eventId, "SomeHandler", CancellationToken.None);

        // Assert
        second.Should().BeTrue();
    }
}
