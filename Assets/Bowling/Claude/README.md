# Bowlingbahn für Woche 3 – Dokumentation der Arbeiten mit Claude Code

Diese Datei beschreibt Schritt für Schritt, was Claude Code (Anthropic) im Unity-Projekt für Woche 3 gebaut und geändert hat. Die verwendeten Skripte liegen unter [`Skripte/`](Skripte), die Kontrollbilder unter [`Bilder/`](Bilder).

## Arbeitsweise

Claude Code hat den laufenden Unity-Editor über die **Unity CLI** gesteuert (Paket `com.unity.pipeline`). C#-Code wurde dabei direkt im Editor ausgeführt:

```bash
unity command eval_file --file <Pfad zur Datei>   # Datei ausführen
unity command eval --code '<C#-Code>'             # Einzeiler ausführen
```

Die Skripte sind **Eval-Schnipsel**, also C#-Anweisungen ohne Klasse. `UnityEngine` und `UnityEditor` sind dabei bereits eingebunden, `using`-Zeilen sind nicht erlaubt.

Die Dateien haben die Endung **`.csx`**. Als `.cs` würde Unity sie kompilieren, das ergäbe Kompilierfehler und der Editor würde im Safe Mode starten. Zum erneuten Ausführen eine Kopie mit beliebiger Endung außerhalb von `Assets/` anlegen und mit `eval_file` starten.

Jede Datei zeigt den **letzten Stand**, mit dem sie ausgeführt wurde. Einige Werte wurden in späteren Schritten überschrieben. Das ist bei den betroffenen Schritten vermerkt.

## Übersicht der Ergebnisse

| Datei / Objekt | Inhalt |
|---|---|
| `Assets/Scenes/BowlingVR.unity` | Kopie der `SampleScene` mit Bahn, Kugel und Ablage, Passthrough aus |
| `Assets/Bowling/Prefabs/BowlingLane.prefab` | Bahn mit Anlauf, Rinnen, Bumpern, Pit, Trigger-Zonen und 10 Pins |
| `Assets/Bowling/Prefabs/CollidableBowlingPin.prefab` | Pin aus dem VRTK-Modell, echte Maße |
| `Assets/Bowling/Prefabs/BowlingBall.prefab` | Kugel mit Meta XR Interaction SDK (Hand und Controller) |
| `Assets/Bowling/Materials/*.mat` | URP-Lit-Materialien |
| `Assets/Bowling/PhysicsMaterials/*.physicMaterial` | `LaneSurface`, `Bumper`, `Pin`, `Ball` |
| `Assets/Bowling/Scripts/Pin.cs`, `PinGroup.cs` | aus dem VRTK-Tutorial, ohne Zinnia angepasst |
| `Assets/Bowling/LICENSE_VRTK.md` | MIT-Lizenz der VRTK-Inhalte |

## Schritt 1 – Projekt und Szene untersuchen

Skript: [`01_Szene_inspizieren.csx`](Skripte/01_Szene_inspizieren.csx)

- Ermittelt: Unity 6000.3.18f1, Meta XR SDK 207.0.1, OpenXR 1.18, URP 17.3.
- `SampleScene` enthält Camera Rig, Controller- und Hand-Tracking, einen Building-Block-Würfel, Passthrough und keinen Boden.
- Die Mittelkamera hat für Passthrough einen transparenten Hintergrund.

## Schritt 2 – Szene `BowlingVR` und Bahn bauen

Skript: [`02_Bahn_bauen.csx`](Skripte/02_Bahn_bauen.csx)

1. `SampleScene` öffnen und als `Assets/Scenes/BowlingVR.unity` speichern. Die `SampleScene` bleibt unverändert.
2. **Passthrough aus:** Objekt `[BuildingBlock] Passthrough` deaktivieren, `OVRManager.isInsightPassthroughEnabled = false`, Clear Flags von `CenterEyeAnchor` auf *Skybox*.
3. Ordner unter `Assets/Bowling/` anlegen, URP-Lit-Materialien und Physik-Materialien erzeugen.
   - `LaneSurface`: Reibung 0,04, Combine *Minimum*. Das entspricht einer geölten Bahn und ist Variante B gegen Kugeln, die in der Rinne liegen bleiben.
4. Bahn aus Quadern unter `BowlingLane` bauen. Die Foul-Linie liegt bei z = 0, die Bahn läuft in +z.

| Teil | Maße |
|---|---|
| `Lane/LaneSurface` | 1,054 m breit, bis z = 19,156 (Pin-Deck-Ende) |
| `Lane/Arrows` | 7 Zielpfeile auf den Brettern 5–35, 12–15 ft hinter der Foul-Linie |
| `Gutters/Gutter_*` | je 0,235 m breit, Boden 4,8 cm unter der Bahn; `Capping_*` als Abschlussleiste |
| `Bumpers/Bumper_*` | 0,1 m breit, oben bei 0,18 m, z = 0,3–18,9. **`Bumpers` deaktivieren = Bumper aus** |
| `Pit` | Boden bei −0,3 m, `Kickback_*` als Seitenwände ab z = 17,3 |
| `Zones` | Trigger `GutterZone_Left/Right`, `PitZone` ohne Skript (für die Rückholung durch die Studierenden) |
| `Hall` | Boden des Spielbereichs, Flächen neben der Bahn, Rückwand |
| `Pins` | Dreieck, Abstand 0,3048 m, Pin 1 bei z = 18,288 |

5. `BowlingLane` als Prefab speichern.

