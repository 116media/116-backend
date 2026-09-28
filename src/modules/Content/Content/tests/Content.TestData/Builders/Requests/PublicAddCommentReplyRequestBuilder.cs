using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;
using _116.Tests.TestData.Constants;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicAddCommentReplyRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicAddCommentReplyRequestBuilder
{
    private string _body;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAddCommentReplyRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicAddCommentReplyRequestBuilder()
    {
        _body = TestConstants.Interactions.ValidCommentBody;
    }

    /// <summary>
    /// Sets the body.
    /// </summary>
    /// <param name="body">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicAddCommentReplyRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicAddCommentReplyRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicAddCommentReplyRequest instance.</returns>
    public PublicAddCommentReplyRequest Build()
    {
        return new PublicAddCommentReplyRequest(Body: _body);
    }
}
