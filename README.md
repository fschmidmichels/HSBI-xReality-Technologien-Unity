# XRT Projekt

XR-Projekt für die Meta Quest (Android), Hochschule Bielefeld (HSBI), Wintersemester 2026/2027.

| | |
|---|---|
| Produktname | XRT Projekt |
| Package Name (Android) | `com.HSBI.XRTProjekt` |
| Version | 0.1.0 |
| Zielplattform | Android / Meta Quest (Min. API Level 32, Target API Level 34) |

## Unity-Version und Render-Pipeline

- **Unity-Editor:** 6000.3.18f1 (Unity 6.3, Revision `5ebeb53e4c07`)
- **Render-Pipeline:** Universal Render Pipeline (URP) `com.unity.render-pipelines.universal` 17.3.0
  - Qualitätsstufe *Mobile* → `Assets/Settings/Mobile_RPAsset.asset` (Standard für Android/Quest)
  - Qualitätsstufe *PC* → `Assets/Settings/PC_RPAsset.asset` (Standard für Standalone/Editor)

## XR-Pakete und SDK-Versionen

Stand laut Package Manager (`Packages/manifest.json` / `Packages/packages-lock.json`):

| Paket | Version | Hinweis |
|---|---|---|
| `com.meta.xr.sdk.all` (Meta XR All-in-One SDK) | 207.0.1 | direkt installiert |
| ↳ `com.meta.xr.sdk.core` | 207.0.1 | |
| ↳ `com.meta.xr.sdk.interaction` | 207.0.1 | |
| ↳ `com.meta.xr.sdk.interaction.ovr` | 207.0.1 | |
| ↳ `com.meta.xr.sdk.haptics` | 207.0.1 | |
| ↳ `com.meta.xr.mrutilitykit` | 207.0.1 | |
| ↳ `com.meta.xr.sdk.platform` | 207.0.0 | |
| ↳ `com.meta.xr.sdk.audio` | 85.0.0 | |
| ↳ `com.meta.xr.sdk.voice` | 85.0.1 | |
| `com.unity.xr.openxr` (OpenXR Plugin) | 1.18.0 | direkt installiert |
| `com.unity.xr.management` (XR Plug-in Management) | 4.5.4 | Abhängigkeit |
| `com.unity.xr.core-utils` | 2.6.0 | Abhängigkeit |
| `com.unity.xr.hands` | 1.7.3 | Abhängigkeit |
| `com.unity.inputsystem` | 1.19.0 | direkt installiert |

**XR-Konfiguration:** XR Plug-in Management mit **OpenXR**-Loader (Android und Standalone).
Aktivierte OpenXR-Features für Android: *Meta Quest Support*, *Meta XR Feature*, *Meta XR Foveation*, *Meta XR Subsampled Layout*, *Space Warp*;
Interaction Profiles: *Oculus Touch Controller*, *Meta Quest Touch Pro Controller*, *Meta Quest Touch Plus Controller*.

## Öffnen, Bauen und Installieren

### Voraussetzungen

- Unity Hub mit Unity **6000.3.18f1** inkl. Modul **Android Build Support** (OpenJDK, Android SDK & NDK Tools)
- Meta Quest (2/3/3S/Pro) mit aktiviertem **Entwicklermodus** (über die Meta Horizon App)
- USB-C-Datenkabel; optional Meta Quest Developer Hub (MQDH) oder `adb`

### Projekt öffnen

1. Repository klonen bzw. Projektordner kopieren.
2. Unity Hub → **Add → Add project from disk** → Projektordner `XRT Projekt` wählen.
3. Mit Unity 6000.3.18f1 öffnen. Beim ersten Öffnen werden die Pakete heruntergeladen und der `Library`-Ordner aufgebaut (dauert einige Minuten).
4. Falls der Meta XR Project Setup Tool Hinweise anzeigt (**Meta → Tools → Project Setup Tool**), empfohlene Fixes mit *Fix All* übernehmen.
5. Szene `Assets/Scenes/SampleScene.unity` öffnen.

### Bauen

1. **File → Build Profiles** öffnen und Plattform **Android** auswählen → *Switch Platform*.
2. Prüfen, dass unter **Scene List** `Assets/Scenes/SampleScene.unity` aktiviert ist.
3. Unter **Edit → Project Settings → XR Plug-in Management** (Android-Tab) muss **OpenXR** mit Feature-Gruppe **Meta Quest** aktiv sein.
4. Quest per USB verbinden, im Headset das USB-Debugging erlauben.
5. Unter *Run Device* die Quest auswählen und **Build and Run** klicken (oder nur **Build**, um eine `.apk` zu erzeugen).

### Installieren (ohne Build and Run)

APK manuell per `adb` installieren:

```bash
adb install -r XRTProjekt.apk
```

Alternativ die APK in Meta Quest Developer Hub per Drag-and-Drop installieren. Die App erscheint im Headset unter **Bibliothek → Unbekannte Quellen**.

## Externe Assets

| Asset | Quelle | Lizenz |
|---|---|---|
| Meta XR All-in-One SDK (inkl. Core, Interaction, MR Utility Kit, Audio, Voice, Platform, Haptics) | Unity Package Manager / [Meta Developer](https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/) | [Meta Platform Technologies SDK License](https://developers.meta.com/horizon/licenses/oculussdk/) |
| Unity-Pakete (URP, OpenXR, Input System, XR Hands u. a.) | Unity Package Manager (Unity Registry) | [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license) |
| URP-Projektvorlage (`Assets/TutorialInfo`, `Assets/Settings`, `SampleScene`) | Unity Hub, Vorlage *Universal 3D* | Unity Companion License |

Darüber hinaus werden derzeit keine Assets aus dem Unity Asset Store oder anderen Drittquellen verwendet. Neue externe Assets bitte hier mit Quelle und Lizenz ergänzen.

## Eingesetzte KI-Werkzeuge

| Werkzeug | Zweck |
|---|---|
| Claude Code (Anthropic) | Erstellung dieser README auf Basis der Projektdateien (Unity-Version, Paketliste, XR-Einstellungen) |

Weitere eingesetzte KI-Werkzeuge (z. B. für Code, Texturen, 3D-Modelle, Audio) bitte hier mit Zweck ergänzen.
