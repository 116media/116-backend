namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="PlaylistVideoEntity" />. Its state lives in <c>Entities/PlaylistVideoEntity.cs</c>.
/// </summary>
public partial class PlaylistVideoEntity
{
    /// <summary>
    /// Updates the display sort order of this video within the playlist.
    /// </summary>
    /// <param name="sortOrder">The new sort order value.</param>
    public void UpdateSortOrder(int sortOrder) => SortOrder = sortOrder;
}
