var sb = new System.Text.StringBuilder();
const string modelPath  = "Assets/Bowling/Models/BowlingPin.fbx";
const string prefabPath = "Assets/Bowling/Prefabs/CollidableBowlingPin.prefab";
const float docScaleFactor = 7.8f;   // laut VRTK-Anleitung
const float realHeight = 0.381f;     // echter Pin (15 in)
const float pinMass = 1.6f;

// 1) Modell mit dem Scale Factor aus der Anleitung importieren und Höhe messen
var imp = (ModelImporter)AssetImporter.GetAtPath(modelPath);
imp.globalScale = docScaleFactor;
imp.SaveAndReimport();
var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
var probe = (GameObject)PrefabUtility.InstantiatePrefab(model);
probe.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
var bDoc = probe.GetComponentInChildren<Renderer>().bounds;
UnityEngine.Object.DestroyImmediate(probe);
float importFactor = realHeight / bDoc.size.y;
// Collider-Ausdehnung laut Anleitung: Capsule von -0.0775 bis 0.4975, Box bis 0.515 -> 0.5925 m
float s = realHeight / 0.5925f;
sb.AppendLine("Höhe mit Scale Factor 7.8: " + bDoc.size.y.ToString("F3") + " m, Collider-Faktor " + s.ToString("F4"));

// 2) Auf echte Höhe skalieren
imp.globalScale = docScaleFactor * s;
imp.SaveAndReimport();
model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
sb.AppendLine("Neuer Scale Factor: " + imp.globalScale.ToString("F3"));

// 3) Material laut Anleitung (Albedo D4D4D4)
const string matPath = "Assets/Bowling/Materials/BowlingPin.mat";
var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, matPath); }
mat.SetColor("_BaseColor", new Color(0xD4 / 255f, 0xD4 / 255f, 0xD4 / 255f));
mat.SetFloat("_Smoothness", 0.8f);
EditorUtility.SetDirty(mat);
var pinPhys = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Bowling/PhysicsMaterials/Pin.physicMaterial");

// 4) Pin aufbauen wie in der Anleitung, Collider-Werte mit Faktor s skaliert
var pin = (GameObject)PrefabUtility.InstantiatePrefab(model);
pin.name = "CollidableBowlingPin";
pin.transform.position = Vector3.zero;
pin.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
pin.transform.localScale = Vector3.one;
foreach (var r in pin.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;

var rb = pin.AddComponent<Rigidbody>();
rb.mass = pinMass;
rb.interpolation = RigidbodyInterpolation.Interpolate;
rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

var cap = pin.AddComponent<CapsuleCollider>();
cap.direction = 0; // X-Achse
cap.center = new Vector3(0.21f * s, 0f, 0f);
cap.radius = 0.08f * s;
cap.height = 0.575f * s;
cap.sharedMaterial = pinPhys;

var box = pin.AddComponent<BoxCollider>();
box.center = new Vector3(0.465f * s, 0f, 0f);
box.size = new Vector3(0.1f, 0.03f, 0.03f) * s;
box.sharedMaterial = pinPhys;

pin.AddComponent<Pin>();

// Abgleich Mesh <-> Collider
var mb = pin.GetComponentInChildren<Renderer>().bounds;
Physics.SyncTransforms();
var cb = cap.bounds; cb.Encapsulate(box.bounds);
sb.AppendLine("Mesh   y: " + mb.min.y.ToString("F3") + " .. " + mb.max.y.ToString("F3") + ", Breite " + mb.size.x.ToString("F3"));
sb.AppendLine("Collider y: " + cb.min.y.ToString("F3") + " .. " + cb.max.y.ToString("F3") + ", Breite " + cb.size.x.ToString("F3"));

AssetDatabase.DeleteAsset(prefabPath);
PrefabUtility.SaveAsPrefabAsset(pin, prefabPath);
UnityEngine.Object.DestroyImmediate(pin);
AssetDatabase.SaveAssets();
sb.AppendLine("Prefab gespeichert: " + prefabPath);
return sb.ToString();
