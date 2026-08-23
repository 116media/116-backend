using Xunit;

namespace _116.Unit.Tests.Common;

/// <summary>
/// Serializes every test class that mutates a process-global environment variable.
/// Disabling parallelization prevents one class from clobbering a variable while another
/// reads it (e.g. module registration deciding whether to enable migrations/seeding).
/// </summary>
[CollectionDefinition("EnvironmentVariable", DisableParallelization = true)]
public sealed class EnvironmentVariableCollection;
