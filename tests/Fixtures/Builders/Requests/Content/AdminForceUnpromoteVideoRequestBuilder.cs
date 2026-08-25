using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminForceUnpromoteVideoRequest"/> instances in tests
/// with a valid default reason that satisfies the force-unpromote video validator.
/// </summary>
public class AdminForceUnpromoteVideoRequestBuilder
{
    private string _reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminForceUnpromoteVideoRequestBuilder"/> class
    /// with a valid default unpromote reason.
    /// </summary>
    public AdminForceUnpromoteVideoRequestBuilder()
    {
        _reason = TestConstants.Video.ValidRejectionReason;
    }

    /// <summary>
    /// Builds the <see cref="AdminForceUnpromoteVideoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminForceUnpromoteVideoRequest instance.</returns>
    public AdminForceUnpromoteVideoRequest Build()
    {
        return new AdminForceUnpromoteVideoRequest(Reason: _reason);
    }
}
