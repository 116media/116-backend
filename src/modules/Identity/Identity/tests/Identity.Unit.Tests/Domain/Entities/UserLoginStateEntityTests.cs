using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Xunit;

namespace _116.Identity.Unit.Tests.Domain.Entities;

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
