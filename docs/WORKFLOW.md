# Platform Workflow: User & Admin

## User Application Workflow (Front-End)

### User Journey Flow

#### 1. Landing Page / Homepage
- Featured video or article ("À la Une")
- Trending videos and articles
- **Featured articles grid (10 slots)**:
  - 1 × Alaune / Spotlight
  - 4 × Sponsored (brand-collab, featured, news, industry analysis)
  - 5 × Weekly Scandals / Gossip
- Short videos section (loopable, preload 10)
- Quick access to: Discovery, Interviews, FlexBeat, Reality, etc.
- Hero carousel (ads for products, concerts, new albums, promos)
- Call-To-Action / Modal to sign up or explore

#### 2. User Sign Up / Log In
- Email / Google / Facebook

#### 3. Explore Content
- **Articles Feed**: filter by category, artist, newest, featured
- **Videos Feed**: filter by show (FlexBeat, Interviews, etc.)
- **Search Bar**: searches both articles and videos

#### 4. Content Interaction

**Articles:**
- Like / Comment / Share
- Bookmark (stored in "My Library")

**Videos:**
- Share / Rate (1–5 stars) / Add to Playlist
- YouTube comments: displayed (latest 10), with link to full YouTube thread

**Short Videos:**
- Like / Share / Bookmark
- "View Full Video" button (not on all short videos, e.g. scandals excluded)
- View count, like count, share count, bookmark count

#### 5. My Library / Profile Page
- Saved articles (bookmarks)
- Rated videos
- Personal playlists
- Recent activity (commented articles, liked videos)

#### 6. Pricing Page
- CTA to **Get Featured**, **Submit Music**, **Book an Interview**
- Display of pricing options and submission form

---

## Key User-Facing Pages

### 1. Homepage
- Hero carousel (ads)
- Modal: promotions (concerts, new album releases, upcoming shows)
- Articles grid (10-slot featured layout)
- Videos by Category (tabs)
- Short videos section

### 2. Article Feed
- Filters: Most liked, most commented, latest, featured, à la une
- Tags: genres, themes, artists

### 3. Video Feed
- Categories as tabs: Music Videos, Interviews, FlexBeat, Reality, Documentaries, etc.
- Filters: highest rated, most shared

### 4. Single Article Page
- Full article content
- Comments section (native platform comments)
- Share + Like + Bookmark buttons
- Related articles below
- Popular articles on the side (ranked by likes + comments)
- Tags at the bottom

### 5. Single Video Page
- Embedded YouTube video
- Ratings (1–5 stars), Share, Add to Playlist
- Latest YouTube comments (10 max), with link to YouTube for full thread
- Related videos at the bottom
- Popular videos on the side (ranked by ratings)
- Lyrics link (if available)

### 6. Search Results Page
- Unified search: videos + articles
- Filter by type (video / article), category, date

### 7. Pricing Page
- List of content packages and pricing
- Payment gateway / submission form

### 8. User Profile
- Bookmarked articles
- Rated videos
- Video playlists
- Commented articles

---

## Admin Dashboard Workflow (Back-End)

### Super Admin Capabilities

| Action | Description |
|---|---|
| Approve / Reject content | Review submitted articles and videos before publishing |
| Feature / Pin content | Mark articles as À la Une, Featured, or Sponsored |
| Manage content tags | Mark content as paid or free |
| Manage pricing packages | Create, edit, archive pricing options for content and ads |
| Manage payment receipts | Upload proof of payment, mark as verified, generate receipts |
| Moderate interactions | Monitor comments, ratings, bookmarks |
| Analytics | Views, interactions, top-rated content, ad performance |
| Create ads | Set up banner ads and popup story ads |
| Manage promotional content | Schedule and configure homepage promotions |
| Manage roles & permissions | Create/update roles, assign permissions to users |

### Content Creation Workflow (Admin)

```
1. Admin creates content (article or video)
         │
         ▼
2. Selects Content Type (Article / Video)
   + Category (e.g. Artist Profile / Music Video)
   + Promotion level if paid (Featured, Alaune, Social Boost)
         │
         ▼
3. System auto-calculates total price based on category pricing
         │
         ▼
4. Content saved as DRAFT
         │
         ▼
5. Customer sends payment proof (bank transfer / MoMo receipt)
   Customer uploads receipt to portal OR sends to admin
         │
         ▼
6. Admin reviews proof of payment
   Admin marks payment as VERIFIED
   Admin uploads the proof of payment file
         │
         ▼
7. System generates downloadable receipt for the customer
         │
         ▼
8. Content status updated: PENDING → APPROVED
   Super Admin or Moderator reviews and publishes
         │
         ▼
9. Content goes LIVE
   If featured/alaune: homepage placement activated for configured duration
   If social boost: flagged for manual Facebook/Instagram promotion
```

### Video-Specific Status Flow

```
DRAFT → REVIEWED → APPROVED → PUBLISHED
  │
  └── Video requires YouTube link before PUBLISHED status can be set
```

> Customers purchase video packages (e.g. Le Focus, FlexBeat) before the shooting date. The video starts in DRAFT, then moves through review and approval before the YouTube link is added and the video is published.

---

## Optional Mobile App Features (Future Phase)

- Push notifications for featured content
- Offline reading of saved articles
- In-app content submissions
- Short video feed (native vertical scroll, like Reels)
- In-app playlist management