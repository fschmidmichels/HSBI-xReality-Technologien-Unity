var sb = new System.Text.StringBuilder();
const float hallX = 4f, wallT = 0.2f, zBack = -8f, zFront = 20.3f, ceilY = 3.3f;
var lit = Shader.Find("Universal Render Pipeline/Lit");

Material Mat(string name, Color c, float smooth, Color emission) {
    string p = "Assets/Bowling/Materials/" + name + ".mat";
    var m = AssetDatabase.LoadAssetAtPath<Material>(p);
    if (m == null) { m = new Material(lit); AssetDatabase.CreateAsset(m, p); }
    m.SetColor("_BaseColor", c);
    m.SetFloat("_Smoothness", smooth);
    if (emission.maxColorComponent > 0f) {
        // LocalKeyword statt EnableKeyword, sonst setzt die URP-Materialprüfung das Schlüsselwort beim Speichern zurück
        m.SetColor("_EmissionColor", emission);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        m.SetKeyword(new UnityEngine.Rendering.LocalKeyword(m.shader, "_EMISSION"), true);
    } else {
        m.SetKeyword(new UnityEngine.Rendering.LocalKeyword(m.shader, "_EMISSION"), false);
    }
    EditorUtility.SetDirty(m);
    return m;
}
var mWall    = Mat("HallWall",    new Color(0.22f, 0.20f, 0.38f), 0.15f, Color.black);
var mCeiling = Mat("HallCeiling", new Color(0.16f, 0.16f, 0.19f), 0.05f, Color.black);
var mMasking = Mat("Masking",     new Color(0.06f, 0.08f, 0.18f), 0.3f,  Color.black);
var mPanel   = Mat("CeilingLight",Color.white,                   0f,    new Color(1f, 0.96f, 0.88f) * 6f);
var mGlow    = Mat("CeilingGlow", new Color(0.16f, 0.16f, 0.19f), 0.05f, new Color(1f, 0.95f, 0.85f) * 0.8f);   // Lichtschein: Grundfarbe wie Decke, Verlaufstextur aus 28_Deckenlicht_Schein
var mNeonC   = Mat("NeonCyan",    new Color(0.1f, 0.9f, 1f),      0f,    new Color(0.1f, 0.85f, 1f) * 3f);
var mNeonM   = Mat("NeonMagenta", new Color(1f, 0.2f, 0.8f),      0f,    new Color(1f, 0.15f, 0.75f) * 3f);

var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
foreach (var r in scene.GetRootGameObjects()) if (r.name == "BowlingHall") UnityEngine.Object.DestroyImmediate(r);
var hall = new GameObject("BowlingHall").transform;

Transform Group(string name) { var g = new GameObject(name).transform; g.SetParent(hall, false); return g; }
GameObject Box(string name, Transform parent, Vector3 min, Vector3 max, Material mat, bool collider, bool castShadows) {
    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    go.name = name;
    go.transform.SetParent(parent, false);
    go.transform.localPosition = (min + max) / 2f;
    go.transform.localScale = max - min;
    var mr = go.GetComponent<MeshRenderer>();
    mr.sharedMaterial = mat;
    mr.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
    if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<BoxCollider>());
    go.isStatic = true;
    return go;
}

// ---------- Raum: Wände und Decke (werfen keine Schatten, damit das Deckenlicht durchkommt) ----------
var room = Group("Room");
Box("Wall_Left",  room, new Vector3(-hallX - wallT, -0.35f, zBack - wallT), new Vector3(-hallX, ceilY, zFront), mWall, true, false);
Box("Wall_Right", room, new Vector3(hallX, -0.35f, zBack - wallT), new Vector3(hallX + wallT, ceilY, zFront), mWall, true, false);
Box("Wall_Back",  room, new Vector3(-hallX, -0.35f, zBack - wallT), new Vector3(hallX, ceilY, zBack), mWall, true, false);
Box("Ceiling",    room, new Vector3(-hallX - wallT, ceilY, zBack - wallT), new Vector3(hallX + wallT, ceilY + 0.1f, zFront), mCeiling, true, false);

