# Logging and Caching

## Checklist

- [ ] Query is **not** registered as cacheable
- [ ] `Cache-Control: no-store` on the response
- [ ] `q` excluded from Serilog request logging
- [ ] No query string reaches Seq at default level

---

## Why the response is not cached

`PublicGetPublishedArticlesQuery` already states the reason on the list endpoints:

> Only the anonymous projection is stored: an authenticated response carries per-user
> interaction flags, and caching it would show one reader another reader's likes.
> **Free-text search additionally produces an unbounded key space.**

Search is nothing *but* free text, so every argument that excluded it there excludes it
here. Caching it would mean one Redis entry per distinct query, evicting the entries
that do carry value.

`PublicSearchQuery` therefore implements plain `IQuery<T>` and neither
`ICacheableRequest` nor `IConditionallyCacheableRequest`.

### What is cached instead

Nothing in this path. The expensive part is the index scan, and the GIN index is the
mitigation. If a hot-query cache is ever wanted, cache the **top N queries by volume**
from the phase-4 telemetry, not every query reactively.

---

## HTTP caching

```csharp
.WithMetadata(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None })
```

Reader queries must not sit in an intermediary or a browser disk cache. A query can
contain a person's name; `no-store` keeps it out of shared caches entirely.

---

## Request logging

Serilog's request logging captures the full request path by default, query string
included. For this endpoint that would write every reader's query into Seq, which
`../../../../docs/search/06-analytics-and-privacy.md` forbids.

### The enricher

```csharp
namespace _116.Api.Logging;

/// <summary>
/// Redacts the search term from the logged request path so reader queries are not
/// persisted in the log store. The path and every other parameter are kept.
/// </summary>
public static class SearchQueryRedaction
{
    private const string Placeholder = "q=[redacted]";

    /// <summary>
    /// Returns the request path with the value of the `q` parameter replaced.
    /// </summary>
    /// <param name="path">The full request path including its query string.</param>
    /// <returns>The path safe to log.</returns>
    public static string Redact(string path)
    {
        return QueryTermPattern().Replace(path, Placeholder);
    }

    [GeneratedRegex(@"q=[^&]*", RegexOptions.IgnoreCase)]
    private static partial Regex QueryTermPattern();
}
```

Wired through `options.GetMessageTemplateProperties` in the `UseSerilogRequestLogging`
configuration so it applies to this route only.

### What is still logged

| Field | Logged |
| --- | --- |
| Route and method | yes |
| Status code and duration | yes |
| `scope`, `pageIndex`, `pageSize` | yes |
| `q` | **redacted** |
| Result count | yes, as a structured property |

Result count is logged deliberately: a spike in zero-result responses is an operational
signal, and it carries no reader text.

### Diagnostics

When a query genuinely needs investigating, raise the level for that request rather
than logging every query permanently. The redaction applies at the default level only.
