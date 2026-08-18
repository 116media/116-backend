namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "OtpPepperless" xUnit test collection, which owns the only host booted without an
/// OTP pepper configured.
/// </summary>
/// <remarks>
/// Parallelization is disabled because <see cref="OtpPepperlessApiFixture" /> clears a
/// process-global environment variable. Running concurrently with a collection that boots its own
/// host would leave that host unable to hash an OTP.
/// </remarks>
[CollectionDefinition("OtpPepperless", DisableParallelization = true)]
public class OtpPepperlessCollection : ICollectionFixture<OtpPepperlessPostgresFixture>;
