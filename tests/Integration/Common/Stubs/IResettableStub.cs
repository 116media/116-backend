namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Implemented by every external-service stub that carries state across requests.
/// The integration base classes reset all registered stubs before each test.
/// </summary>
public interface IResettableStub
{
    /// <summary>
    /// Returns the stub to the state it had when the host was first built.
    /// </summary>
    void Reset();
}
