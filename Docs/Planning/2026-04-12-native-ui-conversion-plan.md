# BE Home Native UI Conversion Plan

## Planning Date

April 12, 2026.

## Why This Plan Exists

`be-home` is live today, so we need to keep the hosted WebView implementation shippable while we investigate a first-party native replacement. The recent Android renderer failures have turned too much engineering time into WebView recovery, crash forensics, and device-specific workarounds instead of product progress.

At the same time, a native UI path has meaningful upside:

- removes the hosted website renderer from the critical browse experience
- gives `be-home` direct ownership of its navigation, layout, and lifecycle
- forces us to harden a real C# client over our maintained BE web API
- validates our public API contracts from a second first-party consumer

This plan is intentionally incremental. We should not try to replace the whole hosted site in one jump.

## Current Constraints

### 1. We must keep supporting the WebView path

The hosted website flow is still the only complete production-capable path, especially for authentication and other account-driven workflows.

### 2. Board SDK input fields are still a blocker

UI Toolkit input fields remain unreliable on Board, which means native sign-in and other text-entry-heavy flows are not currently a safe near-term target.

### 3. We should avoid environment-specific product logic

The native path should work against both `staging` and `production` through the same maintained runtime settings surface. No staging-only code paths should be introduced.

### 4. “Native mode” and “WebView package fully stripped” are related but not identical

This wave adds a build/runtime mode that does not initialize the hosted browse WebView path. That is enough to prevent native-mode builds from producing normal hosted-browse WebView behavior and related false-positive runtime logs.

A later follow-up may still be needed if we want to physically remove the WebView package/dependency from native-only builds at compile/package time.

## Decision

We will move forward with a **native UI spike** while preserving the hosted WebView implementation.

The maintained rollout model is:

1. Keep the hosted WebView implementation working for live support.
2. Add a build-selectable native UI mode.
3. Port one BE API endpoint at a time into a native UI Toolkit shell.
4. Hold off on native auth flows until Board input support is viable.

## Build Modes

`be-home` now has two shell modes:

- `HostedWebView`
  - Current production path.
  - Uses the maintained hosted website inside the embedded WebView shell.
- `NativeCatalogSpike`
  - Experimental native UI Toolkit path.
  - Does not initialize the main hosted browse WebView path.
  - Starts with direct BE API catalog consumption.

These modes are controlled through:

- Project Settings: `Project > BE Home`
- Optional Build Profile override: `BE Home`

This keeps the switch environment-aware and build-aware without hardcoding behavior to `staging` or `production`.

## Architectural Direction

### UI stack

- Continue using **UI Toolkit**.
- Keep authored layout in **UXML** and authored styling in **USS**.
- Avoid inline styling unless there is a specific unavoidable runtime reason.

### View pattern

The native spike should follow the existing **MVVMC-style** direction already documented in [MVVMC.md](../Dev/MVVMC.md):

- `Model`
- `ViewModel`
- `View`
- `Controller`
- `UserIntent`

The current spike uses a lightweight local version of that pattern inside `be-home` rather than importing or modifying the reference project at `C:\Source\matt-stroman\board_naval-warfare\Assets\Game`.

### API client direction

The native path should consume the maintained BE API through `be-home` C# services:

- contract DTOs in `Assets/Scripts/Api/Contracts`
- runtime models in `Assets/Scripts/Api/Models`
- endpoint services in `Assets/Scripts/Api/Services`

This gives us a clear endpoint-by-endpoint migration path and keeps the native client grounded in the maintained backend contract.

## Initial Spike Scope

### Implemented in this wave

- build/runtime shell toggle between hosted WebView and native spike
- native UI Toolkit browse host inside `MainScreen`
- first native API client slice for `GET /catalog`
- native browse list-first screen that renders public catalog summaries without requiring native text input

### Explicitly out of scope for this wave

- native sign-in
- native Quick View parity
- native title-detail endpoint integration
- native purchase/install flow
- fully stripping the WebView package from the Unity project at compile/package time
- replacing every hosted page at once

