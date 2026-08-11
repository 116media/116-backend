using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminRejectVideoRequest"/> instances in tests
/// with a valid default rejection reason that satisfies the reject video validator.
/// </summary>
public class AdminRejectVideoRequestBuilder
{
    private string _reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRejectVideoRequestBuilder"/> class
    /// with a valid default rejection reason.
    /// </summary>
    public AdminRejectVideoRequestBuilder()
    {
        _reason = TestConstants.Video.ValidRejectionReason;
    }

    /// <summary>
    /// Builds the <see cref="AdminRejectVideoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminRejectVideoRequest instance.</returns>
    public AdminRejectVideoRequest Build()
    {
        return new AdminRejectVideoRequest(Reason: _reason);
    }
}
