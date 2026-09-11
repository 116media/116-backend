using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Core.Domain.Entities;
using _116.Identity.Domain.Entities;
using _116.Mailer.Domain.Entities;
using _116.Shared.Domain;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Domain;

/// <summary>
/// Unit tests for <see cref="IAggregateRoot" />: the marker that separates aggregate roots from
/// member entities and gates what a repository may be opened over.
/// </summary>
public class AggregateRootTests
{
    private class TestRoot : Aggregate<Guid>
    {
        public static TestRoot Create(Guid id)
        {
            return new TestRoot { Id = id };
        }
    }

    private class TestMember : Entity<Guid>
    {
        public static TestMember Create(Guid id)
        {
            return new TestMember { Id = id };
        }
    }

    [Fact]
    public void Aggregate_ShouldBeMarkedAsAnAggregateRoot()
    {
        // Arrange & Act
        TestRoot root = TestRoot.Create(Guid.NewGuid());

        // Assert
        root.Should().BeAssignableTo<IAggregateRoot>();
    }

    [Fact]
    public void Entity_ShouldNotBeMarkedAsAnAggregateRoot()
    {
        // Arrange & Act
        TestMember member = TestMember.Create(Guid.NewGuid());

        // Assert
        member.Should().NotBeAssignableTo<IAggregateRoot>();
    }

    [Fact]
    public void Entity_ShouldCarryNoDomainEvents()
    {
        // Arrange & Act
        TestMember member = TestMember.Create(Guid.NewGuid());

        // Assert
        member.Should().NotBeAssignableTo<IAggregate>();
    }

    [Fact]
    public void EveryModuleAggregate_ShouldBeAnAggregateRoot()
    {
        // Arrange
        Assembly[] moduleAssemblies =
        [
            typeof(FileEntity).Assembly,
            typeof(UserEntity).Assembly,
            typeof(ArticleEntity).Assembly,
            typeof(NotificationEntity).Assembly,
        ];

        Type[] aggregates =
        [
            .. moduleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type is { IsAbstract: false, IsClass: true } && typeof(IAggregate).IsAssignableFrom(type)
                ),
        ];

        // Act
        Type[] unmarked = [.. aggregates.Where(type => !typeof(IAggregateRoot).IsAssignableFrom(type))];

        // Assert
        aggregates.Should().HaveCountGreaterThan(50);
        unmarked.Should().BeEmpty();
    }
}
