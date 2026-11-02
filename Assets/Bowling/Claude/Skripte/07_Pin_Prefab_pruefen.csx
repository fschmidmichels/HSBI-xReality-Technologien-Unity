var sb = new System.Text.StringBuilder();
var imp = (ModelImporter)AssetImporter.GetAtPath("Assets/Bowling/Models/BowlingPin.fbx");
sb.AppendLine("globalScale=" + imp.globalScale + " useFileScale=" + imp.useFileScale + " fileScale=" + imp.fileScale);
var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bowling/Prefabs/CollidableBowlingPin.prefab");
var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
foreach (var t in go.GetComponentsInChildren<Transform>())
    sb.AppendLine(t.name + " localPos=" + t.localPosition.ToString("F4") + " localScale=" + t.localScale.ToString("F4") + " lossy=" + t.lossyScale.ToString("F4") + " rot=" + t.localEulerAngles);
var mf = go.GetComponentInChildren<MeshFilter>();
sb.AppendLine("mesh bounds size=" + mf.sharedMesh.bounds.size.ToString("F4") + " center=" + mf.sharedMesh.bounds.center.ToString("F4"));
var r = go.GetComponentInChildren<Renderer>();
sb.AppendLine("renderer bounds min=" + r.bounds.min.ToString("F4") + " max=" + r.bounds.max.ToString("F4"));
UnityEngine.Object.DestroyImmediate(go);
return sb.ToString();
