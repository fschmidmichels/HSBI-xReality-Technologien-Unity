// Ersetzt die flachen Glow-Quader (Seitenflächen erzeugten eine feine Lichtkante) durch Quads, die nur nach unten zeigen.
const string path = "Assets/Bowling/Prefabs/BowlingHall.prefab";
var root = PrefabUtility.LoadPrefabContents(path);
var mGlow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/CeilingGlow.mat");
int n = 0;
var panelList = new System.Collections.Generic.List<Transform>(); foreach (Transform t in root.transform.Find("CeilingPanels")) panelList.Add(t);
foreach (var panel in panelList) {
    if (!panel.name.StartsWith("Panel_")) continue;
    var old = panel.Find("Glow");
    if (old == null) continue;
    Vector3 worldPos = old.position, worldSize = old.lossyScale;
    UnityEngine.Object.DestroyImmediate(old.gameObject);
    var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
    q.name = "Glow";
    UnityEngine.Object.DestroyImmediate(q.GetComponent<Collider>());
    q.transform.SetParent(root.transform, false);
    q.transform.position = new Vector3(worldPos.x, worldPos.y + worldSize.y / 2f, worldPos.z);  // Unterseite der Decke minus 2 mm
    q.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);   // Vorderseite zeigt nach unten
    q.transform.localScale = new Vector3(worldSize.x, worldSize.z, 1f);
    var mr = q.GetComponent<MeshRenderer>();
    mr.sharedMaterial = mGlow;
    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    q.isStatic = true;
    q.transform.SetParent(panel, true);
    n++;
}
PrefabUtility.SaveAsPrefabAsset(root, path);
PrefabUtility.UnloadPrefabContents(root);
return n + " Glow-Flächen als Quad ersetzt";
