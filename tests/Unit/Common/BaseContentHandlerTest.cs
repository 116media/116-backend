using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Mapster;
using MapsterMapper;

namespace _116.Unit.Tests.Common;

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
    protected IContentOrderDtoFactory CreateOrderDtoFactory(params CustomerEntity[] customers)
    {
        return new ContentOrderDtoFactory(
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
    protected ICategoryDtoFactory CreateCategoryDtoFactory(
        IFileStorageService fileStorage,
        params ContentTypeEntity[] contentTypes
    )
    {
        return new CategoryDtoFactory(
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
    protected IContentLookupFactory CreateContentLookupFactory(
        CategoryEntity[]? categories = null,
        CustomerEntity[]? customers = null,
        PromotionLevelEntity[]? promotionLevels = null,
        TagEntity[]? tags = null
    )
    {
        return new ContentLookupFactory(
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
    protected IPackageDtoFactory CreatePackageDtoFactory(params CategoryEntity[] categories)
    {
        return new PackageDtoFactory(Mapper, MockCategoryRepository.Create().SetupGetByIds(categories).Object);
    }
}
