// Woche 6 Grafik: gebackenes Licht (Subtractive), Light Probes, Reflection Probe, Render Scale/MSAA für Quest.
// Danach 48_Licht_backen.csx ausführen.
var sb = new System.Text.StringBuilder();
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
var hall = GameObject.Find("BowlingHall");

// 1) URP-Asset: Render Scale und MSAA auf Quest-übliche Werte
var rp = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
var rpSo = new SerializedObject(rp);
rpSo.FindProperty("m_RenderScale").floatValue = 1f;
rpSo.FindProperty("m_MSAA").intValue = 4;
rpSo.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(rp);
sb.AppendLine("URP: renderScale=" + rp.renderScale + " msaa=" + rp.msaaSampleCount + " boxProjection=" + rpSo.FindProperty("m_ReflectionProbeBoxProjection").boolValue);

// 2) Lighting Settings als eigenes Asset neben der Szene
string lsPath = "Assets/Scenes/BowlingVR/BowlingVR_Lighting.lighting";
var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(lsPath);
if (ls == null) { ls = new LightingSettings(); ls.name = "BowlingVR_Lighting"; AssetDatabase.CreateAsset(ls, lsPath); }
ls.bakedGI = true; ls.realtimeGI = false;
ls.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
ls.mixedBakeMode = MixedLightingMode.Subtractive;       // billigster Mixed-Modus, von Meta für Quest empfohlen
ls.lightmapResolution = 16f; ls.lightmapPadding = 4; ls.lightmapMaxSize = 1024;
ls.directionalityMode = LightmapsMode.NonDirectional;   // spart eine Textur pro Lightmap
ls.lightmapCompression = LightmapCompression.NormalQuality;
ls.ao = true; ls.aoMaxDistance = 0.6f; ls.aoExponentIndirect = 1f; ls.aoExponentDirect = 0f;
ls.maxBounces = 3; ls.directSampleCount = 32; ls.indirectSampleCount = 256; ls.environmentSampleCount = 256;
ls.filteringMode = LightingSettings.FilterMode.Auto;
EditorUtility.SetDirty(ls);
Lightmapping.lightingSettings = ls;
RenderSettings.subtractiveShadowColor = new Color(0.22f, 0.22f, 0.30f);  // Farbe der Echtzeitschatten von Pins/Kugel auf gebackenen Flächen

// 3) Bestehende Lichter: Mixed = gebacken für Statisches, Echtzeit für Pins/Kugel
foreach (var name in new[] { "CeilingLight (Directional)", "PinDeckLight", "ApproachLight" }) {
    var l = GameObject.Find(name).GetComponent<Light>();
    Undo.RecordObject(l, "Mixed"); l.lightmapBakeType = LightmapBakeType.Mixed;
    sb.AppendLine(name + " -> Mixed, int=" + l.intensity);
}

// 4) Flächenlichter (nur gebacken) unter den Deckenpaneelen und an den Neonleisten
var oldGroup = hall.transform.Find("BakedLights");
if (oldGroup != null) Undo.DestroyObjectImmediate(oldGroup.gameObject);
var group = new GameObject("BakedLights"); Undo.RegisterCreatedObjectUndo(group, "BakedLights");
group.transform.SetParent(hall.transform, false);
Light Area(string n, Vector3 pos, Quaternion rot, Vector2 size, Color c, float intensity) {
    var go = new GameObject(n); go.transform.SetParent(group.transform, false);
    go.transform.SetPositionAndRotation(pos, rot);
    var l = go.AddComponent<Light>();
    l.type = LightType.Rectangle; l.areaSize = size; l.color = c; l.intensity = intensity;
    l.lightmapBakeType = LightmapBakeType.Baked; l.range = 12f;
    return l;
}
int panels = 0;
foreach (Transform p in hall.transform.Find("CeilingPanels")) {
    if (!p.name.StartsWith("Panel_")) continue;
    // Area Lights strahlen entlang +Z, daher 90° um X -> nach unten
    Area("Area_" + p.name, p.position + Vector3.down * 0.03f, Quaternion.Euler(90f, 0f, 0f), new Vector2(1.2f, 0.4f), new Color(1f, 0.95f, 0.88f), 8f);
    panels++;
}
var neonUnten = hall.transform.Find("MaskingUnit/Neon_Unten");
var neonOben = hall.transform.Find("MaskingUnit/Neon_Oben");
// Neonleisten strahlen Richtung Spieler (-Z)
Area("Area_Neon_Unten", neonUnten.position + Vector3.back * 0.05f, Quaternion.Euler(0f, 180f, 0f), new Vector2(8f, 0.05f), new Color(0.1f, 0.9f, 1f), 4f);
Area("Area_Neon_Oben", neonOben.position + Vector3.back * 0.05f, Quaternion.Euler(0f, 180f, 0f), new Vector2(8f, 0.05f), new Color(1f, 0.2f, 0.8f), 4f);
sb.AppendLine("Area Lights: " + panels + " Paneele + 2 Neon");

