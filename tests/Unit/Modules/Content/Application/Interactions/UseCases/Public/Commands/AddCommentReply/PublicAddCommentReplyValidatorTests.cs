using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Unit tests for <see cref="PublicAddCommentReplyValidator"/>.
/// </summary>
public class PublicAddCommentReplyValidatorTests
{
    private readonly PublicAddCommentReplyValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static PublicAddCommentReplyCommand Command(string body) =>
        new(ArticleId: Guid.NewGuid(), ParentCommentId: Guid.NewGuid(), UserId: Guid.NewGuid(), Body: body);

    [Fact]
    public async Task Validate_WhenBodyIsValid_ShouldPass()
    {
        ValidationResult result = await _validator.ValidateAsync(Command(TestConstants.Interactions.ValidCommentBody));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenBodyIsEmpty_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(Command(""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicAddCommentReplyCommand.Body));
    }

    [Fact]
    public async Task Validate_WhenBodyIsWhiteSpace_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(Command("   "));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenBodyExceedsMaxLength_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(
            Command(new string('a', ContentConstants.MaxCommentBodyLength + 1))
        );

        result.IsValid.Should().BeFalse();
    }
}
