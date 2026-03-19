using _116.Content.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain;

/// <summary>
/// Unit tests for <see cref="PlaylistVideoEntity"/> domain behaviour.
/// </summary>
public class PlaylistVideoEntityTests
{
    #region Create

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        Guid id = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        Guid videoId = Guid.NewGuid();

        PlaylistVideoEntity pv = PlaylistVideoEntity.Create(id, playlistId, videoId, sortOrder: 0);

        pv.Id.Should().Be(id);
        pv.PlaylistId.Should().Be(playlistId);
        pv.VideoId.Should().Be(videoId);
        pv.SortOrder.Should().Be(0);
    }

    #endregion

    #region UpdateSortOrder

    [Fact]
    public void UpdateSortOrder_ShouldUpdateSortOrderValue()
    {
        PlaylistVideoEntity pv = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            sortOrder: 1
        );

        pv.UpdateSortOrder(5);

        pv.SortOrder.Should().Be(5);
    }

    #endregion
}
