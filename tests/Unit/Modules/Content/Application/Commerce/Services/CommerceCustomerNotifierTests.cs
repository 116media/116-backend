using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Mailer.Contracts.Application;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Services;

/// <summary>
/// Unit tests for <see cref="CommerceCustomerNotifier" />: the formatting
/// helpers every customer email is built from, and the guards that keep a
/// notification from being sent to a customer that no longer resolves.
/// </summary>
public class CommerceCustomerNotifierTests
{
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly CommerceCustomerNotifier _notifier;

    public CommerceCustomerNotifierTests()
    {
        _notifier = new CommerceCustomerNotifier(_mailerMock.Object, _customerRepositoryMock.Object);
    }

    [Fact]
    public void OrderReference_ShouldBeTheFirstEightHexCharsUppercased()
    {
        var orderId = Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890");

        CommerceCustomerNotifier.OrderReference(orderId).Should().Be("ABCDEF12");
    }

    [Fact]
    public void OrderReference_ShouldBeStableForTheSameOrder()
    {
        var orderId = Guid.NewGuid();

        CommerceCustomerNotifier.OrderReference(orderId).Should().Be(CommerceCustomerNotifier.OrderReference(orderId));
    }

    [Theory]
    [InlineData(150, "150.00")]
    [InlineData(99.5, "99.50")]
    [InlineData(0.125, "0.13")]
    public void FormatAmount_ShouldRenderTwoInvariantDecimals(decimal amount, string expected)
    {
        CommerceCustomerNotifier.FormatAmount(amount).Should().Be(expected);
    }

    [Fact]
    public void PaymentMethods_ShouldListEveryEnumMember()
    {
        CommerceCustomerNotifier.PaymentMethods().Should().Be("BankTransfer, MobileMoney, Cash");
    }

    [Fact]
    public void ItemSummary_WithEveryCategoryLoaded_ShouldListTheCategoryNames()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        CategoryEntity music = CategoryFactory.Create(Guid.NewGuid(), "Musique", "musique");
        CategoryEntity interview = CategoryFactory.Create(Guid.NewGuid(), "Interview", "interview");
        order.Items.Add(ContentOrderItemFactory.CreateWithCategory(order.Id, music));
        order.Items.Add(ContentOrderItemFactory.CreateWithCategory(order.Id, interview));

        CommerceCustomerNotifier.ItemSummary(order).Should().Be("Musique, Interview");
    }

    [Fact]
    public void ItemSummary_WithACategoryNotLoaded_ShouldFallBackToTheItemCount()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        CategoryEntity music = CategoryFactory.Create(Guid.NewGuid(), "Musique", "musique");
        order.Items.Add(ContentOrderItemFactory.CreateWithCategory(order.Id, music));
        order.Items.Add(ContentOrderItemFactory.Create(order.Id, Guid.NewGuid()));

        CommerceCustomerNotifier.ItemSummary(order).Should().Be("2 item(s)");
    }

    [Fact]
    public void ItemSummary_WithoutItems_ShouldFallBackToTheItemCount()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        CommerceCustomerNotifier.ItemSummary(order).Should().Be("0 item(s)");
    }

    [Fact]
    public async Task NotifyOrderCancelledAsync_WithAResolvableCustomer_ShouldEnqueueTheCancellation()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create("label@example.com");
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(order.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        await _notifier.NotifyOrderCancelledAsync(order, CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.OrderCancelled,
                    It.Is<EmailRecipient>(recipient => recipient.Address == "label@example.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(tokens =>
                        tokens["orderReference"] == CommerceCustomerNotifier.OrderReference(order.Id)
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyOrderCancelledAsync_WhenTheCustomerNoLongerResolves_ShouldEnqueueNothing()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(order.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act
        await _notifier.NotifyOrderCancelledAsync(order, CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task NotifyPromotionRemovedAsync_WithoutACustomerId_ShouldNotLookUpOrNotifyAnyone()
    {
        // Act
        await _notifier.NotifyPromotionRemovedAsync(
            customerId: null,
            contentTitle: "Eloko Oyo",
            reason: "Policy violation",
            cancellationToken: CancellationToken.None
        );

        // Assert
        _customerRepositoryMock.VerifyNoOtherCalls();
        _mailerMock.VerifyNoOtherCalls();
    }
}
