using _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="PublicAddArticleCommentRequest"/> instances in tests.
/// </summary>
public class PublicAddArticleCommentRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _body;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAddArticleCommentRequestBuilder"/> class
    /// with a valid random comment body that satisfies the validator.
    /// </summary>
    public PublicAddArticleCommentRequestBuilder()
    {
        string sentence = _faker.Lorem.Sentence(wordCount: 8);
        _body = sentence[..Math.Min(TestConstants.Interactions.MaxCommentBodyLength, sentence.Length)];
    }

    /// <summary>
    /// Sets the comment body text.
    /// </summary>
    /// <param name="body">The comment body text.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicAddArticleCommentRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicAddArticleCommentRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicAddArticleCommentRequest instance.</returns>
    public PublicAddArticleCommentRequest Build()
    {
        return new PublicAddArticleCommentRequest(Body: _body);
    }
}
