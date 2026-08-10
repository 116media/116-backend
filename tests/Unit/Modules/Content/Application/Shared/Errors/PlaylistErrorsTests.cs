using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="PlaylistErrors"/> factory methods.
/// </summary>
public class PlaylistErrorsTests
{
    private readonly PlaylistErrors _errors = TestErrorsFactory.CreatePlaylistErrors();
    private readonly PlaylistErrorMessage _message = LocalizerFactory.CreateMessage<PlaylistErrorMessage>();

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        var id = Guid.NewGuid();

        NotFoundException ex = _errors.NotFound(id);

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.NotFound(id));
    }

    [Fact]
    public void NotOwner_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.NotOwner();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.NotOwner());
    }

    [Fact]
    public void VideoAlreadyInPlaylist_ShouldReturnConflictException()
    {
        ConflictException ex = _errors.VideoAlreadyInPlaylist();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.VideoAlreadyInPlaylist());
    }

    [Fact]
    public void Msg_Localizer_NotOwner_ShouldReturnLocalizedString()
    {
        _errors.Msg.Localizer["NotOwner"].Value.Should().Be(_message.NotOwner());
    }
}
