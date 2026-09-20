using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;

/// <summary>
/// Unit tests for <see cref="PublicVoteOnLyricsRevisionValidator"/>.
/// </summary>
public class PublicVoteOnLyricsRevisionValidatorTests
{
    private readonly PublicVoteOnLyricsRevisionValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static PublicVoteOnLyricsRevisionCommand Command(string? comment)
    {
        return new PublicVoteOnLyricsRevisionCommand(
            RevisionId: Guid.NewGuid(),
            Vote: EnumVote.Approve,
            Comment: comment,
            UserId: Guid.NewGuid()
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Cette correction est juste.")]
    public async Task Validate_WhenCommentIsAbsentOrWithinTheLimit_ShouldPass(string? comment)
    {
        ValidationResult result = await _validator.ValidateAsync(Command(comment));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenCommentIsAtTheLimit_ShouldPass()
    {
        ValidationResult result = await _validator.ValidateAsync(
            Command(new string('a', ContentConstants.MaxVoteCommentLength))
        );

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenCommentExceedsTheLimit_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(
            Command(new string('a', ContentConstants.MaxVoteCommentLength + 1))
        );

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicVoteOnLyricsRevisionCommand.Comment));
    }
}