> **Später geändert:** Anlauf 4,572 m → 3 m und Rig bei z = −3,5 → −1,5 (Schritt 12). Die Capsule-Platzhalter für die Pins wurden in Schritt 7 ersetzt.

![Bahn aus Spielersicht](Bilder/bahn_spielersicht.png)

## Schritt 3 – Kontrolle mit Bildern und Physiktest

Skripte: [`03_Vorschaubilder_rendern.csx`](Skripte/03_Vorschaubilder_rendern.csx), [`04_Physiktest_Wuerfe.csx`](Skripte/04_Physiktest_Wuerfe.csx)

- Eine temporäre Kamera rendert Bilder in den Temp-Ordner.
- Im Physiktest wird mit `Physics.Simulate` eine 7-kg-Kugel geworfen. Danach wird die Szene neu geladen, damit nichts gespeichert wird.
- Ergebnis mit den Platzhalter-Pins: Die Pins stehen stabil, ein gerader Wurf wirft 9 von 10 Pins um, die Bumper lenken die Kugel ab. Ohne Bumper landet die Kugel in der Rinne und rollt in den Pit. Eine langsame Kugel (1 m/s) rollt in der Rinne weiter.

## Schritt 4 – Defektes Pin-Prefab analysieren

Skript: [`05_Pinmodell_vermessen.csx`](Skripte/05_Pinmodell_vermessen.csx)

Das importierte `CollidableBowlingPin.prefab` aus dem VRTK-Projekt hatte drei Fehler:
1. Es verwies auf ein Modell mit fremder GUID.
2. Das referenzierte Material fehlte.
3. `Pin.cs` und `Bumper.cs` benötigten **Zinnia**, und das ist nicht installiert. Deshalb schlug die Kompilierung fehl.

## Schritt 5 – Skripte ohne Zinnia

Die Dateien in `Assets/Bowling/Scripts` wurden direkt bearbeitet:
- `Pin.cs`: `OnCollisionEnter` ersetzt den `CollisionNotifier`. Nur Treffer durch bewegliche Objekte starten die Prüfung. `ApproxEquals` wurde durch eine `Mathf`-Rechnung ersetzt.
- `PinGroup.cs`: `velocity` → `linearVelocity` (Unity 6).
- `Bumper.cs`: angepasst, später vom Nutzer gelöscht.

## Schritt 6 – Pin-Prefab nach VRTK-Anleitung neu aufbauen

Skripte: [`06_Pin_Prefab_aufbauen.csx`](Skripte/06_Pin_Prefab_aufbauen.csx), [`07_Pin_Prefab_pruefen.csx`](Skripte/07_Pin_Prefab_pruefen.csx)

