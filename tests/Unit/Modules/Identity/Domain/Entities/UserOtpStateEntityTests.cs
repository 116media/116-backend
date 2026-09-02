using _116.Identity.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="UserOtpStateEntity" />. The record is read-only by design — the
/// counters move through atomic SQL updates — so the factory is the only behavior to prove.
/// </summary>
public class UserOtpStateEntityTests
{
    [Fact]
    public void Create_ShouldUseTheUserIdAsIdentity()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserOtpStateEntity.Create(userId: userId);

        // Assert
        state.Id.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldStartWithNoFailuresRecorded()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserOtpStateEntity.Create(userId: userId);

        // Assert
        state.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldStartUnlocked()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserOtpStateEntity.Create(userId: userId);

        // Assert
        state.LockedUntil.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldKeepRecordsForDifferentUsersDistinct()
    {
        // Arrange & Act
        var first = UserOtpStateEntity.Create(userId: Guid.NewGuid());
        var second = UserOtpStateEntity.Create(userId: Guid.NewGuid());

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
