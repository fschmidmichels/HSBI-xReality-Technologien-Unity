var sb = new System.Text.StringBuilder();
string basePath = "Packages/com.meta.xr.sdk.interaction/Runtime/Prefabs/HandGrab/";
foreach (var side in new[] { "Left", "Right" }) {
    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "OpenXRGhost-Hand" + side + ".prefab");
    var g = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    g.hideFlags = HideFlags.HideAndDontSave;
    g.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    sb.AppendLine("== " + side + " root components: " + string.Join(",", System.Array.ConvertAll(g.GetComponents<Component>(), c => c.GetType().Name)));
    foreach (var t in g.GetComponentsInChildren<Transform>(true)) {
        string n = t.name.ToLower();
        if (n.Contains("wrist") || n.Contains("palm") || n.Contains("tip") || n.Contains("thumb") && n.Contains("metacarpal") || n.Contains("middle") && n.Contains("proximal") || n.Contains("little") && n.Contains("proximal") || n.Contains("index") && n.Contains("proximal") || t.parent == g.transform)
            sb.AppendLine(t.name + " pos=" + t.position.ToString("F3") + " fwd=" + t.forward.ToString("F2") + " up=" + t.up.ToString("F2") + " right=" + t.right.ToString("F2"));
    }
    UnityEngine.Object.DestroyImmediate(g);
}
return sb.ToString();
