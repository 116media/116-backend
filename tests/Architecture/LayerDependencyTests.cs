using _116.Architecture.Tests.Common;
using NetArchTest.Rules;
using Xunit;

namespace _116.Architecture.Tests;

/// <summary>
/// Layer rules inside a module. A module is one assembly, so the project graph cannot enforce
/// these the way it enforces the module boundary — only these rules can.
/// </summary>
public class LayerDependencyTests
{
    /// <summary>
    /// Infrastructure concerns the domain must never name.
    /// </summary>
    private static readonly string[] PersistenceAndWeb =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
    ];

    /// <summary>
    /// Third-party provider SDKs. A use case may depend on an abstraction of a provider, never on
    /// the provider's own types, so swapping one never reaches past Infrastructure.
    /// </summary>
    private static readonly string[] ProviderSdks = ["CloudinaryDotNet", "MailKit", "MimeKit", "StackExchange.Redis"];

    [Fact]
    public void DomainDependsOnNoOtherLayer()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            Types
                .InAssembly(module.Assembly)
                .That()
                .ResideInNamespace($"{module.Root}.Domain")
                .ShouldNot()
                .HaveDependencyOnAny($"{module.Root}.Application", $"{module.Root}.Infrastructure")
                .GetResult()
                .ShouldHold($"{module.Name}.Domain must not depend on the layers above it");
        }
    }

    [Fact]
    public void DomainDependsOnNoPersistenceOrWebFramework()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            Types
                .InAssembly(module.Assembly)
                .That()
                .ResideInNamespace($"{module.Root}.Domain")
                .ShouldNot()
                .HaveDependencyOnAny(PersistenceAndWeb)
                .GetResult()
                .ShouldHold($"{module.Name}.Domain must stay free of persistence and web frameworks");
        }
    }

    [Fact]
    public void ApplicationAndDomainNameNoProviderSdk()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            foreach (string layer in (string[])["Domain", "Application"])
            {
                Types
                    .InAssembly(module.Assembly)
                    .That()
                    .ResideInNamespace($"{module.Root}.{layer}")
                    .ShouldNot()
                    .HaveDependencyOnAny(ProviderSdks)
                    .GetResult()
                    .ShouldHold($"{module.Name}.{layer} must abstract provider SDKs, not name them");
            }
        }
    }

    [Fact]
    public void ApplicationDoesNotDependOnInfrastructure()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            Types
                .InAssembly(module.Assembly)
                .That()
                .ResideInNamespace($"{module.Root}.Application")
                .ShouldNot()
                .HaveDependencyOn($"{module.Root}.Infrastructure")
                .GetResult()
                .ShouldHold($"{module.Name}.Application must depend on abstractions, not Infrastructure");
        }
    }
}
