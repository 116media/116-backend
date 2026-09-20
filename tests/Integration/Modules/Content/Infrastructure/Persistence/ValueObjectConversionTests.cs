using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.ValueObjects;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Integration tests for the <see cref="Slug"/> and <see cref="Money"/> EF value converters,
/// driving them through DI repositories against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ValueObjectConversionTests : BaseRepositoryTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueObjectConversionTests"/> class
    /// with the shared database fixture.
    /// </summary>
    public ValueObjectConversionTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task Slug_WrittenThroughTheRepository_RoundTripsAsTheSameValue()
    {
        // Arrange
        var (repo, db) = CreateScopedRepository<ITagRepository, ContentDbContext>();
        TagEntity tag = TagFactory.Create("Afro Gospel", "afro-gospel");

        // Act
        await repo.AddAsync(tag);
        await db.SaveChangesAsync();

        // Assert
        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        TagEntity? saved = await verifyContext.Tags.FindAsync(tag.Id);

        saved.Should().NotBeNull();
        saved!.Slug.Should().Be(new Slug("afro-gospel"));
    }

    [Fact]
    public async Task Slug_StoredAsPlainText_IsMatchedByTheDatabaseByStringEquality()
    {
        // Arrange
        await using ContentDbContext seedContext = CreateDbContext<ContentDbContext>();
        TagEntity tag = TagFactory.Create("Rumba Odemba", "rumba-odemba");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        // Act
        TagEntity? found = await Resolve<ITagRepository>().GetBySlugAsync("rumba-odemba");

        // Assert — the converter stores the bare string, so the column stays queryable as text
        found.Should().NotBeNull();
        found!.Id.Should().Be(tag.Id);
    }

    [Fact]
    public async Task Money_WrittenThroughTheRepository_RoundTripsAsTheSameAmount()
    {
        // Arrange
        var (repo, db) = CreateScopedRepository<IPromotionLevelRepository, ContentDbContext>();
        PromotionLevelEntity level = PromotionLevelFactory.Create();
        level.Update(name: level.Name, durationDays: 7, priceUsd: 249.95m, spotPriority: level.SpotPriority);

        // Act
        await repo.AddAsync(level);
        await db.SaveChangesAsync();

        // Assert — numeric(10,2) keeps the cents, and the converter hands back a Money
        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        PromotionLevelEntity? saved = await verifyContext.PromotionLevels.FindAsync(level.Id);

        saved.Should().NotBeNull();
        saved!.PriceUsd.Should().Be(new Money(249.95m));
    }

    [Fact]
    public async Task Money_OnANullableColumn_RoundTripsAsNullRatherThanZero()
    {
        // Arrange
        await using ContentDbContext seedContext = CreateDbContext<ContentDbContext>();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderEntity.Create(Guid.NewGuid(), customer.Id, null);
        order.AddItem(
            contentKind: EnumCoreContentType.Article,
            categoryId: category.Id,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: false,
            isBonus: false
        );
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Customers.Add(customer);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        // Act
        ContentOrderEntity loaded = await Resolve<IContentOrderRepository>().GetByIdOrThrowAsync(order.Id);

        // Assert
        loaded.Items.Should().ContainSingle().Which.PromoPriceSnapshotUsd.Should().BeNull();
        loaded.TotalAmountUsd.Should().Be(Money.Zero);
    }
}
