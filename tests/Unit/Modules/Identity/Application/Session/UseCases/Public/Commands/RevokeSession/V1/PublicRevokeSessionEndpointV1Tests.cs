using _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;

public class PublicRevokeSessionEndpointV1Tests
{
    [Fact]
    public void PublicRevokeSessionResponse_ShouldConstructCorrectly()
    {
        var response = new PublicRevokeSessionResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
