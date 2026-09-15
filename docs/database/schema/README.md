# Database schema export

Two artifacts, because they answer different questions. They agree — verified by diffing the
table sets, which match exactly.

`core.sql` currently runs two migrations ahead of `applied-full.sql`: `AddFileState` and
`CollapseFileStates` are generated but unapplied, so `core.files` shows `state` there and
`is_deleted`/`claimed_at` here. The table sets are unaffected.

`identity.sql` runs one migration ahead of `applied-full.sql`: `AddUserLoginState` is
generated but unapplied, so `identity.user_login_state` and the removal of
`users.failed_login_attempts`/`users.locked_until` appear there but not here.

| File | Source | Answers |
| --- | --- | --- |
| `applied-full.sql` | `pg_dump` of the migrated database | **What the schema *is*.** Final state, real Postgres DDL, includes non-EF objects. |
| `core.sql` `identity.sql` `content.sql` `mailer.sql` | `dotnet ef migrations script` | **How to build it.** Per-context, from-scratch, replays migration history. |

## `applied-full.sql` — the reference

One file, `pg_dump --schema-only`, taken after every migration was applied. Read this when you
want to know what a table actually looks like.

| Schema | Tables |
| --- | --- |
| `content` | 50 — 49 entities + `domain_event_outbox` |
| `identity` | 10 — 9 entities + `domain_event_outbox` |
| `mailer` | 4 — 3 entities + `domain_event_outbox` |
| `core` | 3 — 1 entity + `domain_event_outbox` + `processed_domain_events` |
| `quartz` | 12 — scheduler tables, **created at runtime, present in no migration** |
| `public` | 1 — `__EFMigrationsHistory`, shared by all four contexts |

The entity-table count matches `Domain/Entities/*.cs` exactly in every module (49 / 9 / 3 / 1),
except Identity, whose 10th entity (`UserLoginStateEntity`) lands with the unapplied
`AddUserLoginState` migration.

The `quartz` schema is the reason this file exists at all: Quartz creates its 12 tables itself
at startup, so they appear in no EF migration and no generated script. A dump is the only way
to see the complete database.

## The four `*.sql` scripts — provisioning

Generated from the migrations on disk, so they are valid even when no database exists. Each is
a from-scratch script for an **empty** database.

They replay history rather than describe the end state: `processed_domain_events` is created
and then dropped again in Content, Identity and Mailer, so grepping `CREATE TABLE` over them
overcounts by three. Applied in full, the result is identical to `applied-full.sql` — confirmed
by applying all four to a throwaway database and comparing table sets.

## Which to use

- **Reading the schema, writing a query, checking a column type or constraint name** →
  `applied-full.sql`.
- **Creating a database from nothing, or CI** → the four scripts.
- **Anything about Quartz** → `applied-full.sql` only.

`applied-full.sql` is only as current as the database it came from. This one was taken with all
53 migrations applied, so it matches the model. Re-dump after applying new migrations:

```bash
docker exec 116_db pg_dump --schema-only --no-owner --no-privileges -U postgres -d 116_db \
  > docs/database/schema/applied-full.sql
```

Check it is current before trusting it — compare the count against the migration files on disk:

```bash
docker exec 116_db psql -U postgres -d 116_db -tAc 'select count(*) from public."__EFMigrationsHistory"'
```

The API applies migrations itself at startup (`src/Api/DatabaseMigrator.cs`), so booting the
compose stack is enough to bring the database up to date.

## Why these are plain scripts, not `--idempotent`

`--idempotent` wraps every migration in a `DO $EF$ … $EF$` PL/pgSQL block, and Content uses
seven `CREATE INDEX CONCURRENTLY` statements, which PostgreSQL refuses to run inside a
transaction or function body. The idempotent variant of `content.sql` therefore fails partway
through with `CREATE INDEX CONCURRENTLY cannot run inside a transaction block`.

These are plain from-scratch scripts instead: run them against an **empty** database. All four
were verified to apply cleanly that way.

## Regenerating

