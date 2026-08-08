using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end proof of the commerce domain event pipeline over real HTTP:
/// a verified payment stamps the paid-for effects onto the fulfilling
/// content and writes the receipt outbox row through the post-commit
/// <c>OrderPaidEvent</c> handlers, while a rejected business operation
/// (409 on double-verify) dispatches nothing.
/// </summary>
[Collection("Database")]
public class CommerceEventDispatchFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VerifyPayment_Committed_StampsContentAndWritesReceiptOutboxRowThroughTheEventPath()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            category.Id,
            promoLevel.Id,
            promoLevel.PriceUsd
        );
        ArticleEntity article = ArticleFactory.CreatePendingPayment(category.Id, customer.Id, orderItem.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PromotionLevels.Add(promoLevel);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Articles.Add(article);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        var response = await Client.SendAsync(msg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        DateTimeOffset after = DateTimeOffset.UtcNow;

        await using ContentDbContext contentContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persistedOrder = await contentContext.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.Paid);

        ArticleEntity? persistedArticle = await contentContext.Articles.FindAsync(article.Id);
        persistedArticle!.IsPromoted.Should().BeTrue();
        persistedArticle.PromotionLevelId.Should().Be(promoLevel.Id);
        persistedArticle.PromotedUntil.Should().NotBeNull();
        persistedArticle.PromotedUntil!.Value.Should().BeOnOrAfter(before.AddDays(promoLevel.DurationDays));
        persistedArticle.PromotedUntil.Value.Should().BeOnOrBefore(after.AddDays(promoLevel.DurationDays));
        persistedArticle.Status.Should().Be(EnumContentStatus.PendingReview);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var receipts = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == customer.Email && o.Template == "PaymentReceipt")
            .ToListAsync();
        receipts.Should().ContainSingle();
    }

    [Fact]
    public async Task VerifyPayment_WhenAlreadyVerified_Returns409AndDispatchesNothing()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        ArticleEntity article = ArticleFactory.CreatePendingPayment(category.Id, customer.Id, orderItem.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Articles.Add(article);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        // Act
        var response = await Client.SendAsync(msg);

        // Assert
        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.PaymentAlreadyVerified())
        );

        await using ContentDbContext contentContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persistedOrder = await contentContext.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.PendingPayment);

        ArticleEntity? persistedArticle = await contentContext.Articles.FindAsync(article.Id);
        persistedArticle!.IsPromoted.Should().BeFalse();
        persistedArticle.SocialBoost.Should().BeFalse();
        persistedArticle.Status.Should().Be(EnumContentStatus.PendingPayment);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        (
            await mailerContext.OutboxEmails.CountAsync(o =>
                o.RecipientAddress == customer.Email && o.Template == "PaymentReceipt"
            )
        )
            .Should()
            .Be(0);
    }
}
