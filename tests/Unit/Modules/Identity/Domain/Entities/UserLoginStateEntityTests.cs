using _116.Identity.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="UserLoginStateEntity"/>.
/// </summary>
public class UserLoginStateEntityTests
{
    [Fact]
    public void Create_ShouldUseTheUserIdAsIdentity()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserLoginStateEntity.Create(userId);

        // Assert
        state.Id.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldStartWithNoFailuresRecorded()
    {
        // Act
        var state = UserLoginStateEntity.Create(Guid.NewGuid());

        // Assert
        state.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldStartUnlocked()
    {
        // Act
        var state = UserLoginStateEntity.Create(Guid.NewGuid());

        // Assert
        state.LockedUntil.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldKeepRecordsForDifferentUsersDistinct()
    {
        // Act
        var first = UserLoginStateEntity.Create(Guid.NewGuid());
        var second = UserLoginStateEntity.Create(Guid.NewGuid());

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
