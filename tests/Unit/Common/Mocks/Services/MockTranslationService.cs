using _116.Content.Application.Shared.Services;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="ITranslationService"/>.
/// </summary>
public static class MockTranslationService
{
    /// <summary>
    /// Creates a new mock instance of ITranslationService with a safe default setup that echoes
    /// back the source text unchanged.
    /// </summary>
    public static Mock<ITranslationService> Create()
    {
        Mock<ITranslationService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up TranslateAsync to return the given translated text for any input.
    /// </summary>
    public static Mock<ITranslationService> SetupTranslate(this Mock<ITranslationService> mock, string translatedText)
    {
        mock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(translatedText);
        return mock;
    }

    /// <summary>
    /// Verifies that TranslateAsync was never invoked — used to prove the idempotent
    /// request-translation path skips the AI call when a translation already exists.
    /// </summary>
    public static void VerifyTranslateNotCalled(this Mock<ITranslationService> mock)
    {
        mock.Verify(
            x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    /// <summary>
    /// Verifies that TranslateAsync was invoked exactly once.
    /// </summary>
    public static void VerifyTranslateCalledOnce(this Mock<ITranslationService> mock)
    {
        mock.Verify(
            x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    private static void SetupDefaults(Mock<ITranslationService> mock)
    {
        mock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("translated text");
    }
}
