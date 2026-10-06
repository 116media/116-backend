using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.Contracts.Application.Services;
using _116.Storage.Contracts.Application.Services;
using Mapster;
using MapsterMapper;
using Moq;

namespace _116.Content.TestData;

/// <summary>
/// Base class for Content module handler tests that provides a configured IMapper instance.
/// </summary>
public abstract class BaseContentHandlerTest
{
    /// <summary>
    /// Configured Mapster mapper instance using Content module mapping configuration.
    /// </summary>
    protected readonly IMapper Mapper;

    protected BaseContentHandlerTest()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        Mapper = new Mapper(config);
    }

    /// <summary>
    /// Builds the real order projection factory over mocked lookup repositories, resolving the
    /// supplied customers by id and every other lookup as empty.
    /// </summary>
    /// <param name="customers">The customers the projections should name.</param>
    /// <returns>The factory.</returns>
    protected IContentOrderDtoService CreateOrderDtoService(params CustomerEntity[] customers)
    {
        return new ContentOrderDtoService(
            Mapper,
            MockCustomerRepository.Create().SetupGetByIds(customers).Object,
            MockCategoryRepository.Create().Object,
            MockPromotionLevelRepository.Create().Object,
            MockPricingTierRepository.Create().Object
        );
    }

    /// <summary>
    /// Builds the real category projection factory over a mocked storage contract and mocked
    /// lookup repositories, resolving the supplied content types by id.
    /// </summary>
    /// <param name="fileStorage">The storage contract resolving posters.</param>
    /// <param name="contentTypes">The content types the projections should name.</param>
    /// <returns>The factory.</returns>
    protected ICategoryDtoService CreateCategoryDtoService(
        IFileStorageService fileStorage,
        params ContentTypeEntity[] contentTypes
    )
    {
        return new CategoryDtoService(
            Mapper,
            fileStorage,
            MockContentTypeRepository.Create().SetupGetByIds(contentTypes).Object,
            MockPricingTierRepository.Create().Object
        );
    }

    /// <summary>
    /// Builds the real content lookup resolver over mocked lookup repositories, resolving the
    /// supplied rows by id and every other lookup as empty.
    /// </summary>
    /// <param name="categories">The categories the projections should name.</param>
    /// <param name="customers">The customers the projections should name.</param>
    /// <param name="promotionLevels">The promotion levels the projections should name.</param>
    /// <param name="tags">The tags the projections should carry.</param>
    /// <returns>The resolver.</returns>
    protected IContentLookupService CreateContentLookupService(
        CategoryEntity[]? categories = null,
        CustomerEntity[]? customers = null,
        PromotionLevelEntity[]? promotionLevels = null,
        TagEntity[]? tags = null
    )
    {
        return new ContentLookupService(
            MockCategoryRepository.Create().SetupGetByIds(categories ?? []).Object,
            MockCustomerRepository.Create().SetupGetByIds(customers ?? []).Object,
            MockPromotionLevelRepository.Create().SetupGetByIds(promotionLevels ?? []).Object,
            MockTagRepository.Create().SetupGetByIds(tags ?? []).Object
        );
    }

    /// <summary>
    /// Builds the real package projection factory over a mocked category repository, resolving
    /// the supplied slot categories by id.
    /// </summary>
    /// <param name="categories">The slot categories the projections should name and price.</param>
    /// <returns>The factory.</returns>
    protected IPackageDtoService CreatePackageDtoService(params CategoryEntity[] categories)
    {
        return new PackageDtoService(Mapper, MockCategoryRepository.Create().SetupGetByIds(categories).Object);
    }

    /// <summary>
    /// Builds the lyrics DTO service over the given file storage and an author lookup that resolves nobody unless supplied.
    /// </summary>
    /// <param name="fileStorage">The storage contract the DTOs resolve URLs through.</param>
    /// <param name="userLookup">The author lookup, or null for one that resolves nobody.</param>
    protected ILyricsDtoService CreateLyricsDtoService(
        IFileStorageService fileStorage,
        IUserLookupService? userLookup = null
    )
    {
        return new LyricsDtoService(
            Mapper,
            userLookup ?? new Mock<IUserLookupService>().Object,
            fileStorage,
            CreateContentLookupService()
        );
    }

    /// <summary>
    /// Builds the article DTO service over the given file storage and an author lookup that resolves nobody unless supplied.
    /// </summary>
    /// <param name="fileStorage">The storage contract the DTOs resolve URLs through.</param>
    /// <param name="userLookup">The author lookup, or null for one that resolves nobody.</param>
    protected IArticleDtoService CreateArticleDtoService(
        IFileStorageService fileStorage,
        IUserLookupService? userLookup = null
    )
    {
        return new ArticleDtoService(
            Mapper,
            userLookup ?? new Mock<IUserLookupService>().Object,
            fileStorage,
            CreateContentLookupService()
        );
    }

    /// <summary>
    /// Builds the short video DTO service over the given file storage, author lookup and parent-video repository.
    /// </summary>
    /// <param name="fileStorage">The storage contract the DTOs resolve URLs through.</param>
    /// <param name="userLookup">The author lookup, or null for one that resolves nobody.</param>
    /// <param name="videoRepository">The parent-video repository, or null for one that resolves nothing.</param>
    protected IShortVideoDtoService CreateShortVideoDtoService(
        IFileStorageService fileStorage,
        IUserLookupService? userLookup = null,
        IVideoRepository? videoRepository = null
    )
    {
        return new ShortVideoDtoService(
            Mapper,
            userLookup ?? new Mock<IUserLookupService>().Object,
            fileStorage,
            videoRepository ?? MockVideoRepository.Create().Object
        );
    }
}