## Endpoint-by-Endpoint Migration Plan

### Wave 1: `GET /catalog`

Goal:

- Prove that native `be-home` can consume the public catalog directly from the BE API.
- Validate environment targeting, HTTP transport, JSON mapping, and UI Toolkit rendering without the hosted browse WebView.

Deliverables:

- `BeHomeCatalogService`
- catalog DTOs and runtime models
- native browse screen with:
  - list/status panel
  - refresh intent
  - selected-title summary panel

Success criteria:

- native mode loads catalog results on device
- no hosted browse WebView is initialized in native mode
- the same native build logic works against both `staging` and `production`

### Wave 2: `GET /catalog/{studioIdentifier}/{titleIdentifier}`

Goal:

- Replace the first real “tap into title details” flow natively.

Deliverables:

- title-detail DTO/model/service
- native detail surface for:
  - description
  - media metadata selection state
  - release/acquisition summary

Notes:

- this is the highest-value follow-up because the current hosted flow is most unstable around Quick View and title-detail transitions

### Wave 3: supporting public browse metadata endpoints

Candidate endpoints:

- genre catalog
- studios list/detail
- home spotlights or curated browse groupings as needed

Goal:

- Rebuild the browse landing and filter/navigation story incrementally instead of treating `/catalog` as a dead-end list.

### Wave 4: outbound acquisition/install handoff

Goal:

- decide what native mode should do when a player wants to acquire a title

Likely options:

- open a controlled hosted detail/acquisition page
- open a minimal external browser handoff
- continue using hosted content only for the purchase/install segment

This should remain separate from the browse-list migration.

### Wave 5: native auth, only after Board input support is viable

Goal:

- revisit native sign-in after Board input-field support is no longer a blocker

Until then:

- keep auth-sensitive experiences on the hosted path
- or gate them behind limited hosted handoffs from the native shell

## Rollout Strategy

### Short term

- Keep `HostedWebView` as the default production-safe mode.
- Use `NativeCatalogSpike` for targeted internal builds and device validation.

### Medium term

- Add more native screens behind the same mode switch.
- Keep a clear ledger of which user journeys are native vs hosted.

### Long term

- Once native browse/title-detail/install flows are stable enough, decide whether the hosted WebView path should:
  - remain as a fallback mode
  - be retained only for auth-heavy paths
  - or be retired entirely

## Testing Strategy

### Unit tests

Every new endpoint migration wave should add tests for:

- DTO-to-model mapping
- error handling for incomplete/invalid payloads
- native view-model/state-building logic

### Device validation

For each migrated screen, validate:

- environment targeting on both `staging` and `production`
- initial load behavior
- repeated refresh/navigation behavior
- renderer/log noise differences compared to hosted mode

### Logging

Native-mode logs should stay explicit and product-scoped:

- successful API load summaries
- user-intent transitions
- failures with enough context to identify endpoint and state

## Risks

### 1. We may still need hosted web handoffs for auth and acquisition

That is acceptable. A partial native shell is still valuable if it removes the unstable browse/detail path from the critical loop.

### 2. The API may expose gaps when consumed directly by a first-party native client

That is also valuable. This is part of why the spike is worth doing.

### 3. Full WebView package stripping may require build-system work

The new UI mode reduces runtime dependency on the main hosted browse WebView, but native-only packaging cleanup may need a later wave.

## Recommended Next Steps

1. Validate the new `NativeCatalogSpike` mode on Board against `staging`.
2. Add device notes for layout, performance, and any API/transport surprises.
3. Move immediately into `GET /catalog/{studioIdentifier}/{titleIdentifier}` if the list-first spike is stable.
4. Keep hosted auth and other text-entry-heavy flows out of scope until Board input support improves.

## Decision Gate

We should keep investing in the native path if the next two endpoint waves demonstrate:

- stable on-device rendering
- predictable BE API consumption
- materially less time spent debugging WebView/renderer behavior

If those conditions hold, the native path is likely a better long-term foundation than continuing to build the main browse experience on top of the hosted WebView stack.
