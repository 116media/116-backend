using System.Reflection;
using _116.Architecture.Tests.Common;
using _116.Shared.Domain;
using AwesomeAssertions;
using Xunit;

namespace _116.Architecture.Tests;

/// <summary>
/// The aggregate marker rule across every module: a repository may only be opened over an aggregate
/// root, so a class deriving from <see cref="Aggregate{TId}" /> must carry <see cref="IAggregateRoot" />.
/// </summary>
public class AggregateMarkerTests
{
    [Fact]
    public void EveryModuleAggregate_ShouldBeAnAggregateRoot()
    {
        Assembly[] domains = [.. ArchitectureRule.Modules.Select(module => Assembly.Load($"{module.Name}.Domain"))];

        Type[] aggregates =
        [
            .. domains
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type is { IsAbstract: false, IsClass: true } && typeof(IAggregate).IsAssignableFrom(type)
                ),
        ];

        Type[] unmarked = [.. aggregates.Where(type => !typeof(IAggregateRoot).IsAssignableFrom(type))];

        aggregates.Should().HaveCountGreaterThan(40);
        unmarked.Should().BeEmpty();
    }
}
