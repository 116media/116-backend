using _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter.MetaFields;

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