// ---------- Masking Unit über dem Pin-Deck (verdeckt den Pinsetter, nur die Pins bleiben sichtbar) ----------
var masking = Group("MaskingUnit");
const float maskFront = 17.3f, maskBottom = 1.0f;
Box("Masking", masking, new Vector3(-hallX, maskBottom, maskFront), new Vector3(hallX, ceilY, zFront - 0.1f), mMasking, true, false);
Box("Blende_Left",  masking, new Vector3(-hallX, 0.05f, maskFront - 0.05f), new Vector3(-0.812f, maskBottom, maskFront), mMasking, true, false);
Box("Blende_Right", masking, new Vector3(0.812f, 0.05f, maskFront - 0.05f), new Vector3(hallX, maskBottom, maskFront), mMasking, true, false);
Box("Neon_Unten", masking, new Vector3(-hallX, maskBottom, maskFront - 0.03f), new Vector3(hallX, maskBottom + 0.05f, maskFront - 0.01f), mNeonC, false, false);
Box("Neon_Oben",  masking, new Vector3(-hallX, ceilY - 0.3f, maskFront - 0.03f), new Vector3(hallX, ceilY - 0.26f, maskFront - 0.01f), mNeonM, false, false);

// ---------- Leuchtende Deckenpaneele (nur Emission, keine Lichtberechnung) ----------
var panels = Group("CeilingPanels");
void Panel(float x, float z) {
    var p = Box("Panel_" + x.ToString("0.0") + "_" + z.ToString("0.0"), panels, new Vector3(x - 0.6f, ceilY - 0.03f, z - 0.2f), new Vector3(x + 0.6f, ceilY, z + 0.2f), mPanel, false, false);
    // Lichtschein: Quad 2 mm unter der Decke, Vorderseite nach unten (ein flacher Quader hätte leuchtende Seitenkanten)
    var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
    g.name = "Glow";
    UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
    g.transform.SetParent(p.transform, false);
    g.transform.position = new Vector3(x, ceilY - 0.002f, z);
    g.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
    g.transform.localScale = Vector3.one;
    g.transform.localScale = new Vector3(2.0f / p.transform.lossyScale.x, 1.2f / p.transform.lossyScale.z, 1f / p.transform.lossyScale.y);
    var gr = g.GetComponent<MeshRenderer>();
    gr.sharedMaterial = mGlow;
    gr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    g.isStatic = true;
}
foreach (var z in new[] { -6f, -3.5f, -1f }) foreach (var x in new[] { -2.5f, 0f, 2.5f }) Panel(x, z);
foreach (var z in new[] { 2f, 6f, 10f, 14f }) Panel(0f, z);

// ---------- Echte Lichtquellen (ohne Schatten, wenig Last auf der Quest) ----------
var lights = Group("Lights");
Light Spot(string name, Vector3 pos, Vector3 target, float range, float angle, float inner, float intensity) {
    var go = new GameObject(name);
    go.transform.SetParent(lights, false);
    go.transform.position = pos;
    go.transform.LookAt(target);
    var l = go.AddComponent<Light>();
    l.type = LightType.Spot;
    l.range = range; l.spotAngle = angle; l.innerSpotAngle = inner;
    l.intensity = intensity;
    l.color = new Color(1f, 0.95f, 0.86f);
    l.shadows = LightShadows.None;
    return l;
}
Spot("PinDeckLight",  new Vector3(0f, 0.95f, 17.45f), new Vector3(0f, 0.15f, 18.75f), 4f, 80f, 55f, 4f);
Spot("ApproachLight", new Vector3(0f, ceilY - 0.05f, -1.5f), new Vector3(0f, 0f, -1.5f), 5f, 110f, 70f, 2.5f);

// ---------- Allgemeines Deckenlicht und Umgebungslicht für einen Innenraum ----------
var dir = GameObject.Find("CeilingLight (Directional)").GetComponent<Light>();
dir.intensity = 1.2f;
dir.color = new Color(1f, 0.97f, 0.92f);
EditorUtility.SetDirty(dir);
RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
RenderSettings.ambientSkyColor = new Color(0.55f, 0.53f, 0.60f);
RenderSettings.ambientEquatorColor = new Color(0.40f, 0.38f, 0.44f);
RenderSettings.ambientGroundColor = new Color(0.20f, 0.19f, 0.22f);
RenderSettings.reflectionIntensity = 0.3f;   // glänzende Bahn soll nicht den Himmel spiegeln

PrefabUtility.SaveAsPrefabAssetAndConnect(hall.gameObject, "Assets/Bowling/Prefabs/BowlingHall.prefab", InteractionMode.AutomatedAction);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
sb.AppendLine("BowlingHall angelegt: " + hall.GetComponentsInChildren<MeshRenderer>().Length + " Bauteile, " + hall.GetComponentsInChildren<Light>().Length + " Spots");
return sb.ToString();
