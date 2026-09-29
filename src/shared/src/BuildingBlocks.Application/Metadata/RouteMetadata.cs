namespace _116.BuildingBlocks.Application.Metadata;

/// <summary>
/// Represents metadata information for an API route.
/// </summary>
/// <param name="name">The unique name of the route, used for routing or identification.</param>
/// <param name="summary">A brief summary describing the purpose of the route.</param>
/// <param name="description">A detailed description providing additional context about the route's behavior.</param>
public readonly struct RouteMetadata(string name, string summary, string description)
{
    /// <summary>
    /// Gets the unique name of the route.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the brief summary describing the purpose of the route.
    /// </summary>
    public string Summary { get; } = summary;

    /// <summary>
    /// Gets the detailed description providing additional context about the route's behavior.
    /// </summary>
    public string Description { get; } = description;
}
