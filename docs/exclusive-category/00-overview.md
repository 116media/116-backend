# Exclusive Category (Show) — Overview

## Context

Categories are conceptually **shows**. Each show can optionally have a **poster image** and can be marked as **exclusive**. Only one show can be exclusive at any given time — selecting a new exclusive show automatically unsets all others (mutex pattern).

### What "exclusive" means on the frontend

The show marked as **exclusive** is the one that appears on the **homepage after the promotion feed section**. It renders as a **two-column layout**:

- **Left column**: the show's **poster image**, an "exclusive" **tag**, the show's **title** and **description**, and a **"Watch Now"** button
- **Right column**: a vertical list of **horizontal video cards** belonging to that show, stacked in a single column

This layout will be built in the new frontend (`apps/frontend`) below the `ArticlePromotionFeed` section. The poster image is what gives the exclusive section its visual identity, which is why `PosterFileId` and `IsExclusive` are introduced together.

This feature adds two new fields to `CategoryEntity`:

| Field          | Type   | Description                                                                                                                                                                                                      |
|----------------|--------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `PosterFileId` | `Guid?` | Optional reference to a `FileEntity` storing the show's poster image in Cloudinary. Optional because article categories do not need a poster — only video categories (shows) use it for the exclusive homepage section. |
| `IsExclusive`  | `bool` | Whether this show is the currently featured exclusive. Mutex — at most one category has this set to `true`.                                                                                                      |

## Existing Pattern: IsGossip

The `IsGossip` field already implements the same mutex concept:

- **Entity**: `bool IsGossip` property, set via `Create()` and `Update()` methods
- **EF Configuration**: unique filtered index `HasFilter("is_gossip_fallback = true")` — database-level enforcement that at most one row is `true`
- **Specification**: `GossipCategorySpecification` — `category.IsGossip && category.IsActive`
- **Repository**: `GetGossipCategoryAsync()` — retrieves the single gossip category
- **DTO**: `bool IsGossip` field on `CategoryDto`
- **Handlers**: `Create` and `Update` both accept `IsGossip` and pass it through to the entity

`IsExclusive` follows this exact pattern, plus the mutex enforcement logic in the handler layer (unset previous exclusive before setting new one).

## Scope

| Doc | Contents |
| --- | -------- |
| [01-domain-entity.md](01-domain-entity.md) | Entity property and method changes |
| [02-ef-configuration.md](02-ef-configuration.md) | EF config, index, and migration |
| [03-poster-upload.md](03-poster-upload.md) | New poster upload endpoint and handler |
| [04-dto-and-mapper.md](04-dto-and-mapper.md) | DTO changes and async mapper rewrite |
| [05-repository-and-specification.md](05-repository-and-specification.md) | Repository and specification additions |
| [06-handler-changes.md](06-handler-changes.md) | Create, Update, and query handler changes |
| [07-error-messages.md](07-error-messages.md) | New error keys for exclusive mutex |
| [08-ef-migration.md](08-ef-migration.md) | Migration generation command and naming |
| [09-tests.md](09-tests.md) | Test plan across entity, handler, mapper, endpoint, and specification |
| [10-file-inventory.md](10-file-inventory.md) | Complete list of files to create or modify |
