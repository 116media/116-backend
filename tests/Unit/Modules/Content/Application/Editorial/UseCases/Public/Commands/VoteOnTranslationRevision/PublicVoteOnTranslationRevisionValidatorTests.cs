using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Unit tests for <see cref="PublicVoteOnTranslationRevisionValidator"/>.
/// </summary>
public class PublicVoteOnTranslationRevisionValidatorTests
{
    private readonly PublicVoteOnTranslationRevisionValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static PublicVoteOnTranslationRevisionCommand Command(string? comment)
    {
        return new PublicVoteOnTranslationRevisionCommand(
            RevisionId: Guid.NewGuid(),
            Vote: EnumVote.Approve,
            Comment: comment,
            UserId: Guid.NewGuid()
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Traduction fidele.")]
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicVoteOnTranslationRevisionCommand.Comment));
    }
}
