using Microsoft.AspNetCore.Http;

namespace _116.Shared.Application.Exceptions.Problems;

/// <summary>
/// The RFC 9457 problem a violated domain rule answers with: the status and title the rule
/// reports, and the resolver producing its localized detail from the request's services.
/// </summary>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Title">The ProblemDetails title.</param>
/// <param name="Detail">Resolver producing the localized detail from the context and rule args.</param>
public sealed record RuleProblem(int Status, string Title, Func<HttpContext, IReadOnlyList<string>, string> Detail);
