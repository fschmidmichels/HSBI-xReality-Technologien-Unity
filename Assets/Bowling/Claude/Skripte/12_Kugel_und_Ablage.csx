var sb = new System.Text.StringBuilder();
const float radius = 0.109f;    // Durchmesser 21,8 cm
const float mass = 7f;          // ca. 15 lb
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
var mBall = Mat("Ball", new Color(0x27 / 255f, 0x8C / 255f, 0x35 / 255f), 0.9f);  // Farbe aus dem VRTK-Tutorial
var mHole = Mat("FingerHole", new Color(0.02f, 0.02f, 0.02f), 0.1f);
var mRack = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/Wall.mat");

string pmPath = "Assets/Bowling/PhysicsMaterials/Ball.physicMaterial";
var pBall = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(pmPath);
if (pBall == null) { pBall = new PhysicsMaterial("Ball"); AssetDatabase.CreateAsset(pBall, pmPath); }
pBall.dynamicFriction = 0.2f; pBall.staticFriction = 0.2f; pBall.bounciness = 0.05f;
pBall.frictionCombine = PhysicsMaterialCombine.Average; pBall.bounceCombine = PhysicsMaterialCombine.Average;
EditorUtility.SetDirty(pBall);

// ---------- Kugel ----------
var ball = new GameObject("BowlingBall");
var col = ball.AddComponent<SphereCollider>();
col.radius = radius;
col.sharedMaterial = pBall;
var rb = ball.AddComponent<Rigidbody>();
rb.mass = mass;
rb.interpolation = RigidbodyInterpolation.Interpolate;
rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
visual.name = "Visual";
UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
visual.transform.SetParent(ball.transform, false);
visual.transform.localScale = Vector3.one * radius * 2f;
visual.GetComponent<MeshRenderer>().sharedMaterial = mBall;

// Grifflöcher: zwei Fingerlöcher und ein Daumenloch, ca. 10 cm Spannweite
var holes = new GameObject("FingerHoles").transform;
holes.SetParent(visual.transform.parent, false);
void Hole(string name, float tiltDeg, float sideDeg, float diameter) {
    var dir = Quaternion.Euler(tiltDeg, 0f, sideDeg) * Vector3.up;
    var h = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    h.name = name;
    UnityEngine.Object.DestroyImmediate(h.GetComponent<Collider>());
    h.transform.SetParent(holes, false);
    h.transform.localPosition = dir * (radius - 0.0015f);
    h.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
    h.transform.localScale = new Vector3(diameter, 0.002f, diameter);
    h.GetComponent<MeshRenderer>().sharedMaterial = mHole;
}
Hole("FingerHole_Middle", 20f, -6.5f, 0.022f);
Hole("FingerHole_Ring",   20f,  6.5f, 0.022f);
Hole("ThumbHole",        -32f,  0f,   0.025f);

// ---------- Meta XR Interaction SDK: wie beim Building Block "Grab Interaction" ----------
var grabbable = ball.AddComponent<Oculus.Interaction.Grabbable>();
var so = new SerializedObject(grabbable);
so.FindProperty("_targetTransform").objectReferenceValue = ball.transform;
so.FindProperty("_rigidbody").objectReferenceValue = rb;
so.FindProperty("_kinematicWhileSelected").boolValue = true;
so.FindProperty("_throwWhenUnselected").boolValue = true;
so.ApplyModifiedPropertiesWithoutUndo();

var interaction = new GameObject("GrabInteraction");
interaction.transform.SetParent(ball.transform, false);
var handGrab = interaction.AddComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>();
var soH = new SerializedObject(handGrab);
soH.FindProperty("_pointableElement").objectReferenceValue = grabbable;
soH.FindProperty("_rigidbody").objectReferenceValue = rb;
soH.ApplyModifiedPropertiesWithoutUndo();
var grab = interaction.AddComponent<Oculus.Interaction.GrabInteractable>();
var soG = new SerializedObject(grab);
soG.FindProperty("_pointableElement").objectReferenceValue = grabbable;
soG.FindProperty("_rigidbody").objectReferenceValue = rb;
soG.ApplyModifiedPropertiesWithoutUndo();

var ballPrefab = PrefabUtility.SaveAsPrefabAsset(ball, "Assets/Bowling/Prefabs/BowlingBall.prefab");
UnityEngine.Object.DestroyImmediate(ball);
sb.AppendLine("Prefab: Assets/Bowling/Prefabs/BowlingBall.prefab");

// ---------- Kugelablage neben dem Standplatz ----------
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
foreach (var r in scene.GetRootGameObjects()) if (r.name == "BallRack" || r.name == "BowlingBall") UnityEngine.Object.DestroyImmediate(r);
var rack = new GameObject("BallRack").transform;
rack.position = new Vector3(1.0f, 0f, -3.3f);
GameObject Box(string name, Vector3 min, Vector3 max) {
    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    go.name = name;
    go.transform.SetParent(rack, false);
    go.transform.localPosition = (min + max) / 2f;
    go.transform.localScale = max - min;
    go.GetComponent<MeshRenderer>().sharedMaterial = mRack;
    go.isStatic = true;
    return go;
}
const float top = 0.8f;   // Greifhöhe ohne Bücken
Box("Stand",      new Vector3(-0.08f, -0.005f, -0.08f), new Vector3(0.08f, top - 0.03f, 0.08f));
Box("Tray",       new Vector3(-0.15f, top - 0.03f, -0.3f), new Vector3(0.15f, top, 0.3f));
Box("Rail_Left",  new Vector3(-0.15f, top, -0.3f), new Vector3(-0.12f, top + 0.04f, 0.3f));
Box("Rail_Right", new Vector3(0.12f, top, -0.3f), new Vector3(0.15f, top + 0.04f, 0.3f));
Box("Rail_Front", new Vector3(-0.12f, top, -0.3f), new Vector3(0.12f, top + 0.04f, -0.27f));
Box("Rail_Back",  new Vector3(-0.12f, top, 0.27f), new Vector3(0.12f, top + 0.04f, 0.3f));

var ballInstance = (GameObject)PrefabUtility.InstantiatePrefab(ballPrefab);
// Kugel liegt zwischen den Leisten (Abstand 0,24 m): Mittelpunkt etwas über Tablett, Fingerlöcher nach oben
ballInstance.transform.position = rack.position + new Vector3(0f, top + radius + 0.002f, 0f);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
sb.AppendLine("Kugel und Ablage in " + scene.path);
return sb.ToString();
