using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using _116.Shared.Application.Specifications;

namespace _116.Unit.Tests.Common.Helpers;

/// <summary>
/// Evaluates specifications whose expressions call <c>EF.Functions.ILike</c> or
/// <c>EF.Functions.Like</c>, which throw when invoked outside a PostgreSQL query. Rewrites each
/// call into an in-memory equivalent so predicate logic can be asserted in unit tests.
/// </summary>
public static class ILikeSpecificationEvaluator
{
    /// <summary>
    /// Compiles the specification's expression with ILike and Like calls rewritten for in-memory
    /// execution and evaluates it against the candidate entity.
    /// </summary>
    public static bool IsSatisfiedInMemoryBy<T>(this Specification<T> specification, T candidate)
    {
        var rewritten = (Expression<Func<T, bool>>)new ILikeRewriter().Visit(specification.ToExpression());
        return rewritten.Compile()(candidate);
    }

    private sealed class ILikeRewriter : ExpressionVisitor
    {
        private static readonly MethodInfo MatchesMethod = typeof(ILikeRewriter).GetMethod(
            nameof(Matches),
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Arguments.Count == 3 && node.Method.Name is "ILike" or "Like")
            {
                return Expression.Call(
                    MatchesMethod,
                    Visit(node.Arguments[1]),
                    Visit(node.Arguments[2]),
                    Expression.Constant(node.Method.Name == "ILike")
                );
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Reproduces PostgreSQL LIKE matching, with <c>%</c> matching any run of characters and
        /// <c>_</c> matching exactly one; ILIKE additionally ignores case.
        /// </summary>
        private static bool Matches(string input, string pattern, bool ignoreCase)
        {
            string regexPattern = $"^{Regex.Escape(pattern).Replace("%", ".*").Replace("_", ".")}$";
            RegexOptions options = RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            return Regex.IsMatch(input, regexPattern, options);
        }
    }
}
