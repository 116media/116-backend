using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;

/// <summary>
/// Unit tests for <see cref="AdminSetArticleArtistsValidator"/>.
/// </summary>
public class AdminSetArticleArtistsValidatorTests
{
    private readonly AdminSetArticleArtistsValidator _validator = new();

    [Fact]
    public void Validate_WithDistinctIds_ShouldPass()
    {
        var command = new AdminSetArticleArtistsCommand(Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]);

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyList_ShouldPass()
    {
        var command = new AdminSetArticleArtistsCommand(Guid.NewGuid(), []);

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullList_ShouldFail()
    {
        var command = new AdminSetArticleArtistsCommand(Guid.NewGuid(), null!);

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ArtistIds);
    }

    [Fact]
    public void Validate_WithDuplicateIds_ShouldFail()
    {
        var duplicated = Guid.NewGuid();
        var command = new AdminSetArticleArtistsCommand(Guid.NewGuid(), [duplicated, duplicated]);

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ArtistIds);
    }

    [Fact]
    public void Validate_WithMoreThanTwentyIds_ShouldFail()
    {
        List<Guid> ids = Enumerable.Range(0, 21).Select(_ => Guid.NewGuid()).ToList();
        var command = new AdminSetArticleArtistsCommand(Guid.NewGuid(), ids);

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ArtistIds);
    }
}
