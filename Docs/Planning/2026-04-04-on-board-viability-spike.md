# On-Board Viability Spike

## Purpose

Define the fastest proof-of-concept path for validating whether *BE Home* can support the minimum viable on-Board title discovery and install loop.

This spike is intentionally about failing fast on platform constraints before deeper product or UX investment.

## Planning Date

April 4, 2026.

## Core Question

Can a Unity app running on Board:

- read the BE web API for catalog data
- render listings and remote images
- open a purchase/download flow in web content
- locate the downloaded APK afterward
- install that APK in a Board-supported way
- confirm the installed app is reachable from Board UI

## Current Evidence

### Board-documented capabilities

- Board SDK docs explicitly support Unity on Android 13 / API level 33 with ARM64 builds and standard APK deployment through `bdb` from a connected computer.
- Board docs explicitly describe `bdb` as a tool that connects Board to **your computer via USB-C** and installs APKs from there.
- Board docs explicitly state sideloaded apps are launched from `Settings > Sideloaded` on Board OS `1.6.2+`.
- Board FAQ still describes the current program phase as **Phase 1 (Today) - Open Creation + Off-Board Monetization** with **no on-Board discovery**.

### Android-documented constraints relevant to the spike

- Android `WebView` is a normal supported app component, so an in-app browser surface is feasible in principle.
- Accessing files in `Downloads` that the app did **not** create is not straightforward on Android 13; Android recommends the Storage Access Framework for files in `MediaStore.Downloads` that another app created.
- Android package installation initiated from an app generally requires the package-installer flow plus `REQUEST_INSTALL_PACKAGES`, user trust for that source, and user-facing approval. That is Android behavior in general, but Board may further restrict or bypass this through its own sideload model.

## Current Spike Findings

### Confirmed on-device behavior

- Installing a Board APK with `adb` alone is not enough to make it appear in `Settings > Sideloaded`.
- Installing that same APK with installer attribution set to `co.harrishill.developerbridgeservice` **does** make it appear in `Settings > Sideloaded`.
- `co.harrishill.developerbridgeservice` is a privileged system app under `/system/priv-app/BoardDeveloperBridgeService` with broad package, storage, and device permissions.

### Confirmed platform constraints

- Board's `bdb` install path rejects `android.permission.REQUEST_INSTALL_PACKAGES` as a forbidden permission for third-party apps.
- Path-based `pm install ... <apk-path>` from the app process cannot read APK files from either shared `Downloads` or app-owned external storage because the package manager service cannot open those paths under current SELinux and storage rules.
- Streaming APK bytes into `pm install -S ...` from the app process progresses farther than path-based install and avoids the file-read problem.
- Streaming APK bytes into `pm install --user current -S ...` from the app process fails with an `INTERACT_ACROSS_USERS_FULL` permission denial because the package manager treats that request as a cross-user install.
- Streaming APK bytes into `pm install --user 0 -S ...` from the app process gets past file-access and user-targeting issues but fails because `co.harrishill.developerbridgeservice` does not belong to the BE Home app UID, so the app cannot claim Board's installer identity.

### Installation spike conclusion

- The app-process install path is considered **closed for now**.
- We have now ruled out the meaningful public-path variants we could responsibly test:
  - standard Android install via `REQUEST_INSTALL_PACKAGES`
  - path-based `pm install`
  - streamed `pm install` with `--user current`
  - streamed `pm install` with `--user 0`
- Under current known constraints, a normal third-party Board app cannot install another APK in a way that Board recognizes as a sideloaded app.
- Reopening this spike now depends on Board confirming a supported handoff into their privileged installer bridge or another supported on-device install mechanism.

## Immediate Concerns

### 1. `bdb` appears to be host-side, not on-device

This is the biggest concern.

As of the Board Docs `v3.3.0`, the only documented `bdb` flow is:

- connect Board to a computer over USB-C
- run `bdb status`
- run `bdb install path/to/app.apk`

There is no documented Board API, Unity SDK API, Android intent, or on-device shell integration that allows an app on Board itself to invoke `bdb install`.

Working assumption until disproven:

- **on-device self-install via `bdb` should be treated as unsupported / unproven**

This is the first hard viability gate.

## 2. Launch target is probably not the Library app today

Board's current docs say sideloaded apps are accessible through:

- `Settings > Sideloaded`

not through the Library app.

So the success criterion should currently be:

- installed APK appears in `Settings > Sideloaded` and launches successfully there

not:

- installed APK appears in the first-party Library app

If the requirement is specifically "must surface in Library," the current docs argue against that being available for sideloaded apps today.

### 3. Download-file access is likely possible, but not by raw path assumptions

The risky part is not "can Android read files?" but "can our app directly enumerate wherever another app saved an APK?"

On Android 13:

- app-owned files are straightforward
- files another app placed in `Downloads` likely require Storage Access Framework or another user-mediated flow

So the spike should avoid assuming a raw filesystem path like `/sdcard/Download/foo.apk` will just work.

### 4. Web purchase flows may be fragile inside WebView

A WebView itself is feasible, but payment/download pages may depend on:

- popups
- redirects
- cookies
- external auth
- downloadable attachments

That means "Web content loads" is not enough. We need a controlled end-to-end test page plus at least one realistic external provider page.

### 5. `WebView Shell` should not be treated as the product browser strategy

Chromium's own documentation describes System WebView Shell as a thin test app for exercising WebView functionality and explicitly says it is **not a production quality browser** and should not be used as the basis for a browser.

Working assumption:

- if we need browser capability, we should prefer an embedded WebView inside *BE Home* over relying on `WebView Shell` as a product dependency

## Recommended Fast-Fail Experiment Order

## Spike 1: API Read + Image Render

Goal:

