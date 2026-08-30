using _116.Identity.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="UserTokenStateEntity" />. The record is read-only by design — bumps are
/// atomic SQL updates — so the factory is the only behavior to prove.
/// </summary>
public class UserTokenStateEntityTests
{
    [Fact]
    public void Create_ShouldUseTheUserIdAsIdentity()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserTokenStateEntity.Create(userId: userId);

        // Assert
        state.Id.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldSeedANonEmptySecurityStamp()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserTokenStateEntity.Create(userId: userId);

        // Assert
        state.SecurityStamp.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldStartTheTokenVersionAtZero()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var state = UserTokenStateEntity.Create(userId: userId);

        // Assert
        state.TokenVersion.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldSeedADistinctStampPerRecord()
    {
        // Arrange & Act
        var first = UserTokenStateEntity.Create(userId: Guid.NewGuid());
        var second = UserTokenStateEntity.Create(userId: Guid.NewGuid());

        // Assert
        first.SecurityStamp.Should().NotBe(second.SecurityStamp);
    }
}