`dotnet ef` instantiates the real composition root, so the boot-time configuration validator
runs and every required environment variable must be present. `.env` does not currently carry
all of them, so the missing ones are supplied inline for the command only — never written to
`.env`:

```bash
cd apps/backend
set -a; . ./.env; set +a
export FRONTEND_BASE_URL="https://example.com"
export OTP_PEPPER="0123456789abcdef0123456789abcdef0123456789abcdef"
export JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS=30
export EMAIL_PROVIDER=Smtp EMAIL_FROM_ADDRESS="no-reply@example.com" EMAIL_FROM_NAME="116"
export SMTP_HOST=localhost SMTP_PORT=1025 SMTP_USERNAME=u SMTP_PASSWORD=p SMTP_USE_STARTTLS=false
export RESEND_API_KEY=dummy GOOGLE_CLIENT_ID=dummy FACEBOOK_APP_ID=dummy FACEBOOK_APP_SECRET=dummy
export ODESLI_API_URL="https://api.song.link/v1-alpha.1" ODESLI_API_KEY=dummy
export SEQ_URL="http://localhost:5341" TRUSTED_PROXY_NETWORKS="127.0.0.1/32"

for m in Core Identity Content Mailer; do
  dotnet ef migrations script \
    --project "src/Modules/$m/$m" \
    --startup-project src/Api \
    --context "${m}DbContext" \
    --output "docs/database/schema/$(echo $m | tr 'A-Z' 'a-z').sql"
done
```

The values above are throwaway placeholders that only need to satisfy the validator — the
script generator never opens a connection or contacts any of these services.

## Applying

The four scripts are independent and order-insensitive; each creates its own schema and they
share `public.__EFMigrationsHistory`. They are from-scratch scripts, so they expect an empty
database — re-running one against an already-migrated database will fail on the first existing
object.

```bash
docker exec 116_db psql -U postgres -tAc "CREATE DATABASE schema_check;"
for f in core identity content mailer; do
  docker exec -i 116_db psql -v ON_ERROR_STOP=1 -U postgres -d schema_check < "$f.sql"
done
```

## `dbml/` — for dbdiagram.io

**Use these.** DBML, not SQL — paste the file straight into the dbdiagram.io editor pane.

| File | Tables | Relationships |
| --- | --- | --- |
| `dbml/identity.dbml` | 11 | 9 |
| `dbml/core.dbml` | 3 | 0 |
| `dbml/mailer.dbml` | 4 | 0 |
| `dbml/content.dbml` | 50 | 61 |

Carries primary keys, not-null, defaults, single and composite unique indexes, and every
foreign key with its `ON DELETE` behaviour. Check constraints become a table `Note`, since
DBML has no equivalent.

### Pasting SQL there will never work

The dbdiagram.io editor pane accepts **DBML only**. SQL pasted into it fails on line 1, because
DBML comments are `//` and SQL uses `--`:

```
A Custom element can only appear in a Project or a Dep     1:1
Unexpected token '$'                                       8:4
```

SQL has to go through **Import → PostgreSQL**, a separate dialog. The `dbml/` files skip that
step entirely.

### And never paste the provisioning scripts

`core.sql` / `identity.sql` / `content.sql` / `mailer.sql` replay migration history: a table is
created without its later columns, which then arrive via `ALTER TABLE ... ADD`. Every SQL
importer ignores `ALTER TABLE ... ADD COLUMN`, so it builds the table from the original
`CREATE TABLE` and then fails on the first index naming a column it never saw:

```
No column named 'is_active' inside Table 'identity.permissions'
No column named 'provider_subject_id' inside Table 'identity.users'
```

Those columns do exist — they are just added later in the script.

### Regenerating the DBML

Derived from `applied-full.sql`, so re-dump that first if migrations have moved. Four
normalizations are applied: PostgreSQL type names map to DBML ones (`character varying(n)` →
`varchar(n)`, `timestamp with time zone` → `timestamptz`), named inline constraints
(`CONSTRAINT x NOT NULL`) collapse to `NOT NULL`, table-level `CHECK` clauses move to a `Note`,
and `::type` casts are stripped from defaults.
