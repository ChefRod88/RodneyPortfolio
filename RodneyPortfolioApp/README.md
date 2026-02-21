# Rodney Chery Portfolio - Native App

Capacitor wrapper for [rodneyachery.com](https://www.rodneyachery.com). The app loads your portfolio website in a native WebView.

## Prerequisites

- **Android:** Android Studio (or Android SDK)
- **iOS:** Xcode (Mac only) + CocoaPods (`sudo gem install cocoapods`)

## Run on Android

```bash
npx cap run android
```

Or open in Android Studio:

```bash
npx cap open android
```

## Run on iOS

First, ensure Xcode is installed and selected:

```bash
sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
```

Then install CocoaPods dependencies (if not done):

```bash
cd ios/App && pod install && cd ../..
```

Run on simulator or device:

```bash
npx cap run ios
```

Or open in Xcode:

```bash
npx cap open ios
```

## Sync Changes

When you update `capacitor.config.ts` or add plugins:

```bash
npx cap sync
```

## Configuration

The app loads `https://www.rodneyachery.com` via `capacitor.config.ts` (`server.url`). Updates to your website appear in the app immediately—no rebuild needed.
