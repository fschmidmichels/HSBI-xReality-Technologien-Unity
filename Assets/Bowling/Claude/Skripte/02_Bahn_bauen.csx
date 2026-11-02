
var log = new System.Text.StringBuilder();

// ---------- Maße (Meter / Kilogramm) ----------
const float laneW      = 1.054f;   // Bahnbreite ohne Rinnen
const float gutterW    = 0.235f;   // Rinnenbreite
const float gutterD    = 0.048f;   // Rinnentiefe (ca. 1 7/8 in)
const float headPinZ   = 18.288f;  // Foul-Linie bis Pin 1 (60 ft)
const float pinSpacing = 0.3048f;  // 12 in
const float pinDeckEnd = 19.156f;  // Pin 1 bis Pit-Kante ca. 34 3/16 in
const float approachL  = 4.572f;   // 15 ft
const float pinH = 0.381f, pinD = 0.121f, pinMass = 1.6f;
const float kickbackStartZ = 17.3f;
const float pitEndZ = 20.2f;
const float pitFloorY = -0.3f;
const float cappingW = 0.05f;
float halfLane = laneW / 2f;
float gutterOuter = halfLane + gutterW;      // 0.762
float outerEdge = gutterOuter + cappingW;    // 0.812

// ---------- Ordner ----------
void EnsureFolder(string parent, string name) {
    if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
}
EnsureFolder("Assets", "Bowling");
EnsureFolder("Assets/Bowling", "Materials");
EnsureFolder("Assets/Bowling", "PhysicsMaterials");
EnsureFolder("Assets/Bowling", "Prefabs");

// ---------- Szene: SampleScene als BowlingVR speichern ----------
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/BowlingVR.unity");
scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
log.AppendLine("Szene: " + scene.path);

// ---------- Passthrough deaktivieren ----------
foreach (var root in scene.GetRootGameObjects()) {
    if (root.name == "[BuildingBlock] Passthrough") { root.SetActive(false); log.AppendLine("Passthrough-Objekt deaktiviert"); }
}
var ovrType = System.Type.GetType("OVRManager, Oculus.VR");
if (ovrType != null) {
    foreach (var m in UnityEngine.Object.FindObjectsByType(ovrType, FindObjectsSortMode.None)) {
        var f = ovrType.GetField("isInsightPassthroughEnabled");
        if (f != null) { f.SetValue(m, false); EditorUtility.SetDirty(m); log.AppendLine("OVRManager.isInsightPassthroughEnabled = false"); }
    }
}
foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) {
    if (cam.name == "CenterEyeAnchor") { cam.clearFlags = CameraClearFlags.Skybox; EditorUtility.SetDirty(cam); log.AppendLine("CenterEyeAnchor: Clear Flags = Skybox"); }
}

// ---------- Rig und Cube in den Anlauf verschieben ----------
foreach (var root in scene.GetRootGameObjects()) {
    if (root.name == "[BuildingBlock] Camera Rig") root.transform.position = new Vector3(0f, 0f, -3.5f);
    if (root.name == "[BuildingBlock] Cube") root.transform.position = new Vector3(0.4f, 1f, -3.25f);
}

// ---------- Materialien ----------
var lit = Shader.Find("Universal Render Pipeline/Lit");
Material Mat(string name, Color c, float smooth) {
    string p = "Assets/Bowling/Materials/" + name + ".mat";
    var m = AssetDatabase.LoadAssetAtPath<Material>(p);
    if (m == null) { m = new Material(lit); AssetDatabase.CreateAsset(m, p); }
    m.SetColor("_BaseColor", c);
    m.SetFloat("_Smoothness", smooth);
    EditorUtility.SetDirty(m);
    return m;
}
var mLane     = Mat("Lane",      new Color(0.86f, 0.70f, 0.48f), 0.85f);
var mApproach = Mat("Approach",  new Color(0.72f, 0.55f, 0.35f), 0.6f);
var mGutter   = Mat("Gutter",    new Color(0.18f, 0.18f, 0.20f), 0.5f);
var mBumper   = Mat("Bumper",    new Color(0.95f, 0.55f, 0.10f), 0.4f);
var mPin      = Mat("Pin",       new Color(0.97f, 0.97f, 0.95f), 0.8f);
var mLine     = Mat("FoulLine",  new Color(0.05f, 0.05f, 0.05f), 0.3f);
var mMarker   = Mat("Marker",    new Color(0.15f, 0.15f, 0.15f), 0.3f);
var mWall     = Mat("Wall",      new Color(0.10f, 0.10f, 0.14f), 0.1f);
var mFloor    = Mat("HallFloor", new Color(0.25f, 0.28f, 0.35f), 0.1f);

