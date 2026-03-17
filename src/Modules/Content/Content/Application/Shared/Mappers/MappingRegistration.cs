using Mapster;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Centralized mapping configuration registration for the Content module.
/// Creates and configures a TypeAdapterConfig instance without mutating global state.
/// </summary>
public static class MappingRegistration
{
    /// <summary>
    /// Creates and configures a new TypeAdapterConfig with all Content module mappings.
    /// This config can be registered in DI and injected where needed.
    /// </summary>
    /// <returns>A fully configured TypeAdapterConfig instance.</returns>
    public static TypeAdapterConfig CreateConfiguration()
    {
        var config = new TypeAdapterConfig();

        // Register all mapper configurations
        ContentTypeMapper.Register(config);
        PricingTierMapper.Register(config);
        PromotionLevelMapper.Register(config);
        TagMapper.Register(config);
        CategoryMapper.Register(config);
        CustomerMapper.Register(config);
        PackageMapper.Register(config);
        ArticleMapper.Register(config);
        VideoMapper.Register(config);
        ShortVideoMapper.Register(config);
        LyricsMapper.Register(config);
        ContentOrderMapper.Register(config);

        // Compile once for performance
        config.Compile();

        return config;
    }
}
