namespace _116.Core.Application.Shared.Cache;

/// <summary>
/// Tag names shared by the file projections that cache a result and the event handlers that
/// evict them.
/// </summary>
public static class CoreCacheTags
{
    /// <summary>
    /// Any projection of a stored file: its reference or its URL.
    /// </summary>
    public const string Files = "core:files";
}
