using _116.Architecture.Tests.Common;
using NetArchTest.Rules;
using Xunit;

namespace _116.Architecture.Tests;

/// <summary>
/// Module-boundary rules: a module may reach another module through its Contracts assembly only.
/// The project graph already enforces this — no module csproj references another module's
/// implementation project — so these rules are the ratchet that keeps it that way.
/// </summary>
public class ModuleBoundaryTests
{
    /// <summary>
    /// The layers a module keeps to itself. Contracts are deliberately absent: they are the seam.
    /// </summary>
    private static readonly string[] InternalLayers = ["Domain", "Application", "Infrastructure"];

    [Fact]
    public void NoModuleDependsOnAnotherModulesInternals()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            string[] forbidden =
            [
                .. ArchitectureRule
                    .Modules.Where(other => other.Name != module.Name)
                    .SelectMany(other => InternalLayers.Select(layer => $"{other.Root}.{layer}")),
            ];

            Types
                .InAssembly(module.Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult()
                .ShouldHold($"{module.Name} must reach other modules through their Contracts only");
        }
    }
}
