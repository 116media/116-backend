using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using _116.Content;
using _116.Core;
using _116.Identity;
using _116.Mailer;
using _116.Shared.Application.Exceptions.Messages;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Localization;

/// <summary>
/// Asserts that every key defined in a neutral message catalogue is present and populated in the
/// English and French satellites, with matching format placeholders and a French value that is
/// actually translated.
/// </summary>
public class ResourceCompletenessTests
{
    private const string ResourcesSuffix = ".resources";

    private const string CatalogueSuffix = "Message";

    private static readonly string[] Cultures = ["en", "fr"];

    private static readonly Regex PlaceholderPattern = new(@"\{(\d+)(?::[^}]*)?\}", RegexOptions.Compiled);

    /// <summary>
    /// Keys whose French value is legitimately identical to the neutral English value — proper
    /// nouns, format-only strings, and untranslatable tokens. Every entry needs a reason, and an
    /// entry added to silence a failure is a missing translation wearing a disguise.
    /// </summary>
    private static readonly HashSet<string> IdenticalByDesign = [];

    /// <summary>
    /// One anchor type per assembly that ships message resources. Adding a module with its own
    /// catalogue means adding its marker type here.
    /// </summary>
    private static readonly Assembly[] ResourceAssemblies =
    [
        typeof(SharedExceptionMessage).Assembly,
        typeof(CoreModule).Assembly,
        typeof(IdentityModule).Assembly,
        typeof(ContentModule).Assembly,
        typeof(MailerModule).Assembly,
    ];

    /// <summary>
    /// Every neutral message catalogue found in the resource assemblies, as (assembly name,
    /// resource base name) pairs. Two families share the base name <c>ValidationErrorMessage</c>
    /// in different assemblies, so the key is assembly-qualified rather than the short name.
    /// </summary>
    /// <returns>
    /// The discovered catalogues, one theory case each.
    /// </returns>
    public static TheoryData<string, string> Catalogues()
    {
        TheoryData<string, string> data = new();

        foreach (Assembly assembly in ResourceAssemblies)
        {
            IEnumerable<string> baseNames = assembly
                .GetManifestResourceNames()
                .Where(name => name.EndsWith(ResourcesSuffix, StringComparison.Ordinal))
                .Select(name => name[..^ResourcesSuffix.Length])
                .Where(name => name.EndsWith(CatalogueSuffix, StringComparison.Ordinal))
                .Order(StringComparer.Ordinal);

            foreach (string baseName in baseNames)
            {
                data.Add(assembly.GetName().Name!, baseName);
            }
        }

        return data;
    }

    [Fact]
    public void Catalogues_ShouldDiscoverEveryShippedResourceFamily()
    {
        Catalogues().Should().HaveCount(33);
    }

    [Theory]
    [MemberData(nameof(Catalogues))]
    public void EveryNeutralKey_ShouldBePresentAndPopulatedInEveryCulture(string assemblyName, string baseName)
    {
        ResourceManager manager = CreateManager(assemblyName, baseName);

        IReadOnlyDictionary<string, string> neutral = ReadSet(manager, CultureInfo.InvariantCulture);
        neutral.Keys.Should().NotBeEmpty($"{baseName} defines no keys");

        foreach (string culture in Cultures)
        {
            IReadOnlyDictionary<string, string> translated = ReadSet(manager, new CultureInfo(culture));

            translated
                .Keys.Should()
                .BeEquivalentTo(neutral.Keys, $"{baseName}.{culture}.resx must define exactly the neutral key set");

            foreach ((string key, string neutralValue) in neutral)
            {
                translated.TryGetValue(key, out string? value);

                value.Should().NotBeNullOrWhiteSpace($"{baseName}.{culture}.resx['{key}'] is empty");

                Placeholders(value!)
                    .Should()
                    .BeEquivalentTo(
                        Placeholders(neutralValue),
                        $"{baseName}.{culture}.resx['{key}'] must format the same arguments as the neutral string"
                    );
            }
        }
    }

    [Theory]
    [MemberData(nameof(Catalogues))]
    public void FrenchCatalogue_ShouldNotRepeatTheNeutralEnglishString(string assemblyName, string baseName)
    {
        ResourceManager manager = CreateManager(assemblyName, baseName);

        IReadOnlyDictionary<string, string> neutral = ReadSet(manager, CultureInfo.InvariantCulture);
        IReadOnlyDictionary<string, string> french = ReadSet(manager, new CultureInfo("fr"));

        IEnumerable<string> untranslated = neutral
            .Where(entry =>
                french.TryGetValue(entry.Key, out string? value)
                && value == entry.Value
                && !IdenticalByDesign.Contains($"{baseName}.{entry.Key}")
            )
            .Select(entry => entry.Key);

        untranslated.Should().BeEmpty($"{baseName}.fr.resx repeats the neutral English string for these keys");
    }

    /// <summary>
    /// Builds a reader for one catalogue, resolving the owning assembly by its simple name.
    /// </summary>
    private static ResourceManager CreateManager(string assemblyName, string baseName)
    {
        Assembly assembly = ResourceAssemblies.Single(item => item.GetName().Name == assemblyName);

        return new ResourceManager(baseName, assembly);
    }

    /// <summary>
    /// Reads one culture's resource set without falling back to a parent culture, so a missing
    /// satellite is observable as an empty set rather than silently resolving to the neutral strings.
    /// </summary>
    private static IReadOnlyDictionary<string, string> ReadSet(ResourceManager manager, CultureInfo culture)
    {
        ResourceSet? set = manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);

        if (set is null)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return set.Cast<DictionaryEntry>()
            .Where(entry => entry.Value is string)
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!, StringComparer.Ordinal);
    }

    /// <summary>
    /// Extracts the composite-format argument indexes used by a message, so that a translation
    /// dropping or inventing a placeholder fails before it reaches a caller of <c>string.Format</c>.
    /// </summary>
    private static IReadOnlyCollection<string> Placeholders(string value)
    {
        return PlaceholderPattern
            .Matches(value)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