// ---------- Physik-Materialien ----------
PhysicsMaterial PMat(string name, float friction, float bounce, PhysicsMaterialCombine fc) {
    string p = "Assets/Bowling/PhysicsMaterials/" + name + ".physicMaterial";
    var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(p);
    if (m == null) { m = new PhysicsMaterial(name); AssetDatabase.CreateAsset(m, p); }
    m.dynamicFriction = friction; m.staticFriction = friction; m.bounciness = bounce;
    m.frictionCombine = fc; m.bounceCombine = PhysicsMaterialCombine.Average;
    EditorUtility.SetDirty(m);
    return m;
}
var pLane   = PMat("LaneSurface", 0.04f, 0f,   PhysicsMaterialCombine.Minimum);
var pBumper = PMat("Bumper",      0.10f, 0.3f, PhysicsMaterialCombine.Minimum);
var pPin    = PMat("Pin",         0.30f, 0.3f, PhysicsMaterialCombine.Average);

// ---------- Hilfsfunktionen ----------
GameObject Box(string name, Transform parent, Vector3 min, Vector3 max, Material mat, PhysicsMaterial pm, bool collider) {
    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    go.name = name;
    go.transform.SetParent(parent, false);
    go.transform.localPosition = (min + max) / 2f;
    go.transform.localScale = max - min;
    go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    var col = go.GetComponent<BoxCollider>();
    if (!collider) UnityEngine.Object.DestroyImmediate(col);
    else if (pm != null) col.sharedMaterial = pm;
    go.isStatic = true;
    return go;
}
GameObject Solid(string name, Transform parent, Vector3 min, Vector3 max, Material mat, PhysicsMaterial pm = null) => Box(name, parent, min, max, mat, pm, true);
GameObject Decal(string name, Transform parent, Vector3 min, Vector3 max, Material mat) => Box(name, parent, min, max, mat, null, false);
GameObject Trigger(string name, Transform parent, Vector3 min, Vector3 max) {
    var go = new GameObject(name);
    go.transform.SetParent(parent, false);
    go.transform.localPosition = (min + max) / 2f;
    var col = go.AddComponent<BoxCollider>();
    col.size = max - min;
    col.isTrigger = true;
    return go;
}
Transform Group(string name, Transform parent) {
    var go = new GameObject(name);
    go.transform.SetParent(parent, false);
    return go.transform;
}

// ---------- Alte Bahn entfernen (Skript ist wiederholbar) ----------
foreach (var root in scene.GetRootGameObjects())
    if (root.name == "BowlingLane") UnityEngine.Object.DestroyImmediate(root);

var lane = new GameObject("BowlingLane").transform;

// Halle
var hall = Group("Hall", lane);
Solid("PlayerAreaFloor", hall, new Vector3(-4f, -0.35f, -8f), new Vector3(4f, -0.005f, 0f), mFloor);
Solid("Surround_Left",  hall, new Vector3(-4f, -0.35f, 0f), new Vector3(-outerEdge, 0.05f, pitEndZ), mFloor);
Solid("Surround_Right", hall, new Vector3(outerEdge, -0.35f, 0f), new Vector3(4f, 0.05f, pitEndZ), mFloor);
Solid("BackWall", hall, new Vector3(-4f, -0.35f, pitEndZ), new Vector3(4f, 2.5f, pitEndZ + 0.1f), mWall);

// Anlauf und Foul-Linie
var approach = Group("Approach", lane);
Solid("ApproachSurface", approach, new Vector3(-outerEdge, -0.1f, -approachL), new Vector3(outerEdge, 0f, 0f), mApproach, pLane);
Decal("FoulLine", approach, new Vector3(-outerEdge, 0f, -0.0125f), new Vector3(outerEdge, 0.001f, 0.0125f), mLine);

// Bahn
var surface = Group("Lane", lane);
Solid("LaneSurface", surface, new Vector3(-halfLane, -0.1f, 0f), new Vector3(halfLane, 0f, pinDeckEnd), mLane, pLane);

// Zielpfeile auf den Brettern 5, 10, ..., 35 (Brettbreite = Bahnbreite / 39), 12-15 ft, mittlerer Pfeil am weitesten
var arrows = Group("Arrows", surface);
float board = laneW / 39f;
int[] boards = { 5, 10, 15, 20, 25, 30, 35 };
for (int i = 0; i < boards.Length; i++) {
    int k = 3 - Mathf.Abs(i - 3);
    float x = halfLane - (boards[i] - 0.5f) * board;
    float z = 3.658f + k * 0.3048f;
    Decal("Arrow_Board" + boards[i], arrows, new Vector3(x - 0.012f, 0f, z - 0.08f), new Vector3(x + 0.012f, 0.001f, z + 0.08f), mMarker);
}

