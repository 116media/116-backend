# Reading and Writing the Projection

## Checklist

- [x] `ISearchCatalog` is the only port the use case reads through
- [x] `ISearchDocumentProjector` is the extension point, one per type
- [x] A page is one query and the counts one `GROUP BY`
- [x] Counts report a zero for every scope, not a missing key
- [x] Search queries carry their own short command timeout

---

## Reading

`SearchDocumentCatalog` answers both calls from the one table.

```csharp
context.SearchDocuments.Where(document =>
    document.IsVisible
    && document.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(SearchConfig.Dictionary, query))
)
```

A narrowed scope adds `document.Type == type`, resolved through `SearchScopes` — the single
place the API's `EnumSearchScope` and the stored `EnumCoreContentType` meet. `All` adds
nothing, so the ranking is across types rather than within each, with no merge in memory.

`EF.Functions.WebSearchToTsQuery` must be called **inside** the expression tree. Hoisting it
into a local first throws `InvalidOperationException: the query has switched to
client-evaluation` — it exists only as a translation marker and has no in-memory body. This
was a real defect, caught by the first integration test written against it.

### Counts

```sql
SELECT s.type, count(*)::int
FROM content.search_documents AS s
WHERE s.is_visible AND s.search_vector @@ websearch_to_tsquery('french', @q)
GROUP BY s.type
```

One query for every scope. A scope the query matched nothing in is zero-filled before the
result is returned, so a client's tabs never lose a key between queries.

### Bounds

`MaxResultDepth` (1000) caps how deep a request may page. Relevance ordering cannot be served
from an index, so each page costs a scan of the match set; without the cap one anonymous
request could rank the whole corpus. Past it the page is empty and the counts still report
real totals, which is the convention Elasticsearch's `max_result_window` and Google's result
cap both follow.

Search also sets a 5-second command timeout, far below the module's 30-second default,
because the endpoint is anonymous and a pathological term must not hold a connection. The
server's own `statement_timeout` is `0`, so this is the only bound that applies.

---

## Writing

```csharp
public interface ISearchDocumentProjector
{
    EnumCoreContentType Type { get; }
    Task ProjectAsync(Guid entityId, CancellationToken ct = default);
    Task RemoveAsync(Guid entityId, CancellationToken ct = default);
    Task<int> RebuildAsync(CancellationToken ct = default);
}
```

One implementation per type, each reading its own aggregate and filling the four weight
slots. `SearchProjection` routes by type and treats a type with no projector as a no-op, so
an event for something not yet searchable costs nothing.

`ProjectAsync` is idempotent and upserts: a document is **hidden rather than deleted** when
its source stops being visible, so a republish is an update instead of a reinsert.

Projection is driven by the domain events, one handler per type. See `00-index.md` for which
events each type raises and the two gaps that have to close before lyrics and artists can be
projected.

---

## Adding a type

One projector, one `AddScoped<ISearchDocumentProjector, …>()`, one `EnumSearchScope` member
and its entry in `SearchScopes`, and one projection handler for its events. The table, the
vector, the index, the catalog, the cache decorator, the handler and the endpoint are all
untouched, and the wire format gains a key rather than changing shape.
