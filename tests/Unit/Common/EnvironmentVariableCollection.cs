using Xunit;

namespace _116.Unit.Tests.Common;

/// <summary>
/// Serializes test classes that mutate the process-global <c>ASPNETCORE_ENVIRONMENT</c> variable.
/// Disabling parallelization prevents one class from clobbering the variable while another reads
/// it (e.g. module registration deciding whether to enable migrations/seeding).
/// </summary>
[CollectionDefinition("EnvironmentVariable", DisableParallelization = true)]
public sealed class EnvironmentVariableCollection;
