using _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Unit tests for <see cref="AdminRejectPaymentValidator"/>.
/// </summary>
public class AdminRejectPaymentValidatorTests
{
    private readonly AdminRejectPaymentValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static AdminRejectPaymentCommand Command(string orderId, string? notes = null)
    {
        return new AdminRejectPaymentCommand(OrderId: orderId, Notes: notes);
    }

    [Fact]
    public async Task Validate_WhenOrderIdIsAGuidAndNotesAreWithinTheLimit_ShouldPass()
    {
        ValidationResult result = await _validator.ValidateAsync(
            Command(Guid.NewGuid().ToString(), "Preuve de paiement illisible.")
        );

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-guid")]
    public async Task Validate_WhenOrderIdIsNotAGuid_ShouldFail(string orderId)
    {
        ValidationResult result = await _validator.ValidateAsync(Command(orderId));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminRejectPaymentCommand.OrderId));
    }

    [Fact]
    public async Task Validate_WhenNotesExceedTheColumnLength_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(
            Command(Guid.NewGuid().ToString(), new string('a', ContentConstants.MaxPaymentNotesLength + 1))
        );

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminRejectPaymentCommand.Notes));
    }
}
