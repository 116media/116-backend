using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IContentTypeRepository"/> verifying persistence behavior against a real
/// PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ContentTypeRepositoryTests : BaseRepositoryTest
{
    public ContentTypeRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("TestArticle");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var result = await repo.GetByIdOrThrowAsync(contentType.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(contentType.Id);
        result.Name.Should().Be("TestArticle");
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<IContentTypeRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenExists_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("UniqueContentType");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var exists = await repo.ExistsByNameAsync("UniqueContentType");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenNotFound_ReturnsFalse()
    {
        var repo = Resolve<IContentTypeRepository>();

        var exists = await repo.ExistsByNameAsync("NonExistentContentType");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_WithDifferentCase_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.ContentTypes.Add(ContentTypeFactory.Create("CasedContentType"));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var exists = await repo.ExistsByNameAsync("cAsEdCoNtEnTtYpE");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WithoutSearch_ReturnsAllOrderedByName()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.ContentTypes.AddRange(
            ContentTypeFactory.Create("Zeta"),
            ContentTypeFactory.Create("Alpha"),
            ContentTypeFactory.Create("Mango")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var result = await repo.GetAllAsync();

        result.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Should().BeInAscendingOrder(x => x.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.ContentTypes.AddRange(
            ContentTypeFactory.Create("SearchableType"),
            ContentTypeFactory.Create("OtherType")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var result = await repo.GetAllAsync(search: "Searchable");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("SearchableType");
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveEntities()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var active = ContentTypeFactory.Create("ActiveType");
        var inactive = ContentTypeFactory.CreateInactive();
        seedContext.ContentTypes.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var result = await repo.GetActiveAsync();

        result.Should().OnlyContain(x => x.IsActive);
        result.Should().Contain(x => x.Id == active.Id);
        result.Should().NotContain(x => x.Id == inactive.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenTheTableOutgrowsTheCap_ReturnsAtMostTheCap()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        for (int i = 0; i < ContentConstants.MaxReferenceListSize + 20; i++)
        {
            seedContext.ContentTypes.Add(ContentTypeFactory.Create($"Bulk{i:D4}"));
        }

        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentTypeRepository>();

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(ContentConstants.MaxReferenceListSize);
    }
}
