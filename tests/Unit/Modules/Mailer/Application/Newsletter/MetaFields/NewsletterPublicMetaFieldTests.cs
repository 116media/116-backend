using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;
using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;
using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter.MetaFields;

/// <summary>
/// Tests that all Newsletter public MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class NewsletterPublicMetaFieldTests
{
    #region Command MetaFields

    [Fact]
    public void PublicConfirmNewsletterMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicConfirmNewsletterMetaField.ConfirmNewsletter;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicConfirmNewsletter");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicSubscribeNewsletterMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicSubscribeNewsletterMetaField.SubscribeNewsletter;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicSubscribeNewsletter");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicUnsubscribeNewsletterMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicUnsubscribeNewsletterMetaField.UnsubscribeNewsletter;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicUnsubscribeNewsletter");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
