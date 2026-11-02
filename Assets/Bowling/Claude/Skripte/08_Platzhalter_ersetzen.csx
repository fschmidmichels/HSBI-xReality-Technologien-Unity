var sb = new System.Text.StringBuilder();
const string lanePath = "Assets/Bowling/Prefabs/BowlingLane.prefab";
var pinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bowling/Prefabs/CollidableBowlingPin.prefab");

// Abstand Pivot -> Fußpunkt (Pivot liegt beim Modell oben im Kopf)
var probe = (GameObject)PrefabUtility.InstantiatePrefab(pinPrefab);
Physics.SyncTransforms();
var cb = probe.GetComponent<CapsuleCollider>().bounds; cb.Encapsulate(probe.GetComponent<BoxCollider>().bounds);
float pivotHeight = -cb.min.y;
var rot = probe.transform.rotation;
UnityEngine.Object.DestroyImmediate(probe);
sb.AppendLine("Pivot-Höhe über Boden: " + pivotHeight.ToString("F4"));

var root = PrefabUtility.LoadPrefabContents(lanePath);
var pins = root.transform.Find("Pins");
var positions = new System.Collections.Generic.List<(string, Vector3)>();
for (int i = pins.childCount - 1; i >= 0; i--) {
    var c = pins.GetChild(i);
    positions.Insert(0, (c.name, c.localPosition));
    UnityEngine.Object.DestroyImmediate(c.gameObject);
}
foreach (var (name, pos) in positions) {
    var p = (GameObject)PrefabUtility.InstantiatePrefab(pinPrefab, pins);
    p.name = name;
    p.transform.localPosition = new Vector3(pos.x, pivotHeight + 0.001f, pos.z);
    p.transform.localRotation = rot;
}
if (pins.GetComponent<PinGroup>() == null) pins.gameObject.AddComponent<PinGroup>();
PrefabUtility.SaveAsPrefabAsset(root, lanePath);
PrefabUtility.UnloadPrefabContents(root);
sb.AppendLine("Ersetzt: " + positions.Count + " Pins, PinGroup an 'Pins'");

AssetDatabase.DeleteAsset("Assets/Bowling/Prefabs/Pin_Placeholder.prefab");
AssetDatabase.DeleteAsset("Assets/Bowling/Materials/Pin.mat");
AssetDatabase.SaveAssets();

var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
var scenePins = GameObject.Find("BowlingLane/Pins");
sb.AppendLine("In der Szene: " + scenePins.transform.childCount + " Pins, erster = " + scenePins.transform.GetChild(0).name + " (" + PrefabUtility.GetCorrespondingObjectFromSource(scenePins.transform.GetChild(0).gameObject)?.name + ")");
return sb.ToString();
