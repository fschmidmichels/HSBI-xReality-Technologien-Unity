const string prefabPath = "Assets/Bowling/Prefabs/CollidableBowlingPin.prefab";
var root = PrefabUtility.LoadPrefabContents(prefabPath);
var box = root.GetComponent<BoxCollider>();
var size = box.size;
box.size = new Vector3(size.x, 0.05f, 0.05f);   // Fußdurchmesser echter Pin ca. 2 in
var rb = root.GetComponent<Rigidbody>();
// Pivot liegt 0,3312 m über dem Boden, lokale +X zeigt nach unten -> Schwerpunkt 0,145 m über dem Boden
rb.automaticCenterOfMass = false;
rb.centerOfMass = new Vector3(0.3312f - 0.145f, 0f, 0f);
PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
PrefabUtility.UnloadPrefabContents(root);
AssetDatabase.SaveAssets();
return "Box size=" + box.size.ToString("F4") + " COM=" + rb.centerOfMass.ToString("F4");
