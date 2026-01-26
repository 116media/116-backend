using Bogus;

namespace _116.Unit.Tests.Common.Fixtures;

/// <summary>
/// Base fixture for unit tests providing common setup and utilities.
/// </summary>
public class UnitTestFixture : IDisposable
{
    /// <summary>
    /// Faker instance for generating random test data.
    /// </summary>
    protected Faker Faker { get; }

    /// <summary>
    /// Cancellation token for async operations in tests.
    /// </summary>
    protected CancellationToken CancellationToken { get; }

    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitTestFixture"/> class.
    /// </summary>
    public UnitTestFixture()
    {
        Faker = new Faker();
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
    }

    /// <summary>
    /// Generates a random GUID.
    /// </summary>
    /// <returns>A new random GUID.</returns>
    protected static Guid NewGuid() => Guid.NewGuid();

    /// <summary>
    /// Generates a random string of specified length.
    /// </summary>
    /// <param name="length">The desired length of the string.</param>
    /// <returns>A random alphanumeric string.</returns>
    protected string RandomString(int length) => Faker.Random.AlphaNumeric(length);

    /// <summary>
    /// Generates a random email address.
    /// </summary>
    /// <returns>A random email address.</returns>
    protected string RandomEmail() => Faker.Internet.Email();

    /// <summary>
    /// Generates a random integer within the specified range.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>A random integer within the range.</returns>
    protected int RandomInt(int min = 0, int max = 1000) => Faker.Random.Int(min, max);

    /// <summary>
    /// Disposes the fixture resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the fixture resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
