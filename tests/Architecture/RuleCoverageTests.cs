using _116.Architecture.Tests.Common;
using AwesomeAssertions;
using NetArchTest.Rules;
using Xunit;

namespace _116.Architecture.Tests;

/// <summary>
/// Guards the rules themselves. A namespace filter that matches nothing makes every rule in this
/// project pass vacuously, so a renamed layer would silently disable the ratchet rather than
/// break the build.
/// </summary>
public class RuleCoverageTests
{
    [Fact]
    public void EveryModuleExposesTheLayersTheRulesFilterOn()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            foreach (string layer in (string[])["Domain", "Application", "Infrastructure"])
            {
                IReadOnlyList<Type> found =
                [
                    .. Types.InAssembly(module.Assembly).That().ResideInNamespace($"{module.Root}.{layer}").GetTypes(),
                ];

                found.Should().NotBeEmpty($"{module.Name}.{layer} must contain types for its rules to mean anything");
            }
        }
    }
}
