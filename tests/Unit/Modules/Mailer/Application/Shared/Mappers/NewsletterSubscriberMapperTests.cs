using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Application.Shared.Mappers;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="NewsletterSubscriberMapper" />: the admin-facing
/// projection of a subscriber row and the list overload the mapper owns so
/// call sites never repeat the projection.
/// </summary>
public class NewsletterSubscriberMapperTests
{
    private static NewsletterSubscriberEntity CreateSubscriber(string email, DateTime? createdAt = null)
    {
        NewsletterSubscriberEntity subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), email);
        subscriber.CreatedAt = createdAt;

        return subscriber;
    }

    [Fact]
    public void ToNewsletterSubscriberDto_ShouldProjectEveryAdminFacingField()
    {
        var createdAt = new DateTime(2026, 1, 5, 8, 30, 0, DateTimeKind.Utc);
        NewsletterSubscriberEntity subscriber = CreateSubscriber("Fan@Example.com", createdAt);

        NewsletterSubscriberDto dto = subscriber.ToNewsletterSubscriberDto();

        dto.Id.Should().Be(subscriber.Id);
        dto.Email.Should().Be("fan@example.com");
        dto.Status.Should().Be(EnumNewsletterStatus.PendingConfirmation);
        dto.ConfirmedAt.Should().BeNull();
        dto.UnsubscribedAt.Should().BeNull();
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToNewsletterSubscriberDto_ForAConfirmedSubscriber_ShouldCarryTheConfirmationTime()
    {
        var confirmedAt = new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc);
        NewsletterSubscriberEntity subscriber = CreateSubscriber("fan@example.com");
        subscriber.Confirm(confirmedAt);

        NewsletterSubscriberDto dto = subscriber.ToNewsletterSubscriberDto();

        dto.Status.Should().Be(EnumNewsletterStatus.Subscribed);
        dto.ConfirmedAt.Should().Be(confirmedAt);
        dto.UnsubscribedAt.Should().BeNull();
    }

    [Fact]
    public void ToNewsletterSubscriberDto_ForAnOptedOutSubscriber_ShouldCarryTheOptOutTime()
    {
        var unsubscribedAt = new DateTime(2026, 3, 15, 9, 45, 0, DateTimeKind.Utc);
        NewsletterSubscriberEntity subscriber = CreateSubscriber("fan@example.com");
        subscriber.Unsubscribe(unsubscribedAt);

        NewsletterSubscriberDto dto = subscriber.ToNewsletterSubscriberDto();

        dto.Status.Should().Be(EnumNewsletterStatus.Unsubscribed);
        dto.UnsubscribedAt.Should().Be(unsubscribedAt);
        dto.ConfirmedAt.Should().BeNull();
    }

    [Fact]
    public void ToNewsletterSubscriberDto_WithoutACreationTime_ShouldProjectANullCreatedAt()
    {
        NewsletterSubscriberEntity subscriber = CreateSubscriber("fan@example.com");

        NewsletterSubscriberDto dto = subscriber.ToNewsletterSubscriberDto();

        dto.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void ToNewsletterSubscriberDtoList_ShouldMapEveryEntityKeepingTheSourceOrder()
    {
        NewsletterSubscriberEntity first = CreateSubscriber("first@example.com");
        NewsletterSubscriberEntity second = CreateSubscriber("second@example.com");

        IReadOnlyList<NewsletterSubscriberDto> dtos = new[] { first, second }.ToNewsletterSubscriberDtoList();

        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be(first.Id);
        dtos[0].Email.Should().Be("first@example.com");
        dtos[1].Id.Should().Be(second.Id);
        dtos[1].Email.Should().Be("second@example.com");
    }

    [Fact]
    public void ToNewsletterSubscriberDtoList_WithNoEntities_ShouldReturnAnEmptyList()
    {
        IReadOnlyList<NewsletterSubscriberDto> dtos = Array
            .Empty<NewsletterSubscriberEntity>()
            .ToNewsletterSubscriberDtoList();

        dtos.Should().BeEmpty();
    }
}
