using Mapster;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// The Content module's Mapster registrations, contributed to the shared cross-module config
/// through <see cref="IRegister" />.
/// </summary>
public sealed class MappingRegistration : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
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
        PlaylistMapper.Register(config);
        ContentOrderMapper.Register(config);
    }

    /// <summary>
    /// Creates and compiles a standalone config carrying only the Content module's mappings.
    /// </summary>
    /// <returns>A fully configured TypeAdapterConfig instance.</returns>
    public static TypeAdapterConfig CreateConfiguration()
    {
        var config = new TypeAdapterConfig();
        new MappingRegistration().Register(config);
        config.Compile();

        return config;
    }
}
