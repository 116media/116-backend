using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Unit tests for <see cref="PublicRecordShortVideoViewValidator"/>.
/// </summary>
public class PublicRecordShortVideoViewValidatorTests
{
    private readonly PublicRecordShortVideoViewValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    [Fact]
    public async Task Validate_WhenNoSignalsAreSupplied_ShouldPass()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new PublicRecordShortVideoViewCommand(ShortVideoId: Guid.NewGuid())
        );

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenSignalsAreWithinTheLimits_ShouldPass()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new PublicRecordShortVideoViewCommand(
                ShortVideoId: Guid.NewGuid(),
                DeviceId: "device-abc",
                IpAddress: "203.0.113.4",
                UserAgent: "Mozilla/5.0"
            )
        );

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenDeviceIdWouldOverflowTheDedupKeyColumn_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new PublicRecordShortVideoViewCommand(
                ShortVideoId: Guid.NewGuid(),
                DeviceId: new string('a', ContentConstants.MaxViewDeviceIdLength + 1)
            )
        );

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRecordShortVideoViewCommand.DeviceId));
    }

    [Fact]
    public async Task Validate_WhenIpAddressExceedsTheColumnLength_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new PublicRecordShortVideoViewCommand(
                ShortVideoId: Guid.NewGuid(),
                IpAddress: new string('1', ContentConstants.MaxViewIpAddressLength + 1)
            )
        );

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRecordShortVideoViewCommand.IpAddress));
    }

    [Fact]
    public async Task Validate_WhenUserAgentExceedsTheColumnLength_ShouldFail()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new PublicRecordShortVideoViewCommand(
                ShortVideoId: Guid.NewGuid(),
                UserAgent: new string('a', ContentConstants.MaxViewUserAgentLength + 1)
            )
        );

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRecordShortVideoViewCommand.UserAgent));
    }
}
