# JobTrek — Design & UX decisions

This document captures the product/design/UX decisions made while turning the app into a
sellable, mobile-friendly B2C product. It exists so the same decisions can be reproduced if the
frontend is ever **ported to React** (the backend/API stays as-is — see the last section).

> The **frontend has no business logic** — it is a thin client over the REST API. Every screen is
> "call an endpoint → show the result → send an action back". That is what makes a port mostly
> mechanical: the hard parts (product decisions below) are already made.

---

## 1. Brand

- **Name: JobTrek** (renamed from *JobFlow* — wanted a "Job + something" name that signals the
  *process/tracking* and reads in both Croatian and English). Centralized in
  `Web/Branding.cs` (`Branding.Name`), but a rename must also sweep: `Components/App.razor`
  (`<title>` + meta), each page `<PageTitle>`, the three auth `.cshtml` (title/logo/footer), CSS
  header comments, and the DB translation `mkt.hero.subtitle` (hr+en).
- Demo login emails stay `@jobflow.app` — they are seeded credentials, **not** the brand.

### Design tokens
| Token | Value |
|---|---|
| Primary (indigo) | `#4F46E5` |
| Accent (violet) | `#7C3AED` |
| Brand gradient | `linear-gradient(135deg, #4F46E5, #7C3AED)` |
| Ink / text | `#0F172A` |
| Muted text | `#64748B` |
| Border / line | `#E5E7EB` |
| Soft background | `#F7F8FA` |
| Font | `Inter` |
| Corner radius | 10px inputs/buttons, 16px cards, 999px pills |

- MudBlazor theme: `Web/Theming/BrandTheme.cs` (indigo/violet, white app bar + drawer).
- Gradient hero header (`.jf-page-header` in `wwwroot/app.css`) is the recurring page-title motif.

---

## 2. Internationalization (i18n)

- **DB-stored, admin-editable translations**; languages **hr + en**, **default hr**.
- `Translation` entity (Key, LanguageCode, Value). `GET /translations` (anonymous),
  `PUT /translations` (EmployeeOnly). Admin editor at `/translations`.
- `TranslationSeeder` inserts any **missing** key on every startup (never wipes admin edits).
- **Web:** `LocalizationService`
  - `T(key)` → current language → hr fallback → key.
  - `TValue(prefix, value)` → resolves `"{prefix}.{value}"` for **data-derived values**
    (`jobType.FullTime`, `workLocation.Remote`, `stepType.PhoneScreen`, `outcome.Passed`), falling
    back to the raw value. Statuses use lowercase keys (`status.applied`) via a per-page
    `StatusKey(status)` switch (NOT TValue).
  - Language persisted to `localStorage` **and** a `jf-lang` **cookie** so the server-rendered MVC
    auth pages can read it.
- **Gotcha:** any component using `T()` in first-paint content must `await Loc.EnsureLoadedAsync()`
  at the start of `OnInitializedAsync` — on a fresh circuit translations aren't loaded yet, and
  subscribing to `OnChange` alone isn't enough. Symptom: raw keys flash on screen.

---

## 3. Auth pages

- `/Login` (candidate) + `/Signup` are **MVC `.cshtml`** (not Blazor). Employee/admin login is a
  separate route **`/admin/login`** (`AdminController`). There's a discreet link from `/Login`.
- All three must be **complete HTML documents** (`Layout = null`) — a fragment shows blank under
  the interactive Blazor router.
