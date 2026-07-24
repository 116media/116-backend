using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;

/// <summary>
/// Integration tests for the AdminVerifyPayment endpoint.
/// </summary>
[Collection("Database")]
public class AdminVerifyPaymentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VerifyPayment_AsSuperAdmin_ReturnsOk()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminVerifyPaymentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FindAsync(payment.Id);
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Verified);

        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.Paid);
    }

    [Fact]
    public async Task VerifyPayment_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that verifying an already-verified payment returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_WhenAlreadyVerified_ReturnsConflict()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that verifying payment on an order with a video order item succeeds,
    /// covering the VideoByOrderItemIdSpecification lookup path.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_WithVideoOrderItem_ReturnsOk()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        VideoEntity video = VideoFactory.CreatePaid(category.Id, customer.Id, orderItem.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Videos.Add(video);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminVerifyPaymentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FindAsync(payment.Id);
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Verified);

        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.Paid);
    }

    /// <summary>
    /// Verifies that verifying payment for a paid lyrics page's promoted order item stamps
    /// <c>IsPromoted</c>/<c>PromotedUntil</c> and moves the lyrics page to <c>PendingReview</c>,
    /// covering the third (lyrics) branch <c>AdminVerifyPaymentFactory</c> added in Phase 4.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_WithLyricsOrderItemAndPromotion_StampsPromotionAndMarksPendingReview()
    {
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
        LyricsEntity lyrics = LyricsFactory.CreatePendingPayment(category.Id, customer.Id, orderItem.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PromotionLevels.Add(promoLevel);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Lyrics.Add(lyrics);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        LyricsEntity? persistedLyrics = await db.Lyrics.FindAsync(lyrics.Id);
        persistedLyrics!.IsPromoted.Should().BeTrue();
        persistedLyrics.PromotedUntil.Should().NotBeNull();
        persistedLyrics.Status.Should().Be(EnumContentStatus.PendingReview);
    }

    /// <summary>
    /// Verifies the "retroactive promotion" path called out explicitly in the Phase 4 plan: an
    /// already-<c>Published</c> free lyrics page that later gets a <c>customerId</c>/
    /// <c>orderItemId</c> via <c>Update()</c> can have its new order's payment verified, which
    /// stamps promotion WITHOUT disturbing its <c>Published</c> status — proving
    /// <c>MarkPendingReview()</c> is correctly a no-op once already past PendingReview.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_RetroactivePromotionOnPublishedFreeLyrics_DoesNotDisturbPublishedStatus()
    {
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

        LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PromotionLevels.Add(promoLevel);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Lyrics.Add(lyrics);
            ctx.ContentPayments.Add(payment);
        });

        // Retroactively link the already-published free lyrics page to the new order/customer,
        // exactly as Phase 1's Update(...) signature allows.
        await using (ContentDbContext linkCtx = CreateDbContext<ContentDbContext>())
        {
            LyricsEntity toLink = (await linkCtx.Lyrics.FindAsync(lyrics.Id))!;
            toLink.Update(
                toLink.CategoryId,
                toLink.SongTitle,
                toLink.ArtistName,
                toLink.Slug,
                toLink.LyricsText,
                toLink.Language,
                toLink.VideoId,
                customer.Id,
                orderItem.Id,
                TestErrorsFactory.CreateLyricsErrors()
            );
            await linkCtx.SaveChangesAsync();
        }

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        LyricsEntity? persistedLyrics = await db.Lyrics.FindAsync(lyrics.Id);
        persistedLyrics!.IsPromoted.Should().BeTrue();
        persistedLyrics.PromotedUntil.Should().NotBeNull();
        persistedLyrics.Status.Should().Be(EnumContentStatus.Published);
    }

    /// <summary>
    /// An unlinked lyrics record (<c>OrderItemId</c> null) must be untouched by payment
    /// verification for an unrelated order item.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_WithUnlinkedLyrics_LeavesLyricsUntouched()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        LyricsEntity unlinkedLyrics = LyricsFactory.CreatePublished(category.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Lyrics.Add(unlinkedLyrics);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        LyricsEntity? persistedLyrics = await db.Lyrics.FindAsync(unlinkedLyrics.Id);
        persistedLyrics!.IsPromoted.Should().BeFalse();
        persistedLyrics.Status.Should().Be(EnumContentStatus.Published);
    }
}
