# Stage 15.9 — Content navigation census

The measured inventory 15.10 and 15.11 tick against. Every entity-typed navigation property in
`src/Modules/Content/Content/Domain/Entities/` — 64 in total — classified as **kept** (a
root→member collection inside one aggregate boundary) or **deleted** (a cross-aggregate or
back-reference navigation replaced by the Guid FK that already exists).

Counts: **10 kept**, **54 deleted**, **2 added** (`Article.Artists`, `Artist.SocialLinks` — their
join tables exist today with a bare `.WithMany()` and no root-side collection). The spec's Part C
figure of 26 counted only the forward cross-aggregate references; the back-references fall out of
the same change and are enumerated here so nothing survives by omission.

Read-site classification: **M** = read on a loaded entity in a mapper/handler/factory — replaced
by a batched id lookup (the `files`-dictionary pattern). **Q** = read inside an EF query
expression — rewritten as a correlated `Any` in the owning repository. **dead** = never read
outside `Domain/` and its EF configuration; deleting it touches nothing but the entity and the
configuration line.

## 1. Kept — root→member collections (10)

| # | Navigation | Declared | Configured |
| --- | --- | --- | --- |
| 1 | `ArticleEntity.Images` | `ArticleEntity.cs:205` | `ArticleImageConfiguration.cs:32` |
| 2 | `ArticleEntity.Tags` | `ArticleEntity.cs:210` | `ArticleTagConfiguration.cs:24` |
| 3 | `VideoEntity.Tags` | `VideoEntity.cs:194` | `VideoTagConfiguration.cs:24` |
| 4 | `LyricsEntity.Tags` | `LyricsEntity.cs:225` | `LyricsTagConfiguration.cs:24` |
| 5 | `ContentOrderEntity.Items` | `ContentOrderEntity.cs:52` | `ContentOrderItemConfiguration.cs:27` |
| 6 | `ContentOrderEntity.Payment` | `ContentOrderEntity.cs:58` | `ContentPaymentConfiguration.cs:35` |
| 7 | `ContentOrderItemEntity.Tiers` | `ContentOrderItemEntity.cs:75` | `ContentItemTierConfiguration.cs:21` |
| 8 | `CategoryEntity.Pricing` | `CategoryEntity.cs:110` | `CategoryPricingConfiguration.cs:26` |
| 9 | `PackageEntity.Slots` | `PackageEntity.cs:36` | `PackageSlotConfiguration.cs:25` |
| 10 | `PlaylistEntity.Videos` | `PlaylistEntity.cs:27` | `PlaylistVideoConfiguration.cs:26` |

After 15.10 each pair is configured from the root side
(`HasMany(root => root.Members).WithOne().HasForeignKey(m => m.RootId)`), the member's
back-reference property (section 3) having been deleted.

## 2. Deleted — cross-aggregate navigations (28)

The spec's 26, plus two inverse collections it did not count (`Video.Shorts`,
`Category.PackageSlots` — both dead). The FK column stays; the configuration becomes the
navigation-less `HasOne<T>().WithMany().HasForeignKey(...)` form with the same delete behaviour.

