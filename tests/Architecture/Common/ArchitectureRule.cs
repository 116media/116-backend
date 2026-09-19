using System.Reflection;
using _116.Content;
using _116.Core;
using _116.Identity;
using _116.Mailer;
using AwesomeAssertions;
using NetArchTest.Rules;

namespace _116.Architecture.Tests.Common;

/// <summary>
/// One module under test: its assembly and the namespace root every layer hangs off.
/// </summary>
/// <param name="Name">The module name, used in assertion messages.</param>
/// <param name="Assembly">The module's assembly.</param>
public record Module(string Name, Assembly Assembly)
{
    /// <summary>
    /// The module's namespace root, for example <c>_116.Content</c>.
    /// </summary>
    public string Root => $"_116.{Name}";
}

/// <summary>
/// Runs a NetArchTest rule and asserts on the result minus the committed allowlist, so a known
/// violation does not break the build while a new one does.
/// </summary>
public static class ArchitectureRule
{
    /// <summary>
    /// Every module the rules apply to.
    /// </summary>
    public static readonly IReadOnlyList<Module> Modules =
    [
        new("Core", typeof(CoreModule).Assembly),
        new("Identity", typeof(IdentityModule).Assembly),
        new("Content", typeof(ContentModule).Assembly),
        new("Mailer", typeof(MailerModule).Assembly),
    ];

    private static readonly HashSet<string> Allowed = LoadAllowlist();

    /// <summary>
    /// Asserts the rule holds, ignoring types named in <c>KnownViolations.txt</c>.
    /// </summary>
    /// <param name="result">The rule result to check.</param>
    /// <param name="because">What the rule protects, quoted back on failure.</param>
    public static void ShouldHold(this TestResult result, string because)
    {
        IReadOnlyList<string> offenders =
        [
            .. (result.FailingTypeNames ?? []).Where(name => !Allowed.Contains(name)).Order(),
        ];

        offenders.Should().BeEmpty(because);
    }

    /// <summary>
    /// Reads the allowlist that ships beside the test assembly.
    /// </summary>
    /// <returns>The allowlisted type names.</returns>
    private static HashSet<string> LoadAllowlist()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "KnownViolations.txt");

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
