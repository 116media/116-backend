using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
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
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
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
    public async Task VerifyPayment_WithoutProofAttached_ReturnsBadRequestAndLeavesTheOrderUnpaid()
    {
        // Arrange — an unevidenced payment must not flip the order to paid
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
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
        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ContentOrderErrorMessage>(m => m.PaymentProofRequired())
        );

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ContentPayments.FindAsync(payment.Id))!.Status.Should().Be(EnumPaymentStatus.Pending);
        (await verifyDb.ContentOrders.FindAsync(order.Id))!.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Fact]
    public async Task VerifyPayment_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task VerifyPayment_WhenTheOrderHasNoPayment_ReturnsNotFound()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentPayment"))
        );
    }

    [Fact]
    public async Task VerifyPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

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
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.PaymentAlreadyVerified())
        );
    }

    [Fact]
    public async Task VerifyPayment_WhenAlreadyRejected_ReturnsConflictAndLeavesTheOrderUnpaid()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateRejected(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.PaymentAlreadyRejected())
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        (await db.ContentPayments.FindAsync(payment.Id))!.Status.Should().Be(EnumPaymentStatus.Rejected);
        (await db.ContentOrders.FindAsync(order.Id))!.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Fact]
    public async Task VerifyPayment_WhenOrderWasCancelled_ReturnsAlreadyPaidConflictAndLeavesOrderCancelled()
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
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var cancelResponse = await Client.PatchAsync(Routes.Admin.Orders.Cancel(order.Id), null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.AlreadyPaid(), LocalizedMessage.EnglishCulture)
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.Cancelled);

        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FindAsync(payment.Id);
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Pending);
    }

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
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
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
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
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
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
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
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
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

    [Fact]
    public async Task VerifyPayment_WithSocialBoostVideoOrderItem_StampsSocialBoostOnVideo()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.CreateSocialBoost(order.Id, category.Id);
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

        video.SocialBoost.Should().BeFalse();

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        VideoEntity? persistedVideo = await db.Videos.FindAsync(video.Id);
        persistedVideo!.SocialBoost.Should().BeTrue();
        persistedVideo.Status.Should().Be(EnumContentStatus.PendingReview);
    }

    [Fact]
    public async Task VerifyPayment_WithSocialBoostArticleOrderItem_StampsSocialBoostOnArticle()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.CreateSocialBoost(order.Id, category.Id);
        ArticleEntity article = ArticleFactory.CreatePaid(category.Id, customer.Id, orderItem.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
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

        article.SocialBoost.Should().BeFalse();

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ArticleEntity? persistedArticle = await db.Articles.FindAsync(article.Id);
        persistedArticle!.SocialBoost.Should().BeTrue();
        persistedArticle.Status.Should().Be(EnumContentStatus.PendingReview);
    }
}
