using _116.BuildingBlocks.Application.Metadata;
using _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers;
using AwesomeAssertions;
using Xunit;

namespace _116.Mailer.Unit.Tests.Application.Newsletter.MetaFields;

/// <summary>
/// Tests that all Newsletter admin MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class NewsletterAdminMetaFieldTests
{
    #region Query MetaFields

    [Fact]
    public void AdminGetNewsletterSubscribersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetNewsletterSubscribersMetaField.GetNewsletterSubscribers;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("AdminGetNewsletterSubscribers");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