// 5) Leuchtende/transparente Hilfsflächen nicht ins Lightmap (Glow-Quads haben keine UV2)
int excluded = 0;
foreach (var r in hall.GetComponentsInChildren<MeshRenderer>(true)) {
    if (r.name == "Glow" || r.name.StartsWith("Neon_")) {
        var f = GameObjectUtility.GetStaticEditorFlags(r.gameObject) & ~StaticEditorFlags.ContributeGI;
        GameObjectUtility.SetStaticEditorFlags(r.gameObject, f);
        r.receiveGI = ReceiveGI.LightProbes; excluded++;
    }
}
// Hinweis: Die Emission-Materialien (CeilingLight, CeilingGlow, Neon*) bleiben unverändert auf "RealtimeEmissive".
// Wird das GI-Flag auf "None" gesetzt, schaltet URP das Keyword _EMISSION ab und die Lampen leuchten nicht mehr.
// Da Realtime GI aus ist, tragen sie ohnehin nichts zum Backen bei; das Licht kommt von den Area Lights.
// Lange Flächen bekommen mehr Lightmap-Auflösung (Würfel-UV2 staucht die Länge)
foreach (var r in GameObject.Find("BowlingLane").GetComponentsInChildren<MeshRenderer>(true))
    if (r.name == "LaneSurface" || r.name.StartsWith("Surround_") || r.name.StartsWith("Gutter_")) { Undo.RecordObject(r, "LM"); r.scaleInLightmap = 2f; }
sb.AppendLine("Ohne Lightmap: " + excluded + " Glow/Neon-Objekte");

// 6) Light Probes für Pins, Kugel und Hände
var oldProbes = hall.transform.Find("LightProbes");
if (oldProbes != null) Undo.DestroyObjectImmediate(oldProbes.gameObject);
var probesGo = new GameObject("LightProbes"); Undo.RegisterCreatedObjectUndo(probesGo, "LightProbes");
probesGo.transform.SetParent(hall.transform, false);
var lpg = probesGo.AddComponent<LightProbeGroup>();
var pts = new System.Collections.Generic.List<Vector3>();
// Bahn: drei Spuren, drei Höhen, alle 2,5 m bis hinter die Pins
foreach (float z in new[] { -0.5f, 2f, 4.5f, 7f, 9.5f, 12f, 14.5f, 16.5f, 17.8f, 18.6f, 19.4f })
    foreach (float x in new[] { -0.9f, 0f, 0.9f })
        foreach (float y in new[] { 0.1f, 0.6f, 1.4f }) pts.Add(new Vector3(x, y, z));
// Spielerbereich: breiteres Raster für Kugelablage und Hände
foreach (float z in new[] { -7f, -5f, -3f, -1.5f })
    foreach (float x in new[] { -3.5f, -1.75f, 0f, 1.75f, 3.5f })
        foreach (float y in new[] { 0.3f, 1.1f, 2.0f }) pts.Add(new Vector3(x, y, z));
// Seitlich der Bahn (Surrounds)
foreach (float z in new[] { 2f, 8f, 14f })
    foreach (float x in new[] { -3f, 3f })
        foreach (float y in new[] { 0.3f, 1.5f }) pts.Add(new Vector3(x, y, z));
lpg.probePositions = pts.ToArray();
sb.AppendLine("Light Probes: " + pts.Count);

// 7) Reflection Probe mit Box Projection über die ganze Halle
var oldRp = hall.transform.Find("ReflectionProbe");
if (oldRp != null) Undo.DestroyObjectImmediate(oldRp.gameObject);
var rpGo = new GameObject("ReflectionProbe"); Undo.RegisterCreatedObjectUndo(rpGo, "ReflectionProbe");
rpGo.transform.SetParent(hall.transform, false);
rpGo.transform.position = new Vector3(0f, 1.2f, 8f);   // Aufnahmepunkt über der Bahnmitte
var probe = rpGo.AddComponent<ReflectionProbe>();
probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
probe.boxProjection = true;
var roomCenter = new Vector3(0f, 1.5f, 6.1f); var roomSize = new Vector3(8.2f, 3.7f, 28.5f);
probe.center = roomCenter - rpGo.transform.position; probe.size = roomSize;
probe.resolution = 256; probe.hdr = true; probe.importance = 1; probe.blendDistance = 0f;
sb.AppendLine("Reflection Probe: Box " + roomSize + ", 256 px");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
return sb.ToString();
