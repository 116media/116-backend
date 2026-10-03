using _116.Architecture.Tests.Common;
using _116.BuildingBlocks.Application.CQRS;
using AwesomeAssertions;
using Xunit;

namespace _116.Architecture.Tests;

/// <summary>
/// The handler dependency budget: a command or query handler declares at most
/// <see cref="MaxDependencies" /> constructor parameters. Handlers over budget on the day the
/// rule landed burn down through <c>HandlerBudgetBurnDown.txt</c>, which only ever shrinks.
/// </summary>
public class HandlerDependencyBudgetTests
{
    private const int MaxDependencies = 4;

    private static readonly HashSet<string> BurnDown = LoadBurnDown();

    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<,>),
        typeof(ICommandHandler<>),
        typeof(IQueryHandler<,>),
    ];

    private static IEnumerable<Type> Handlers =>
        ArchitectureRule
            .Modules.SelectMany(module => module.Assemblies)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(IsHandler);

    [Fact]
    public void EveryHandler_StaysWithinTheDependencyBudget()
    {
        List<string> newViolators =
        [
            .. Handlers
                .Where(handler => DependencyCount(handler) > MaxDependencies && !BurnDown.Contains(handler.Name))
                .Select(handler => $"{handler.Name} ({DependencyCount(handler)})")
                .Order(),
        ];

        newViolators
            .Should()
            .BeEmpty(
                "a handler over {0} dependencies delegates a phase to an application service instead",
                MaxDependencies
            );
    }

    [Fact]
    public void TheBurnDownList_OnlyShrinks()
    {
        List<string> alreadyFixed =
        [
            .. Handlers
                .Where(handler => BurnDown.Contains(handler.Name) && DependencyCount(handler) <= MaxDependencies)
                .Select(handler => handler.Name)
                .Order(),
        ];

        alreadyFixed.Should().BeEmpty("remove fixed handlers from HandlerBudgetBurnDown.txt so it only shrinks");
    }

    private static bool IsHandler(Type type)
    {
        return type is { IsClass: true, IsAbstract: false }
            && type.GetInterfaces()
                .Any(contract =>
                    contract.IsGenericType && HandlerInterfaces.Contains(contract.GetGenericTypeDefinition())
                );
    }

    private static int DependencyCount(Type handler)
    {
        return handler.GetConstructors().Max(constructor => constructor.GetParameters().Length);
    }

    private static HashSet<string> LoadBurnDown()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "HandlerBudgetBurnDown.txt");

        if (!File.Exists(path))
        {
            return [];
        }

        return
        [
            .. File.ReadAllLines(path)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 && !line.StartsWith('#')),
        ];
    }
}
