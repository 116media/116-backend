using _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="PublicEditArticleCommentRequest"/> instances in tests.
/// </summary>
public class PublicEditArticleCommentRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _body;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicEditArticleCommentRequestBuilder"/> class
    /// with a valid random comment body that satisfies the validator.
    /// </summary>
    public PublicEditArticleCommentRequestBuilder()
    {
        string sentence = _faker.Lorem.Sentence(wordCount: 8);
        _body = sentence[..Math.Min(TestConstants.Interactions.MaxCommentBodyLength, sentence.Length)];
    }

    /// <summary>
    /// Sets the comment body text.
    /// </summary>
    /// <param name="body">The new comment body text.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicEditArticleCommentRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicEditArticleCommentRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicEditArticleCommentRequest instance.</returns>
    public PublicEditArticleCommentRequest Build()
    {
        return new PublicEditArticleCommentRequest(Body: _body);
    }
}
