using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Session.Builders;

/// <summary>
/// Interface for building dynamic session queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface ISessionQueryBuilder
{
    /// <summary>
    /// Adds user ID filter to the query.
    /// </summary>
    ISessionQueryBuilder WithUserId(Guid userId);

    /// <summary>
    /// Adds status filter to the query (active or expired).
    /// </summary>
    ISessionQueryBuilder WithStatus(string? status);

    /// <summary>
    /// Adds IP address filter to the query (partial match).
    /// </summary>
    ISessionQueryBuilder WithIpAddress(string? ipAddress);

    /// <summary>
    /// Adds device name filter to the query (partial match).
    /// </summary>
    ISessionQueryBuilder WithDeviceName(string? deviceName);

    /// <summary>
    /// Adds from date filter to the query.
    /// </summary>
    ISessionQueryBuilder WithFromDate(DateTime? fromDate);

    /// <summary>
    /// Adds to date filter to the query.
    /// </summary>
    ISessionQueryBuilder WithToDate(DateTime? toDate);

    /// <summary>
    /// Adds active status filter to the query.
    /// </summary>
    ISessionQueryBuilder WithActiveStatus(bool? isActive);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<SessionEntity>? Build();
}