- fetch public title listings from BE API
- render at least one list view and one detail view
- render remote card/hero/logo imagery

Why first:

- this is low risk and validates networking, TLS, JSON parsing, image loading, and touch/UI layout on Board

Success:

- app launches on Board
- public catalog payload loads
- multiple remote images render reliably

## Spike 2: Embedded WebView Purchase Flow

Goal:

- open a BE-controlled test page inside an in-app WebView
- verify navigation, redirects, cookies, back handling, and download trigger behavior

Then:

- repeat with a realistic third-party purchase/download page such as itch.io or a minimal test host that behaves similarly

Success:

- user can move from title details to the web purchase flow without leaving the app
- at least one real-world style page works without breaking input or navigation

Current implementation path:

- Use the free, open-source [`gree/unity-webview`](https://github.com/gree/unity-webview) package.
- Use the `package-nofragment` variant because this spike does not yet need HTML file-input support.
- Render `https://staging.boardenthusiasts.com/browse` inside the BE Home content area while keeping BE Home's own header chrome visible.
- Treat this as a native-overlay spike first: the primary proof points are page render, touch alignment, scroll behavior, and whether links can be followed comfortably on Board hardware.

Known caveats to watch during the spike:

- The package renders as a native overlay above Unity content, so Unity UI inside the same rectangle will not appear on top of it.
- If purchase flows later require HTML file-input fields, the `nofragment` variant may need to be revisited.
- Keyboard, focus, and popup behavior still need real-device validation on Board hardware even if the initial page render succeeds.

Implementation note from hardware iteration:

- The footer/back-navigation layout behaved best when kept very simple in UI Toolkit: apply `row` directly on the footer container, use `justify-content: space-between`, place the back button as a direct child on the left, keep the status label as the middle child, and use a matching `80px x 80px` spacer on the right so the status text remains visually centered.
- Matching the footer background to `var(--color_header-background)` made the WebView shell feel integrated with the rest of the BE chrome.

## Spike 3: Download Discovery

Goal:

- determine where downloaded APK files actually land on Board
- prove the app can locate a chosen downloaded APK after the download completes

Recommended approach:

- test both a direct browser/WebView-triggered download and a BE-controlled download endpoint
- prefer user-mediated file selection / SAF if direct enumeration is blocked

Success:

- we can reliably obtain a URI or handle to the downloaded APK that the app can later pass to an installer flow if one exists

## Spike 4: Installation Path Viability

Goal:

- determine whether there is any **supported on-device** install path for a downloaded APK

Pass conditions:

- Board exposes a supported API, intent, command bridge, or documented workflow callable from the app
- or Board confirms a supported pattern that does not require a PC-hosted `bdb`

Fail conditions:

- only PC-hosted `bdb install` is supported
- Android package-installer flow is blocked, unusable, or does not result in a valid Board-side install state

This is the decisive spike. If it fails, the current on-Board distribution concept is likely not viable in the intended form.

Current status:

- standard Android unknown-source install is a dead end because `REQUEST_INSTALL_PACKAGES` is forbidden by `bdb`
- Board-recognized installer provenance through `co.harrishill.developerbridgeservice` is proven relevant
- the remaining app-process `pm install` variants have now also been ruled out
- this spike is closed pending vendor guidance

## Spike 5: Installed-App Verification

Goal:

- verify where successfully installed test APKs appear in Board UI
- verify they can be launched from the device UI

Success:

- APK appears under `Settings > Sideloaded`
- APK launches successfully from Board UI

## Decision Gates

### Green

- embedded WebView works
- download discovery works
- Board-supported on-device install path exists

### Yellow

- WebView works only for controlled pages, not real storefront flows
- downloaded APK can be found only through a picker-based UX
- install requires a very manual user flow but is still Board-supported

### Red

- no Board-supported on-device install path exists
- sideloaded content cannot be installed without a PC-hosted `bdb`
- storefront/payment flows cannot complete in a usable way on Board

## Recommended Near-Term Product Framing

Until Spike 4 proves otherwise, frame the concept as:

- **on-Board browse and purchase-assist proof of concept**

not:

- **on-Board browse, purchase, and install**

That keeps us honest about the biggest unresolved platform dependency.

## Suggested Next Actions

1. Implement an embedded in-app WebView spike using a BE-controlled test page first, then repeat with a realistic external purchase/download page.
2. Implement the smallest possible Unity screen that calls the public catalog API and renders title cards plus remote images.
3. On physical Board hardware, prove the remaining download path by triggering a browser or WebView download and confirming the app can later locate and read the downloaded APK through a supported access path.
4. Wait for Board's response on whether any supported handoff into their privileged installer bridge exists.
5. If Board confirms there is no supported install handoff, treat on-device install as blocked for the current platform phase and continue with browse or purchase-assist-only evaluation.

## Sources

- [Board Docs - Build & Deploy](https://docs.dev.board.fun/getting-started/deploy)
- [Board Docs - Setup Reference](https://docs.dev.board.fun/getting-started/setup-reference)
- [Board Docs - FAQ](https://docs.dev.board.fun/faq)
- [Android Developers - Build web apps in WebView](https://developer.android.com/develop/ui/views/layout/webapps/webview)
- [Android Developers - Access media files from shared storage](https://developer.android.com/training/data-storage/shared/media)
- [Android Developers - Access documents and other files from shared storage](https://developer.android.com/training/data-storage/shared/documents-files)
- [Android Developers - PackageManager.canRequestPackageInstalls](https://developer.android.com/reference/android/content/pm/PackageManager#canRequestPackageInstalls())
- [Chromium - System WebView Shell](https://chromium.googlesource.com/chromium/src/+/f6190242/android_webview/docs/webview-shell.md)
