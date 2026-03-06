# RC DEV — NBMBC iOS App

## Overview
Full-featured iOS app for New Bethel Missionary Baptist Church.
Matrix-style glassmorphism UI, dark gold aesthetic, SwiftUI + iOS 17.

## Features
- **Home** — Hero, animated constellation, stats, ministry cards, sermon preview, give
- **Sermons** — Grid library with play overlay, live stream link
- **Events** — Upcoming events with date badges, detail view, add to calendar
- **Prayer** — Category chips, prayer form, scripture sidebar
- **More** — Groups, Watch Live, About, Location, Give (Cash App)
- **Glassmorphism** — `.ultraThinMaterial` + gold borders + constellation particles
- **Matrix particles** — Deterministic TimelineView + Canvas animation (no UIKit)

## Setup

### Option 1 — XcodeGen (Recommended)
1. Install XcodeGen: `brew install xcodegen`
2. From the `NBMBC_iOS/` directory: `xcodegen generate`
3. Open `NBMBC_iOS.xcodeproj` in Xcode
4. Set your Apple Developer Team in Signing & Capabilities
5. Run on a real device or simulator (iOS 17.0+)

### Option 2 — Manual
1. Create a new Xcode project: iOS App, SwiftUI, Swift
2. Set Bundle ID: `com.rcdev.nbmbc`, Display Name: `RC DEV`
3. Drag all files from `NBMBC_iOS/` into the project
4. Add `MapKit` framework (for LocationView)
5. Build and run

## App Store Submission
1. Set up an App Store Connect listing: "RC DEV - New Bethel MBBC"
2. Category: Lifestyle / Reference
3. App Icon: 1024×1024 — dark background (#070710), gold church logo + "RC DEV" text
4. Screenshots: generate from iPhone 15 Pro simulator
5. Configure Push Notifications entitlement if adding sermon alerts later

## Customization

### Replace mock data with real API
Edit `DataStore.swift` — replace the `loadMockData()` method with real
`URLSession` calls to your backend (ASP.NET Core API).

### Live Stream URL
Edit `LiveView.swift` — update `facebookURL` or add YouTube embed via `WebView`.

### Cash App Tag
Already set to `$blueboy78` in `DataStore.swift` → `ChurchInfo.cashAppTag`.

### Colors
All design tokens are in `Theme/Theme.swift`.
- `Theme.gold` = `#C9A84C`
- `Theme.background` = `#070710`

## Architecture
- **Pattern**: MVVM, SwiftUI native
- **State**: `@EnvironmentObject DataStore`
- **Navigation**: `NavigationStack` + `TabView`
- **Background FX**: `TimelineView` + `Canvas` (no third-party dependencies)
- **Maps**: `MapKit` (native)
- **Web**: `WKWebView` wrapper for sermon video and live stream

## File Structure
```
NBMBC_iOS/
├── App/
│   ├── NBMBCApp.swift          — @main entry point
│   └── ContentView.swift       — TabView + custom tab bar
├── Theme/
│   └── Theme.swift             — Colors, gradients, modifiers
├── Models/
│   ├── Models.swift            — Data models
│   └── DataStore.swift         — Observable store + mock data
├── Components/
│   ├── ConstellationView.swift — Matrix particle animation
│   ├── GoldButton.swift        — GoldButton, GhostButton, EyebrowBadge
│   └── SharedComponents.swift  — EmptyStateView, GridPattern, etc.
├── Views/
│   ├── HomeView.swift          — Full home screen
│   ├── SermonsView.swift       — Sermon grid
│   ├── SermonDetailView.swift  — Sermon detail + video player
│   ├── EventsView.swift        — Events list
│   ├── EventDetailView.swift   — Event detail
│   ├── GroupsView.swift        — Ministry groups
│   ├── LiveView.swift          — Watch live stream
│   ├── PrayerView.swift        — Prayer request form
│   ├── AboutView.swift         — About the church
│   ├── LocationView.swift      — MapKit + directions
│   ├── GiveView.swift          — Cash App giving sheet
│   └── MoreView.swift          — More tab hub
└── Assets.xcassets/
```
