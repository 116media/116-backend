using _116.Content.Application.Shared.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Helpers;

/// <summary>
/// Pins the per-entity suffix policy to the dashboard's: categories take the plain slug, the
/// content types the dashboard generated with <c>{ unique: true }</c> always carry a suffix.
/// </summary>
public class SlugUniquenessPolicyTests
{
    private static Task<bool> NothingTaken(string candidate, CancellationToken token) => Task.FromResult(false);

    [Fact]
    public async Task WithoutUnique_ShouldProduceThePlainSlug()
    {
        string slug = await SlugHelper.ToAvailableSlugAsync(name: "116 Archive", isTakenAsync: NothingTaken);

        slug.Should().Be("116-archive");
    }

    [Fact]
    public async Task WithUnique_ShouldAlwaysAppendASuffix()
    {
        string slug = await SlugHelper.ToAvailableSlugAsync(
            name: "Fally Ipupa annonce un concert",
            isTakenAsync: NothingTaken,
            unique: true
        );

        slug.Should().MatchRegex("^fally-ipupa-annonce-un-concert-[a-z0-9]{8}$");
    }

    [Fact]
    public async Task WithUnique_ShouldGiveTwoRecordsOfTheSameNameDifferentSlugs()
    {
        string first = await SlugHelper.ToAvailableSlugAsync("Concert", NothingTaken, unique: true);
        string second = await SlugHelper.ToAvailableSlugAsync("Concert", NothingTaken, unique: true);

        first.Should().NotBe(second);
        first.Should().StartWith("concert-");
        second.Should().StartWith("concert-");
    }

    [Fact]
    public async Task WithoutUnique_WhenThePlainSlugIsTaken_ShouldFallBackToASuffix()
    {
        string slug = await SlugHelper.ToAvailableSlugAsync(
            name: "116 Archive",
            isTakenAsync: (candidate, _) => Task.FromResult(candidate == "116-archive")
        );

        slug.Should().MatchRegex("^116-archive-[a-z0-9]{8}$");
    }
}