| # | Navigation | Declared | Includes | Reads | Kind |
| --- | --- | --- | --- | --- | --- |
| 1 | `Article.Category` | `ArticleEntity.cs:198` | 12 | `ArticleMapper.cs:70,122,200,321` | M |
| 2 | `Article.Customer` | `ArticleEntity.cs:193` | 2 | `ArticleMapper.cs:150` | M |
| 3 | `Article.PromotionLevel` | `ArticleEntity.cs:114` | 4 | `ArticleMapper.cs:135`; `ArticleSpecifications.cs:280` | M + Q |
| 4 | `Video.Category` | `VideoEntity.cs:189` | 14 | `VideoMapper.cs:51,126,154,194`; `PlaylistMapper.cs:24,96` | M |
| 5 | `Video.Customer` | `VideoEntity.cs:184` | 2 | `VideoMapper.cs:75` | M |
| 6 | `Video.PromotionLevel` | `VideoEntity.cs:102` | 4 | `VideoMapper.cs:64`; `VideoSpecifications.cs:193` | M + Q |
| 7 | `Video.Shorts` | `VideoEntity.cs:199` | 0 | none | dead |
| 8 | `Lyrics.Video` | `LyricsEntity.cs:230` | 0 | `LyricsSpecifications.cs:148-149` | Q |
| 9 | `Lyrics.Customer` | `LyricsEntity.cs:235` | 4 | `LyricsMapper.cs:209` | M |
| 10 | `Lyrics.Category` | `LyricsEntity.cs:240` | 9 | `LyricsMapper.cs:64,185,254,329` | M |
| 11 | `Category.ContentType` | `CategoryEntity.cs:105` | 10 | `CategoryMapper.cs:30`; 4 admin category handlers; `PublicGetVideoFeedHandler.cs:43`; `AdminCreateOrderFactory.cs:70` | M |
| 12 | `Category.PackageSlots` | `CategoryEntity.cs:115` | 0 | none (only the inverse side of #24) | dead |
| 13 | `ArticleComment.Article` | `ArticleCommentEntity.cs:56` | 0 | `ArticleCommentRepository.cs:238` | Q |
| 14 | `ArticleComment.ParentComment` | `ArticleCommentEntity.cs:46` | 0 | none — every read is `ParentCommentId` | dead |
| 15 | `PlaylistVideo.Video` | `PlaylistVideoEntity.cs:33` | 2 | `PlaylistRepository.cs:28,50` (filtered include); `PlaylistMapper.cs` ×16 | Q + M |
| 16 | `ContentOrder.Customer` | `ContentOrderEntity.cs:42` | 3 | `ContentOrderSpecifications.cs:56-58`; `ContentOrderMapper.cs:51,57,69` | Q + M |
| 17 | `ContentOrder.Package` | `ContentOrderEntity.cs:47` | 0 | none | dead |
| 18 | `ContentOrderItem.Category` | `ContentOrderItemEntity.cs:65` | 1 | `ContentOrderMapper.cs:29`; `CommerceCustomerNotifier.cs:269` | M |
| 19 | `ContentOrderItem.PromotionLevel` | `ContentOrderItemEntity.cs:70` | 1 | `ContentOrderMapper.cs:31` | M |
| 20 | `ContentItemTier.PricingTier` | `ContentItemTierEntity.cs:37` | 1 | `ContentOrderMapper.cs:23` | M |
| 21 | `CategoryPricing.PricingTier` | `CategoryPricingEntity.cs:37` | 6 | `CategoryMapper.cs:25` | M |
| 22 | `PackageSlot.Category` | `PackageSlotEntity.cs:43` | 5 | `PackageMapper.cs:21,37,38`; `AdminCreateOrderFactory.cs:70` | M |
| 23 | `ShortVideo.ParentVideo` | `ShortVideoEntity.cs:103` | 6 | `ShortVideoMapper.cs:72,175` | M |
| 24 | `StreamingLink.Album` | `StreamingLinkEntity.cs:45` | 0 | none | dead |
| 25 | `StreamingLink.Lyrics` | `StreamingLinkEntity.cs:50` | 0 | none | dead |
| 26 | `ArticleArtist.Artist` | `ArticleArtistEntity.cs:32` | 0 | none | dead |
| 27 | `ArticleTag.Tag` | `ArticleTagEntity.cs:29` | 3 | `ArticleMapper.cs:36-38` | M |
| 28 | `VideoTag.Tag` / `LyricsTag.Tag` | `VideoTagEntity.cs:29` / `LyricsTagEntity.cs:29` | 3 / 4 | `VideoMapper.cs:32-34`, `VideoSpecifications.cs:77`; `LyricsMapper.cs:33-35` | M + Q / M |

## 3. Deleted — member → parent back-references (12)

None is read anywhere outside `Domain/` except `ContentPayment.Order` and `ArticleArtist.Article`;
both die with the 15.10 Commerce/Editorial rewrites. The relationship moves to the root-side
configuration.

| # | Back-reference | Configured | Reads |
| --- | --- | --- | --- |
| 1 | `ArticleImage.Article` | `ArticleImageConfiguration.cs:32` | dead |
| 2 | `ArticleTag.Article` | `ArticleTagConfiguration.cs:24` | dead |
| 3 | `ArticleArtist.Article` | `ArticleArtistConfiguration.cs:25` | Q — `ArtistContentSpecifications.cs:38`, `ArtistRepository.cs:114,153` |
| 4 | `VideoTag.Video` | `VideoTagConfiguration.cs:24` | dead |
| 5 | `LyricsTag.Lyrics` | `LyricsTagConfiguration.cs:24` | dead |
| 6 | `PlaylistVideo.Playlist` | `PlaylistVideoConfiguration.cs:26` | dead |
| 7 | `ContentOrderItem.Order` | `ContentOrderItemConfiguration.cs:27` | dead |
| 8 | `ContentItemTier.OrderItem` | `ContentItemTierConfiguration.cs:21` | dead |
| 9 | `ContentPayment.Order` | `ContentPaymentConfiguration.cs:35` | Q + M — `ContentOrderSpecifications.cs:109-111`; `ContentOrderRepository.cs:128`; `ContentOrderMapper.cs:44-45,116-117` |
| 10 | `PackageSlot.Package` | `PackageSlotConfiguration.cs:25` | dead |
| 11 | `CategoryPricing.Category` | `CategoryPricingConfiguration.cs:26` | dead |
| 12 | `ArtistSocialLink.Artist` | `ArtistSocialLinkConfiguration.cs:25` | dead |

## 4. Deleted — interaction root → target back-references (13)

Interaction aggregates keep referencing their target **by id**; the object reference goes. Eight
carry query-side predicates ("target is published/active") that become correlated `Any` in the
owning repository, paired with the two-step listing shape already used by
`ArticleInteractionRepository.GetSharedArticlesAsync`.

| # | Back-reference | Reads |
| --- | --- | --- |
| 1 | `ArticleLike.Article` | Q — `ArticleInteractionRepository.cs:156-158` |
| 2 | `ArticleBookmark.Article` | Q — `ArticleInteractionRepository.cs:128-130` |
| 3 | `ArticleShare.Article` | Q — `ArticleInteractionRepository.cs:183` |
| 4 | `ArticleCommentLike.Comment` | dead |
| 5 | `ShortVideoLike.ShortVideo` | Q — `ShortVideoRepository.cs:116-120` |
| 6 | `ShortVideoBookmark.ShortVideo` | Q — `ShortVideoRepository.cs:144-148` |
| 7 | `ShortVideoShare.ShortVideo` | Q — `ShortVideoRepository.cs:172` |
| 8 | `ShortVideoViewEvent.ShortVideo` | dead |
| 9 | `LyricsLike.Lyrics` | dead |
| 10 | `LyricsShare.Lyrics` | dead |
| 11 | `LyricsViewEvent.Lyrics` | dead |
| 12 | `VideoRating.Video` | Q — `VideoRepository.cs:232,236-237` |
| 13 | `VideoShare.Video` | Q — `VideoRepository.cs:266` |

## 5. Added — root collections the target model requires (2)

| Navigation | Why |
| --- | --- |
| `ArticleEntity.Artists` (`ICollection<ArticleArtistEntity>`) | `ReplaceArtists(ids)` needs a collection; the join table exists, `ArticleArtistConfiguration.cs:25` is a bare `.WithMany()`. No migration — same FK, same table. |
| `ArtistEntity.SocialLinks` (`ICollection<ArtistSocialLinkEntity>`) | `SetSocialLink`/`RemoveSocialLink` need a collection; `ArtistSocialLinkConfiguration.cs:25` is a bare `.WithMany()`. No migration. |

## 6. Include inventory in `Infrastructure/Repositories` (135)

96 reference section-2 navigations, 6 reference section-3/4 back-references, 33 reference the
kept collections — of which 3 (`PackageRepository.cs:27-28, 56-60, 76-80`) chain through the
deleted `PackageSlot.Category` and are rewritten with it. After 15.11 the only `Include` targets
are the section-1 collections and `ContentOrder.Payment`.