// Rinnen: Boden tiefer als die Bahn, außen eine Abschlussleiste
var gutters = Group("Gutters", lane);
foreach (var side in new[] { -1f, 1f }) {
    string s = side < 0 ? "Left" : "Right";
    float a = side * halfLane, b = side * gutterOuter, c = side * outerEdge;
    Solid("Gutter_" + s, gutters, new Vector3(Mathf.Min(a, b), -0.1f, 0f), new Vector3(Mathf.Max(a, b), -gutterD, pinDeckEnd), mGutter, pLane);
    Solid("Capping_" + s, gutters, new Vector3(Mathf.Min(b, c), -0.1f, 0f), new Vector3(Mathf.Max(b, c), 0.05f, kickbackStartZ), mWall);
}

// Bumper über den Rinnen (Objekt "Bumpers" deaktivieren = Bumper aus)
var bumpers = Group("Bumpers", lane);
foreach (var side in new[] { -1f, 1f }) {
    string s = side < 0 ? "Left" : "Right";
    float a = side * halfLane, b = side * (halfLane + 0.1f);
    Solid("Bumper_" + s, bumpers, new Vector3(Mathf.Min(a, b), -gutterD, 0.3f), new Vector3(Mathf.Max(a, b), 0.18f, 18.9f), mBumper, pBumper);
}

// Pit und Kickbacks
var pit = Group("Pit", lane);
Solid("PitFloor", pit, new Vector3(-gutterOuter, pitFloorY - 0.05f, pinDeckEnd), new Vector3(gutterOuter, pitFloorY, pitEndZ), mWall);
foreach (var side in new[] { -1f, 1f }) {
    string s = side < 0 ? "Left" : "Right";
    float b = side * gutterOuter, c = side * outerEdge;
    Solid("Kickback_" + s, pit, new Vector3(Mathf.Min(b, c), pitFloorY, kickbackStartZ), new Vector3(Mathf.Max(b, c), 0.8f, pitEndZ), mWall);
}

// Trigger-Zonen (ohne Skript, für die Rückholung durch die Studierenden)
var zones = Group("Zones", lane);
Trigger("GutterZone_Left",  zones, new Vector3(-gutterOuter, -gutterD, 0f), new Vector3(-halfLane, 0.25f, pinDeckEnd));
Trigger("GutterZone_Right", zones, new Vector3(halfLane, -gutterD, 0f), new Vector3(gutterOuter, 0.25f, pinDeckEnd));
Trigger("PitZone",          zones, new Vector3(-gutterOuter, pitFloorY, pinDeckEnd), new Vector3(gutterOuter, 0.5f, pitEndZ));

// ---------- Pin-Platzhalter als Prefab ----------
var pinTemplate = GameObject.CreatePrimitive(PrimitiveType.Capsule);
pinTemplate.name = "Pin_Placeholder";
pinTemplate.transform.localScale = new Vector3(pinD, pinH / 2f, pinD);
pinTemplate.GetComponent<MeshRenderer>().sharedMaterial = mPin;
pinTemplate.GetComponent<CapsuleCollider>().sharedMaterial = pPin;
var rb = pinTemplate.AddComponent<Rigidbody>();
rb.mass = pinMass;
rb.interpolation = RigidbodyInterpolation.Interpolate;
rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
var pinPrefab = PrefabUtility.SaveAsPrefabAsset(pinTemplate, "Assets/Bowling/Prefabs/Pin_Placeholder.prefab");
UnityEngine.Object.DestroyImmediate(pinTemplate);

var pins = Group("Pins", lane);
float rowStep = pinSpacing * Mathf.Sqrt(3f) / 2f;   // 0,264 m
int n = 1;
for (int row = 0; row < 4; row++) {
    for (int i = 0; i <= row; i++) {
        float x = (i - row / 2f) * pinSpacing;
        var p = (GameObject)PrefabUtility.InstantiatePrefab(pinPrefab);
        p.name = "Pin_" + n++;
        p.transform.SetParent(pins, false);
        p.transform.localPosition = new Vector3(x, pinH / 2f, headPinZ + row * rowStep);
    }
}

// ---------- Bahn als Prefab speichern ----------
PrefabUtility.SaveAsPrefabAssetAndConnect(lane.gameObject, "Assets/Bowling/Prefabs/BowlingLane.prefab", InteractionMode.AutomatedAction);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
log.AppendLine("Fertig. Pins: " + (n - 1));
return log.ToString();
