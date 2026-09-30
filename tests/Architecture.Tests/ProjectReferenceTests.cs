using System.Xml.Linq;
using AwesomeAssertions;
using Xunit;

namespace _116.Architecture.Tests;

/// <summary>
/// Reference-direction rules read straight off the project graph. The type-level rules cannot see an
/// edge until something uses it, so these read every csproj and fail on the edge itself.
/// </summary>
public class ProjectReferenceTests
{
    private static readonly string[] Modules = ["Content", "Identity", "Storage", "Mailer"];

    private static readonly string[] SharedKernel =
    [
        "Shared.Domain",
        "BuildingBlocks.Domain",
        "BuildingBlocks.Application",
        "BuildingBlocks.Infrastructure",
        "BuildingBlocks.Presentation",
    ];

    [Fact]
    public void DomainReferencesOnlyTheSharedKernelAndItsOwnContracts()
    {
        AssertEdges(
            project => Modules.Any(module => project == $"{module}.Domain"),
            (project, reference) =>
                SharedKernel.Contains(reference) || reference == project.Replace(".Domain", ".Contracts"),
            "a Domain project may reference the shared kernel and its own Contracts only"
        );
    }

    [Fact]
    public void ApplicationReachesOtherModulesThroughContractsOnly()
    {
        AssertEdges(
            project => Modules.Any(module => project == $"{module}.Application"),
            (project, reference) =>
                SharedKernel.Contains(reference)
                || reference.EndsWith(".Contracts", StringComparison.Ordinal)
                || reference == project.Replace(".Application", ".Domain"),
            "an Application project may reference the shared kernel, its own Domain, and Contracts only"
        );
    }

    [Fact]
    public void InfrastructureReferencesItsOwnApplicationOnly()
    {
        AssertEdges(
            project => Modules.Any(module => project == $"{module}.Infrastructure"),
            (project, reference) =>
                SharedKernel.Contains(reference) || reference == project.Replace(".Infrastructure", ".Application"),
            "an Infrastructure project may reference its own Application only"
        );
    }

    [Fact]
    public void ContractsReferenceNoModule()
    {
        AssertEdges(
            project => project.EndsWith(".Contracts", StringComparison.Ordinal),
            (_, reference) => SharedKernel.Contains(reference),
            "a Contracts project is the seam and may reference the shared kernel only"
        );
    }

    [Fact]
    public void ModuleTestDataStaysInsideItsOwnModule()
    {
        AssertEdges(
            project => Modules.Any(module => project == $"{module}.TestData"),
            (project, reference) =>
                reference == "TestData"
                || SharedKernel.Contains(reference)
                || reference.StartsWith(project.Replace(".TestData", "."), StringComparison.Ordinal),
            "a module's TestData may reference its own module and the shared test data only"
        );
    }

    [Fact]
    public void ModuleTestSuitesReachOtherModulesThroughTestDataOnly()
    {
        AssertEdges(
            project =>
                Modules.Any(module => project == $"{module}.Unit.Tests" || project == $"{module}.Integration.Tests"),
            (project, reference) =>
            {
                string own = project[..project.IndexOf('.', StringComparison.Ordinal)];

                return !Modules.Any(module =>
                        module != own && reference.StartsWith($"{module}.", StringComparison.Ordinal)
                    )
                    || reference.EndsWith(".TestData", StringComparison.Ordinal)
                    || reference.EndsWith(".Contracts", StringComparison.Ordinal);
            },
            "a module's test suite may borrow another module's TestData or Contracts, never its layers"
        );
    }

    [Fact]
    public void SharedTestDataNamesNoModuleInfrastructure()
    {
        AssertEdges(
            project => project is "TestData" or "Fixtures",
            (project, reference) =>
                project == "Fixtures"
                || !Modules.Any(module => reference.Equals($"{module}.Infrastructure", StringComparison.Ordinal)),
            "the shared test data is used by every module, so it must not bind any module's Infrastructure"
        );
    }

    private static void AssertEdges(Func<string, bool> selects, Func<string, string, bool> allows, string because)
    {
        List<string> offenders = [];

        foreach ((string project, string[] references) in ReadGraph())
        {
            if (!selects(project))
            {
                continue;
            }

            offenders.AddRange(
                references.Where(reference => !allows(project, reference)).Select(r => $"{project} -> {r}")
            );
        }

        offenders.Should().BeEmpty(because);
    }

    private static IReadOnlyList<(string Project, string[] References)> ReadGraph()
    {
        DirectoryInfo root = new(AppContext.BaseDirectory);

        while (root is not null && !File.Exists(Path.Combine(root.FullName, "116_backend.sln")))
        {
            root = root.Parent!;
        }

        root.Should().NotBeNull("the rules read the csproj graph, which lives beside the solution file");

        return
        [
            .. Directory
                .EnumerateFiles(Path.Combine(root!.FullName, "src"), "*.csproj", SearchOption.AllDirectories)
                .Concat(
                    Directory.EnumerateFiles(
                        Path.Combine(root.FullName, "tests"),
                        "*.csproj",
                        SearchOption.AllDirectories
                    )
                )
                .Select(path =>
                    (
                        Path.GetFileNameWithoutExtension(path),
                        XDocument
                            .Load(path)
                            .Descendants("ProjectReference")
                            .Select(element => Path.GetFileNameWithoutExtension(element.Attribute("Include")!.Value))
                            .ToArray()
                    )
                ),
        ];
    }
}