Den Aufbau gibt [Tutorial 01](https://github.com/ExtendRealityLtd/VRTK.Tutorials.VRBowling/tree/master/Documentation/Tutorials/01.MakingAVRBowlingGame) vor: Rotation Z −90, Rigidbody, Capsule Collider entlang der X-Achse, Box Collider als Fuß, Material `BowlingPin` mit Albedo D4D4D4.

Abweichungen für echte Maße:

| | Anleitung | Umsetzung |
|---|---|---|
| Scale Factor | 7.8 (≈ 0,59 m hoch) | 5.016 → 0,381 m hoch |
| Collider | Anleitungswerte | mit Faktor 0,643 skaliert |
| Masse | 0,4 kg | 1,6 kg |

Unity 6 legt bei diesem FBX einen Faktor 100 auf die Transform-Skalierung. Deshalb wird die Root-Skalierung ausdrücklich auf 1 gesetzt, wie es die Anleitung verlangt.

## Schritt 7 – Platzhalter ersetzen und Standfestigkeit verbessern

Skripte: [`08_Platzhalter_ersetzen.csx`](Skripte/08_Platzhalter_ersetzen.csx), [`09_Pin_Ruhetest_debuggen.csx`](Skripte/09_Pin_Ruhetest_debuggen.csx), [`10_Pin_Fuss_und_Schwerpunkt.csx`](Skripte/10_Pin_Fuss_und_Schwerpunkt.csx)

1. In `BowlingLane.prefab` die 10 Capsules durch `CollidableBowlingPin` ersetzen. Der Pivot liegt im Pin-Kopf, deshalb sitzt jeder Pin 0,331 m hoch. `PinGroup` kommt an `Pins`. `Pin_Placeholder.prefab` und `Pin.mat` werden gelöscht.
2. Problem: Ohne Kugel fielen alle Pins um. Der Box-Fuß aus der Anleitung war skaliert nur 1,9 cm breit, der Schwerpunkt lag bei 0,196 m. Damit kippt ein Pin schon ab 2,8°.
3. Lösung: Box-Fuß 5 × 5 cm (wie ein echter Pin), Schwerpunkt manuell 0,145 m über dem Boden (`automaticCenterOfMass = false`).
4. Erneuter Physiktest: Die Pins stehen stabil, ein gerader Wurf wirft 8 von 10 um.

![Pins](Bilder/pins.png)

## Schritt 8 – Bowlingkugel und Ablage

Skripte: [`11_BuildingBlock_Wuerfel_auslesen.csx`](Skripte/11_BuildingBlock_Wuerfel_auslesen.csx), [`12_Kugel_und_Ablage.csx`](Skripte/12_Kugel_und_Ablage.csx), [`13_Kugel_Physiktest.csx`](Skripte/13_Kugel_Physiktest.csx)

1. Den Building-Block-Würfel aus der `SampleScene` auslesen, um den Komponentenaufbau der *Grab Interaction* zu übernehmen.
2. `BowlingBall.prefab` aufbauen:
   - Root: `SphereCollider` r = 0,109, `Rigidbody` 7 kg, `Grabbable` (kinematisch während des Greifens, Wurf beim Loslassen).
   - `Visual`: Sphere mit 0,218 m Durchmesser, Farbe 278C35 wie im VRTK-Tutorial.
   - `FingerHoles`: zwei Fingerlöcher und ein Daumenloch als schwarze Scheiben ohne Collider.
   - `GrabInteraction`: `HandGrabInteractable` und `GrabInteractable` (**Meta XR Interaction SDK**, kein VRTK).
3. `BallRack`: Tablett mit Randleisten in 0,8 m Höhe neben dem Standplatz.
4. Test: Die Kugel bleibt auf der Ablage liegen, ein Wurf wirft 8 von 10 Pins um. Im Play Mode gab es keine Fehler vom Interaction SDK.

![Kugel auf der Ablage](Bilder/kugel_ablage.png)

## Schritt 9 – Hand Grab Pose und Anlauf auf 3 m

Skripte: [`14_Geisterhand_Achsen.csx`](Skripte/14_Geisterhand_Achsen.csx), [`15_HandGrabPose_und_Anlauf.csx`](Skripte/15_HandGrabPose_und_Anlauf.csx), [`16_HandGrabPose_rendern.csx`](Skripte/16_HandGrabPose_rendern.csx)

1. Die Achsen der OpenXR-Geisterhand aus dem SDK auslesen: +Z zeigt zu den Fingern, −Y ist die Handfläche, linke und rechte Hand sind an X gespiegelt.
2. `GrabInteraction/HandGrabPose` anlegen, ohne Fingerstellung (`_usesHandPose = false`). Eine Pose gilt so für beide Hände. Sie wird im `HandGrabInteractable` eingetragen, *Hand Alignment* = *Align On Grab*.
3. `Approach/ApproachSurface` auf z = −3 bis 0 kürzen. Rig, Ablage und Kugel liegen bei z = −1,5.

![Hand Grab Pose](Bilder/handgrabpose_vorn.png)

## Schritt 10 – Griffpunkt für Controller

Skripte: [`17_Live_Controller_Griffpunkt.csx`](Skripte/17_Live_Controller_Griffpunkt.csx), [`18_Controller_Griffpunkt.csx`](Skripte/18_Controller_Griffpunkt.csx)

Mit Controllern ist im *Comprehensive Interaction Rig* der Modus **„Controller and No Hand“** aktiv. Gegriffen wird dort über den `ControllerGrabInteractor` und das `GrabInteractable`, die Hand Grab Pose wirkt also nicht. Damit sich die Kugel auch mit Controllern ausrichtet, braucht sie einen eigenen Griffpunkt:

1. `GrabInteraction/ControllerGrabSource` anlegen und als `_grabSource` im `GrabInteractable` eintragen. Beim Greifen wird dieser Punkt samt Drehung auf den Griffpunkt des Controllers gezogen.
2. Die Live-Messung ergab: Der Griffpunkt des Controllers liegt vorn an der Controllerspitze. Der Abstand wurde deshalb von 0,15 auf 0,10 m verringert (Einzeiler E).

## Schritt 11 – Manuelle Justierung übernehmen

Einzeiler F in [`19_Einzeiler.csx`](Skripte/19_Einzeiler.csx)

Die Werte, die der Nutzer in der Szene eingestellt hat, wurden ins Prefab übernommen und die Szene gespeichert:

| Objekt | Position | Drehung |
|---|---|---|
| `ControllerGrabSource` | (0 / 0,05 / −0,05) | 90° um X |
| `HandGrabPose` | (0 / 0 / −0,06) | 0° |

## Schritt 12 – Quellen und Lizenz

- README des Projekts: Pin-Modell und Skripte aus [VRTK.Tutorials.VRBowling](https://github.com/ExtendRealityLtd/VRTK.Tutorials.VRBowling) mit MIT-Lizenz ergänzt.
- `Assets/Bowling/LICENSE_VRTK.md` mit dem vollständigen Lizenztext angelegt.

## Schritt 13 – Szene zurücksetzen per Menütaste

Neues Skript `Assets/Bowling/Scripts/SceneResetter.cs` auf dem Objekt `SceneResetter` in `BowlingVR`.

- Wird die **Menütaste des linken Controllers dreimal schnell hintereinander** gedrückt, lädt das Skript die aktive Szene neu. Kugel, Pins und Rig stehen danach wieder in der Startlage.
- Abgefragt wird `OVRInput.Button.Start`. Bei Hand-Tracking löst auch die Menü-Geste der linken Hand aus.
- Im Inspector einstellbar: `requiredPresses` (Standard 3) und `maxPressInterval`, die maximale Zeit zwischen zwei Drücken (Standard 0,5 s).
- `ResetScene()` ist öffentlich und lässt sich auch von anderen Stellen aufrufen, z. B. über einen Button.
- Voraussetzung: `BowlingVR` muss in der Build-Liste stehen. Das ist bereits der Fall.
- Test im Play Mode: Kugel verschoben, `Pin_1` deaktiviert, dann `ResetScene()` aufgerufen. Danach lag die Kugel wieder auf der Ablage und `Pin_1` war wieder aktiv.

## Schritt 14 – Bumper umschalten (Y) und Kugel und Pins zurücksetzen (X)

Neues Skript `Assets/Bowling/Scripts/LaneControls.cs` auf dem Objekt `LaneControls` in `BowlingVR`. Die Felder sind mit `BowlingLane/Bumpers`, `BowlingLane/Pins` (`PinGroup`) und `BowlingBall` verknüpft.

| Taste (linker Controller) | `OVRInput` | Methode | Wirkung |
|---|---|---|---|
| **Y** | `Button.Four` | `ToggleBumpers()` | Bumper abwechselnd aus- und wieder einschalten |
| **X** | `Button.Three` | `ResetBallAndPins()` | Kugel zurück auf die Ablage, alle Pins wieder aufstellen |

- Die Startlage der Kugel wird in `Start()` gespeichert. Beim Zurücksetzen werden Geschwindigkeit und Drehung auf null gesetzt.
- Für die Pins wird `PinGroup.ResetPositions()` verwendet.

**Fehlerbehebung in `PinGroup.cs`:** Beim ersten Test blieben nach dem Zurücksetzen 6 Pins liegen. Die Methode hatte nur die Transform-Position gesetzt, und die Rigidbody-Interpolation hat die Pins wieder zurückgezogen. Außerdem wurden ausgeblendete Pins erst nach dem Setzen aktiviert. Jetzt werden Pins zuerst aktiviert und dann Rigidbody und Transform gesetzt.

Test im Play Mode: Kugel geworfen (9 Pins um oder ausgeblendet), dann `ResetBallAndPins()` aufgerufen. Danach lag die Kugel auf der Ablage und alle 10 Pins standen nach 5 s noch. Die Bumper ließen sich mehrfach umschalten.

## Schritt 15 – Sounds: Herkunft prüfen und Pin-Aufnahme zerschneiden

**Herkunft geprüft:** Die erste Datei `BowlingPinsHit.mp3` stammte laut ihren ID3-Metadaten aus der kommerziellen *Sound Ideas Series 1000 General* („Copyright Sound Ideas – All Rights Reserved“). Sie wurde entfernt und durch drei CC0-Sounds von freesound.org ersetzt. Die Quellen stehen in der Projekt-README unter „Externe Assets“.

| Zweck | Datei | Länge |
|---|---|---|
| Kugel trifft Pins | `766871__zusisking__sprtind-int_bowlingstrike_zus_owsfx_hard.wav` | 1,45 s |
| Pin gegen Pin | `151204__bruce-burbank__bowling-pins-stereo.aiff` → `PinHits/PinHit_01–06.wav` | je 0,2 s |
| Kugel rollt (Loop) | `260778__squirmtheverm__rolling-ball-loop.wav` | 5,48 s |

**Pin-Aufnahme zerschneiden:** Skript [`20_Pin_Sound_schneiden.py`](Skripte/20_Pin_Sound_schneiden.py). Es nutzt nur die Python-Standardbibliothek und wird außerhalb von Unity ausgeführt:

```bash
python -I 20_Pin_Sound_schneiden.py <eingabe.aiff> <ausgabeordner>          # nur analysieren
python -I 20_Pin_Sound_schneiden.py <eingabe.aiff> <ausgabeordner> --write  # schneiden
```

- Es liest die AIFF-Datei und bildet die Lautstärkekurve (RMS in 5-ms-Fenstern). Einen Anschlag erkennt es, wenn die Kurve −18 dB unter der Spitze überschreitet. Zwei Anschläge liegen mindestens 150 ms auseinander.
- Gefunden wurden 6 Anschläge bei 0,22 / 0,90 / 1,51 / 2,16 / 2,71 / 3,27 s. Jeder ist nach etwa 0,15 s auf −40 dB abgeklungen.
- Jeder Schnitt ist 0,2 s lang: 5 ms Vorlauf und 60 ms Ausblendung am Ende.
- **Hinweis:** `PinHit_03` bis `PinHit_05` enthalten nach etwa 0,08 s einen leisen zweiten Kontakt (−30 dB). Die Pins prallen dort kurz zweimal aufeinander. `PinHit_01`, `02` und `06` sind einzelne Anschläge.

## Schritt 16 – Sounds einbauen

Neue Skripte in `Assets/Bowling/Scripts`. Eingerichtet wurden sie mit [`21_Audio_einrichten.csx`](Skripte/21_Audio_einrichten.csx).

**`BallAudio.cs`** an `BowlingBall.prefab`, mit zwei `AudioSource`s:
- **Rollen:** Der Roll-Loop läuft dauerhaft mit Lautstärke 0. Hat die Kugel Bodenkontakt (Kollision ohne Rigidbody in den letzten 0,1 s) und wird nicht gehalten, steigen Lautstärke und Tonhöhe mit der horizontalen Geschwindigkeit: volle Lautstärke 0,8 bei 8 m/s, Tonhöhe 0,9–1,4 (anfangs 0,7–1,2, klang zu tief). Die Lautstärke wird weich nachgeführt, damit es nicht knackt.
- **Strike:** Beim ersten Kontakt mit einem Pin spielt einmal der Strike-Sound. Danach folgt eine Sperre von 2 s, damit er nicht für jeden Pin erneut ertönt.

**`PinAudio.cs`** an `CollidableBowlingPin.prefab`, mit einer `AudioSource`:
- Bei Kollision mit einem anderen Pin oder dem Boden spielt ein zufälliger `PinHit_01–06` mit leicht variierter Tonhöhe (±8 %). Die Lautstärke hängt von der Aufprallgeschwindigkeit ab (voll bei 4 m/s).
- Unter 0,3 m/s bleibt es still. So entstehen beim Aufstellen und beim Zurücksetzen keine Geräusche.
- Treffer durch die Kugel ignoriert das Skript, die übernimmt `BallAudio`.

Alle Quellen sind 3D-Sounds mit logarithmischem Abfall und ohne Doppler-Effekt. Min. Distance: Rollen 1,5 m, Strike 4 m, Pins 3 m. Damit ist der Treffer auch vom 18 m entfernten Anlauf gut zu hören.

**Test:** [`22_Audio_Test_Rekorder.csx`](Skripte/22_Audio_Test_Rekorder.csx). Weil CLI-Abfragen zu langsam für einen Wurf sind, hängt sich ein Rekorder an `EditorApplication.update`. Er zeichnet 6 s lang pro Frame auf und legt das Ergebnis in `SessionState` ab.

| Prüfpunkt | Ergebnis |
|---|---|
| Szenenstart | kein Pin-Sound, kein Rollen auf der Ablage |
| Wurf mit 7 m/s | Rollen bis Lautstärke 0,69 und Tonhöhe 1,14 |
| Treffer | Strike nach 2,74 s, Pin-Klackern ab 2,82 s von 4 Pins |
| Kugel im Pit | Rollen wieder bei 0 |

## Schritt 17 – Pixelige Schatten verbessern

Skripte: [`23_Schatten_auslesen.csx`](Skripte/23_Schatten_auslesen.csx), [`24_Schatten_Vergleichsbilder.csx`](Skripte/24_Schatten_Vergleichsbilder.csx), [`25_Schatten_verbessern.csx`](Skripte/25_Schatten_verbessern.csx)

**Ursache:** Die Shadow Map des Directional Light wurde über eine Shadow Distance von 50 m gespannt. Die Bahn ist aber nur etwa 28 m lang. Bei *Mobile* kamen dazu nur 1024 px, eine Cascade und keine Soft Shadows. Damit entfielen grob 5 cm oder mehr auf ein Shadow-Map-Pixel. Das flache Licht (50°) hat die Schatten zusätzlich in die Länge gezogen.

**Änderungen:**

| Einstellung | PC vorher → nachher | Mobile vorher → nachher |
|---|---|---|
| Shadow Distance | 50 → **25 m** | 50 → **25 m** |
| Cascades | 4 (unverändert) | 1 → **2** (Split bei 25 %, erste Cascade bis ca. 6 m) |
| Soft Shadows | an, High (unverändert) | aus → **an, Low** |
| Auflösung | 2048 → **4096** (in einem zweiten Schritt) | 1024 (unverändert) |

Das `Directional Light` steht jetzt auf **70°** statt 50° Neigung, die Richtung (Y = 330°) ist gleich geblieben.

Geänderte Dateien: `Assets/Settings/PC_RPAsset.asset`, `Assets/Settings/Mobile_RPAsset.asset` und die Szene `BowlingVR` (Licht).

**Vergleich Mobile** (Kugel auf der Ablage, vorher / nachher):

![vorher](Bilder/schatten_vorher_Mobile_ablage.png)
![nachher](Bilder/schatten_nachher_Mobile_ablage.png)

**Weitere Möglichkeiten, falls die Schatten noch nicht reichen:**
- Die Auflösung bei *Mobile* auf 2048 erhöhen. Das kostet auf der Quest GPU-Leistung und Speicher. Bei *PC* sind die Kanten mit 4096 glatt (Bild `Bilder/schatten_4096_PC_ablage.png`).
- Licht vorberechnen (Baked Lighting), sodass nur Kugel und Pins Echtzeitschatten werfen.

## Schritt 18 – Indoor-Halle mit Beleuchtung

Skripte: [`26_Halle_bauen.csx`](Skripte/26_Halle_bauen.csx), [`27_Halle_Vorschaubilder.csx`](Skripte/27_Halle_Vorschaubilder.csx)

Neues Objekt `BowlingHall` in `BowlingVR`, als Prefab gespeichert unter `Assets/Bowling/Prefabs/BowlingHall.prefab`:

| Gruppe | Inhalt |
|---|---|
| `Room` | Seitenwände bei x = ±4 m, Rückwand hinter dem Spieler bei z = −8 m, Decke in 3,3 m Höhe, alle mit Collider |
| `MaskingUnit` | Verkleidung des Pinsetters über dem Pin-Deck ab z = 17,3 m, Unterkante 1,0 m, dazu seitliche Blenden. Nur die Öffnung über den Pins bleibt frei, wie in einer echten Halle |
| `MaskingUnit/Neon_*` | leuchtende Streifen in Cyan und Magenta an der Front der Masking Unit |
| `CeilingPanels` | 13 leuchtende Deckenpaneele: 9 über dem Spielbereich, 4 über der Bahn |
| `Lights/PinDeckLight` | Spot aus der Masking Unit auf die Pins (Reichweite 4 m, 80°, Stärke 4) |
| `Lights/ApproachLight` | Downlight über dem Anlauf (Reichweite 5 m, 110°, Stärke 2,5) |

**Licht für die Quest:**
- Echte Lichtquellen sind nur das bestehende Directional Light (jetzt Stärke 1,2, leicht warm, als allgemeines Deckenlicht mit Schatten) und die zwei Spots ohne Schatten. Das Mobile-Profil erlaubt 4 Zusatzlichter pro Objekt per Pixel.
- Deckenpaneele und Neonstreifen leuchten nur über Emission. Das kostet keine Lichtberechnung. Auf dem PC lässt der vorhandene Bloom-Effekt sie zusätzlich glühen.
- Wände, Decke und Masking Unit werfen keine Schatten. Sonst würden sie das Directional Light blockieren, das von außen durch die Decke scheint.

**Umgebungslicht** (Lighting-Einstellungen der Szene): von *Skybox* auf *Trilight* umgestellt (Himmel 0,55/0,53/0,60, Horizont 0,40/0,38/0,44, Boden 0,20/0,19/0,22). Die Reflexionsstärke steht auf 0,3, damit die glänzende Bahn nicht den Himmel spiegelt.

**Stolperstein Emission:** `Material.EnableKeyword("_EMISSION")` wurde von der URP-Materialprüfung beim Speichern wieder entfernt, die Paneele blieben grau. Abhilfe: `SetKeyword(new LocalKeyword(shader, "_EMISSION"), true)` zusammen mit `globalIlluminationFlags = RealtimeEmissive`.

![Halle aus Spielersicht](Bilder/halle_PC_spieler.png)
![Pins mit Masking Unit](Bilder/halle_PC_pins.png)

## Schritt 19 – Directional Light als Deckenlicht

Das Directional Light bleibt aktiv. Auf der Quest liefert es als *Main Light* die einzigen Echtzeitschatten, weil Schatten für Zusatzlichter im Mobile-Profil abgeschaltet sind. Ohne diese Schatten wäre es in VR schwerer, Höhe und Abstand von Kugel und Pins einzuschätzen. Außerdem ist es das günstigste Licht in URP.

In der Halle soll es aber wie Deckenleuchten wirken und nicht wie Sonnenlicht:
- **Umbenannt** von `Directional Light` in `CeilingLight (Directional)`.
- **Senkrecht gestellt:** Rotation (90, 0, 0) statt (70, 330, 0). Die Schatten fallen jetzt direkt unter die Objekte und sind kürzer.

[`26_Halle_bauen.csx`](Skripte/26_Halle_bauen.csx) verwendet inzwischen den neuen Namen.

![Schatten senkrecht](Bilder/schatten_senkrecht_PC_ablage.png)

## Schritt 20 – Deckenleuchten heller

Skripte: [`28_Deckenlicht_Schein.csx`](Skripte/28_Deckenlicht_Schein.csx), [`29_Halle_Vorschaubilder_XR.csx`](Skripte/29_Halle_Vorschaubilder_XR.csx), dazu die erweiterte [`26_Halle_bauen.csx`](Skripte/26_Halle_bauen.csx)

- **Emission der Paneele** von 2 auf 6 erhöht, die Grundfarbe ist jetzt reinweiß. Die XR-Kamera (`CenterEyeAnchor`) rendert ohne Post-Processing, die Paneele erscheinen in der Brille also reinweiß. Mit Post-Processing (Szenen- und Game-Ansicht) hat das Neutral-Tonemapping die Stärke 2 vorher zu Grau gedrückt.
- **Lichtschein auf der Decke:** Ohne Lichtschein wirkten die Paneele trotzdem matt, weil sie ihre Umgebung nicht aufhellen. Deshalb hat jedes Paneel jetzt ein Kindobjekt `Glow`, eine 2,0 × 1,2 m große Fläche direkt unter der Decke mit dem Material `CeilingGlow`:
  - Emissionstextur `Assets/Bowling/Textures/CeilingGlow.png` (256 × 160 px, per Skript erzeugt): am Paneel hell, nach außen weich auf null abfallend.
  - Die Grundfarbe entspricht der Decke, der Rand geht dadurch unsichtbar in die Decke über.
  - Das kostet keine Lichtberechnung. Es entsteht nur etwas Überzeichnung (Overdraw) auf kleinen Flächen.
- Die Vorschaubilder werden jetzt wie in der XR-Kamera ohne Post-Processing gerendert.

![Deckenleuchten mit Lichtschein](Bilder/halle_deckenlicht_schein.png)

**Nachbesserung – feine Linie um die Leuchten:** Der Lichtschein war zuerst ein 2 mm flacher Quader. Dessen Seitenflächen bekamen die Verlaufstextur mit der hellen Mitte ab und zeichneten eine dünne leuchtende Kante um jede Leuchte. Jetzt ist der Lichtschein ein **Quad**, dessen einzige Fläche nach unten zeigt ([`30_Deckenlicht_Schein_Quads.csx`](Skripte/30_Deckenlicht_Schein_Quads.csx)). `26_Halle_bauen.csx` erzeugt ihn inzwischen direkt als Quad und setzt Grundfarbe und Emission von `CeilingGlow` passend zur Decke. Danach muss `28_Deckenlicht_Schein.csx` erneut laufen, weil es die Verlaufstextur zuweist. Getestet wurde das mit einem kompletten Neuaufbau der Halle.

![Deckenleuchten nah](Bilder/halle_deckenlicht_nah.png)

## Schritt 21 – Teleport mit dem Building Block

Skript: [`31_Teleport_einrichten.csx`](Skripte/31_Teleport_einrichten.csx)

**Building Block *Teleport*** (Meta XR Interaction SDK, Block-ID `85906e61-…`), zweimal installiert:

| Objekt | Variante | Wirkung |
|---|---|---|
| `Teleport NavMesh` | *NavMesh* | Teleport nur auf das gebackene NavMesh (Bereich *Walkable*), dazu ein Interactable *Non Walkable*, das Teleports verhindert |
| `Teleport Physics Blocker` | *Physics Layer Blocker* | Der Teleport-Bogen endet an Collidern in *Default*, *UI* und *Ignore Raycast*, also an Wänden, Bahn und Ablage |

Die Installation im Building-Blocks-Fenster öffnet einen Dialog zur Auswahl der Variante. Per Skript wird stattdessen die Installationsroutine des Blocks mit gesetzter Variante gewählt und die interne Methode `InstallBlockAsync` aufgerufen. Danach wird die Variante wieder zurückgesetzt. Die Objekte bekommen dabei wie bei einer normalen Installation die Komponente `BuildingBlock`.

**NavMesh nur im Spielbereich:** Am Objekt `Teleport NavMesh` hängt ein Unity-`NavMeshSurface` (AI Navigation) mit *Collect Objects = Volume*. Das Volumen reicht über x −4…4 und z −8…0, die Bahn ab der Foul-Linie bleibt also außen vor. Gebacken wird aus Physik-Collidern. Die Daten liegen in `Assets/Scenes/BowlingVR/NavMesh-Teleport.asset`.
- Problem beim ersten Backen: Der `PlayerController` (Capsule) im Camera Rig stand am Startpunkt und wurde als Hindernis eingebacken. Deshalb fehlte genau der Startpunkt im NavMesh.
- Lösung: `NavMeshModifier` mit *Ignore From Build* am `[BuildingBlock] Camera Rig`.
- Prüfung mit `NavMesh.SamplePosition`: Startpunkt, Anlauf und hinterer Hallenboden sind begehbar. Bahn, Bereich neben der Bahn und Ablage sind es nicht.

**Controller auf Teleport umgestellt:** Laut Hinweis des Blocks war das Rig für Controller auf *Slide* eingestellt. `Locomotor/LocomotionSettings/Moving` → `MovingSetting._controllerMovement` steht jetzt auf **Teleport**. Zur Laufzeit schaltet `MovingSetting` die Teleport-Objekte ein und die Slide-Objekte aus. Im Editor wurden sie passend vorgeschaltet.

**Test im Play Mode:** Der Modus ist *Teleport*, beide `TeleportControllerInteractor` sind aktiv, die Konsole meldet keine Fehler.

**Bedienung:**
- Controller: Thumbstick nach vorn drücken zeigt den Teleport-Bogen, beim Loslassen wird teleportiert. Links und rechts dreht (Turn).
- Hand-Tracking: Microgesture mit dem Daumen über den Zeigefinger.

X, Y und die Menütaste bleiben unverändert für Bumper, Reset und Szenen-Reset.

## Schritt 22 – Kleinere Teleport-Sperrzone um die Kugelablage

Skript: [`32_Teleport_Abstand_Ablage.csx`](Skripte/32_Teleport_Abstand_Ablage.csx)

Die gesperrte Zone um ein Hindernis ergibt sich aus dem **Agent-Radius** des NavMesh. Der Standard-Agent *Humanoid* hatte 0,5 m. Die Ablage (0,3 × 0,6 m) sperrte dadurch eine Fläche von 1,3 × 1,6 m, die bis 0,35 m an die Mitte des Anlaufs reichte.

- Radius des Agenten *Humanoid* auf **0,2 m** gesetzt (Projekteinstellung `ProjectSettings/NavMeshAreas.asset`). Das Projekt nutzt das NavMesh nur für Teleport, andere Agenten sind nicht betroffen.
- NavMesh neu gebacken und `Assets/Scenes/BowlingVR/NavMesh-Teleport.asset` überschrieben.

| Prüfpunkt | Ergebnis |
|---|---|
| Startpunkt | Teleport möglich |
| 0,25 m bzw. 0,4 m neben der Ablage | Teleport möglich (vorher gesperrt) |
| 0,1 m neben der Ablage, Ablage selbst | gesperrt |
| 0,3 m hinter der Ablage | Teleport möglich |
| Bahn | gesperrt |

## Schritt 23 – Kugel ohne Kollision mit dem Spieler

Skript: [`33_Kugel_Layer_ohne_Spieler.csx`](Skripte/33_Kugel_Layer_ohne_Spieler.csx)

**Problem:** Der Spieler hat die Kugel weggeschleudert, und wer „auf“ der Kugel stand, wurde leicht angehoben.

**Ursache:** Im Camera Rig steckt das Objekt `PlayerController` auf dem Layer *Water*. Es hat einen Rigidbody, eine Capsule und den `CharacterController` des Interaction SDK.
- Die Capsule hat die Kugel physikalisch angestoßen.
- Der `CharacterController` sucht den Boden mit der Layer-Maske 23 (*Default*, *TransparentFX*, *Ignore Raycast*, *Water*). Die Kugel lag auf *Default* und ist mit 0,22 m niedriger als die erlaubte Stufenhöhe `_maxStep` = 0,3 m. Der Spieler ist deshalb auf sie „hinaufgestiegen“.

**Lösung, ohne das Rig zu verändern:**
- Neuer Layer **`BowlingBall`** (Layer 8, `ProjectSettings/TagManager.asset`). `BowlingBall.prefab` liegt samt allen Kindobjekten auf diesem Layer.
- Kollision `BowlingBall` ↔ `Water` in der Physik-Kollisionsmatrix abgeschaltet (`ProjectSettings/DynamicsManager.asset`).
- Layer 8 ist nicht in der Bodenmaske des Spielers, die Stufenerkennung ignoriert die Kugel also automatisch.
- Mit allem anderen (Ablage, Bahn, Pins, Wände) kollidiert die Kugel weiterhin.

**Test im Play Mode:** Die Kugel wurde direkt in die Spieler-Capsule gelegt. Nach 3 s lag sie unverändert da (Geschwindigkeit 0), das Rig blieb auf seiner Höhe.

Das Greifen sollte vom neuen Layer nicht betroffen sein, weil die Grab-Interactors nach bisherigem Kenntnisstand die Collider der Kugel direkt prüfen. Das ist aber nicht nachgeprüft, und im Headset wurde das Greifen nach der Änderung noch nicht getestet.

## Schritt 24 – Rinnentiefe nach USBC und Paket für Woche 3

Skripte: [`34_Rinnentiefe_USBC.csx`](Skripte/34_Rinnentiefe_USBC.csx), [`35_Paket_Woche3_exportieren.csx`](Skripte/35_Paket_Woche3_exportieren.csx)

**Rinnentiefe:** Laut *USBC Tiered Center Certification Handbook* (05/2025, S. 3) sind flache Rinnen an der letzten Pin-Reihe 3 3/8–3 5/8 in tief (8,6–9,2 cm). Die Referenzbahn hatte 4,8 cm, das entspricht der Mindesttiefe einer runden Rinne.
- Rinnenboden auf **−0,089 m** abgesenkt. Bumper, Abschlussleisten, Trigger-Zonen `GutterZone_*` und der Bahnkörper reichen entsprechend tiefer.
- Physiktest unverändert bestanden. Eine Kugel liegt in der Rinne jetzt mit ihrer Mitte bei y = 0,02 statt 0,06.

**Paket für die Kursunterlagen:** `XRT_Bowling_Woche3.unitypackage` (ca. 360 KB) wird nach `Kurs/Woche_3/Assets für Unity/` exportiert. Inhalt sind `CollidableBowlingPin`, `BowlingLane` und `BowlingHall` samt Abhängigkeiten unter `Assets/Bowling` (Materialien, Physik-Materialien, Pin-Modell, `Pin.cs`, `PinGroup.cs`, `PinAudio.cs`, Pin-Geräusche, Glow-Textur) sowie `LICENSE_VRTK.md`. Der Ordner `Claude/` ist ausgeschlossen.

## Schritt 25 – Testimpuls aus Woche 3, Block 3

Skript: [`36_TestBall_anlegen.csx`](Skripte/36_TestBall_anlegen.csx)

Damit das Muster-Projekt den Stand nach Block 3 von Woche 3 enthält:
- `Assets/Bowling/Scripts/TestImpulse.cs`: genau der Code aus `Woche_3.md`. Er setzt nach einer Wartezeit `delay` (Standard 3 s, per `Invoke`) die Geschwindigkeit des Rigidbody auf `velocity` (Standard 7 m/s in +z).
- Objekt **`TestBall`** in `BowlingVR`: Sphere mit Scale 0.218 bei (0, 0.11, 0.5), Rigidbody 7 kg (*Interpolate*, *Continuous Dynamic*), Materialien `Ball`, Layer `BowlingBall`, Komponente `TestImpulse`. **Standardmäßig deaktiviert**, damit es bei Würfen im Headset nicht stört. Für einen Test wird es aktiviert und der Play Mode gestartet.
- `TestImpulse.cs` ist nicht im Paket für Woche 3 enthalten, weil die Studierenden das Skript selbst anlegen.

Test im Play Mode: Nach dem Aktivieren rollte die Testkugel in den Pit und warf 5 von 10 Pins um. Die Konsole meldete keine Fehler, und nach dem Play Mode ist `TestBall` wieder deaktiviert.

## Schritt 26 – Höhere Grenze für die Winkelgeschwindigkeit der Kugel

Skript: [`37_BallPhysics_Winkelgeschwindigkeit.csx`](Skripte/37_BallPhysics_Winkelgeschwindigkeit.csx)

Unity begrenzt die Winkelgeschwindigkeit von Rigidbodies standardmäßig auf 50 rad/s. Für reines Rollen braucht die Kugel (r = 0,109 m) bei 8 m/s etwa 73 rad/s.

- Neues Skript `Assets/Bowling/Scripts/BallPhysics.cs`. Das Feld `maxAngularVelocity` (Standard **100** rad/s) ist im Inspector sichtbar und setzt in `Awake()` `Rigidbody.maxAngularVelocity`. Über `OnValidate()` wird es auch während des Play Mode übernommen, weil `maxAngularVelocity` nicht serialisiert wird.
- Angehängt an `BowlingBall.prefab` und an `TestBall` in `BowlingVR`.
- Die projektweite Einstellung *Default Max Angular Speed* bleibt bei 50, damit die Pins nicht betroffen sind.
- Der gleiche Code steht in `Woche_3.md` (Block 4, Bewegungsanalyse).

## Offene Punkte

- Hand-Tracking mit der Hand Grab Pose im Headset testen.
- Für die Rückholung liegen die Trigger-Zonen bereit, ein Skript fehlt noch (Aufgabe für die Studierenden).
- `Pin.cs` blendet umgefallene Pins nach 3 s aus. `PinGroup.ResetPositions()` wird bisher nur über die X-Taste (`LaneControls`) aufgerufen, nicht automatisch nach einem Wurf.
