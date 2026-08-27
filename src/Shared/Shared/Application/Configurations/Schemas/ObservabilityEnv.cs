namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Logging and tracing sink variables.
/// </summary>
public static class ObservabilityEnv
{
    // The default keeps the local docker-compose Seq container working with no configuration.
    public static readonly EnvVar<string> SeqUrl = EnvVar
        .Optional("SEQ_URL", @default: "http://localhost:5341")
        .AbsoluteUrl();
}