- They read the `jf-lang` cookie for the language and pull the **same DB-backed translations** as the
  rest of the app via `ServerSideTranslations` (a scoped MVC-side counterpart of `LocalizationService`
  that fetches `GET /translations` once per request). So auth text is **admin-editable** too — no
  hardcoded strings. Keys live under `auth.*` in the seeder. (Only the example-name *placeholders*
  like "Ivan"/"Jane" stay inline — they're hints, not content.)
- On mobile the split-screen brand panel is hidden, so each page shows a compact **`.auth-mobile-brand`**
  logo at the top so it isn't a bare form.

---

## 4. Mobile UX patterns (the important part)

The app targets an **app-like** feel on phones. Blazor Server + MudBlazor is a web stack, so the
polish came from a handful of deliberate patterns:

- **Full-screen dialogs on mobile.** Below 700px, every MudDialog becomes a full-screen sheet
  (`100vw × 100dvh`, no radius) with a **scrollable body** (`app.css`). A floating web modal strands
  fields (and the "create company" button) behind the on-screen keyboard; full-screen puts fields
  near the top so their dropdowns get room above the keyboard.
- **Native `<select>` for fixed choices.** Status, job type, work location, source use plain
  `<select>` → the **OS picker**. Reliable on touch, no keyboard, and it sidesteps MudBlazor popover
  mis-positioning inside scroll containers/dialogs. (MudBlazor popovers can open off-screen on mobile.)
- **Kanban board — dual render.**
  - Desktop: `MudDropContainer` **drag-and-drop**.
  - Mobile (<700px): a **plain horizontal scroll-snap carousel** (no MudDropContainer) — because
    MudDropContainer captures `touchmove` for its own drag and **blocks native scrolling**, so the
    board wouldn't move. Status is changed via a **"Premjesti u"** `<select>` on each card
    (HTML5 drag doesn't work on touch anyway).
- **Applications list — dual render.** Desktop: `MudDataGrid`. Mobile: compact **cards** (title +
  company·source + type/location chips + status pill + chevron); the **whole card is a link** to the
  detail page.
- **Status can be changed in multiple places:** board (desktop drag / mobile select) **and** the
  **detail page** (a `<select>` styled as the status pill in the gradient hero). All call the same
  `ChangeJobApplicationStatus` command.
- **Drawer:** responsive — persistent on desktop, hamburger overlay on mobile that **auto-closes
  after navigating**. Duplicate brand hidden on mobile.
- **Logout** is a plain `<a href="/logout" data-enhance-nav="false">` icon (not a JS `OnClick`) so it
  works even if the SignalR circuit drops (e.g. over a tunnel).

---

## 5. Forms & validation

- Outlined + dense fields, tightened spacing (no oversized gaps).
- Fixed-choice fields are **`MudSelect`** with a **`Comparer`** (the DTOs use reference equality, so
  compare by value/id) **and** a **`ToStringFunc`** — without ToStringFunc a pre-selected value shows
  the DTO's class name.
- **FluentValidation messages are Croatian** (`Web/Validators/*`), e.g. "Naziv radnog mjesta je
  obavezan.", "Odaberi tvrtku s popisa."
- **URL validation is lenient**: accepts links with or without a scheme (`www.tvrtka.com`,
  `tvrtka.com`, `https://…`); the value is **normalized** with `https://` on save so stored links are
  clickable (`FluentValidationExtensions.NormalizeUrl`).

---

## 6. CSS / caching notes

- Static CSS (`app.css`, `marketing.css`, `auth.css`) is served from the physical output `wwwroot`.
  Each needs `<Content Update CopyToOutputDirectory="Always">` in `Web.csproj`.
- Browsers cache these; bump the query version (`app.css?v=N`) in `App.razor` after any CSS change.

## 7. Gotchas learned the hard way

- `MudDropContainer` blocks touch scroll → plain board on mobile.
- `MudSelect` shows `ToString()` (class name) without `ToStringFunc`.
- `overflow-x: auto` on the board makes it a scroll container → **breaks sticky headers and stops the
  page from growing vertically**. Keep board `overflow: visible`; let the page scroll.
- Setting `py`/`pt` on `MudMainContent` **overrides its built-in app-bar top offset** → content slides
  under the app bar. Add breathing room via an inner wrapper instead.
- MudBlazor 6.19.x already provides `MudPopoverProvider` — adding one manually crashes the circuit.
- Linking from an interactive Blazor page to an MVC endpoint: the router swallows the click. Force a
  real navigation (`NavigateTo(url, forceLoad: true)` / `AuthLink`) or a plain anchor with
  `data-enhance-nav="false"`.
- Client-generated Guid PKs added via a navigation collection need `ValueGeneratedNever()`.

---

## 8. Porting the frontend to React — how easy?

**Short answer: the backend is 100% reusable; the frontend is a rewrite, but a mostly mechanical one
because there's no business logic in it.**

### Reused as-is (no changes)
- **The whole API** — standalone ASP.NET Core Web API, JWT auth (`BearerEmployee` / `BearerCandidate`),
  REST endpoints. React consumes the exact same endpoints.
- **i18n data** — `GET /translations` (anonymous) + `PUT /translations`. React fetches the same JSON;
  reimplement `T()` / `TValue()` as a tiny hook/context. Translations, keys and admin editor logic
  are unchanged.
- **Auth flow** — same JWT bearer tokens; store in memory/httpOnly cookie and send `Authorization`.
- **Validation rules** — the API validates server-side (FluentValidation), so React can rely on the
  server and/or mirror the rules client-side (they're documented above).
- **Design tokens + every UX decision in this document.**

### Rewritten (the actual work)
- Blazor components/pages → **React components**. Closest UI lib to MudBlazor is **MUI**
  (or Tailwind + shadcn/ui for a more custom look).
- **ViewModels → React state**: `React Query`/`TanStack Query` for server state (list/detail/board),
  local component state or `Zustand` for UI state. The ViewModels map almost 1:1 to query hooks +
  a bit of local state.
- MVC auth `.cshtml` → React auth routes/pages.

### Effort estimate
- **Low-to-moderate.** Because the frontend is a thin client, the port is "recreate ~10 screens that
  call already-existing endpoints, following the documented UX." The hard, iterative parts (product
  decisions, mobile patterns, i18n design, brand) are **done**.
- **Bonus:** React makes the *native feel* easier — trivial **PWA** (installable, standalone,
  offline shell), better touch/gesture libraries, and a path to **React Native** if a true native app
  is ever wanted.

### Practical porting order
1. Auth + a typed API client (generated from Swagger/OpenAPI).
2. `T()`/`TValue()` i18n hook fed by `GET /translations`.
3. Shared layout (app bar + responsive drawer) + design tokens.
4. Screens: dashboard → applications list → application detail → **board** → translations admin.
5. Re-apply the mobile patterns from §4 (full-screen sheets, native selects, dual board/list).
