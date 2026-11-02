// Vertieft die Rinnen der Referenzbahn auf 8,9 cm (flache Rinne nach USBC: 3 3/8–3 5/8 in) und passt Bumper und Trigger-Zonen an.
const float depth = 0.089f;
const string path = "Assets/Bowling/Prefabs/BowlingLane.prefab";
var sb = new System.Text.StringBuilder();
var root = PrefabUtility.LoadPrefabContents(path);
void SetY(Transform t, float minY, float maxY) {
    var p = t.localPosition; var s = t.localScale;
    float oldMin = p.y - s.y / 2f, oldMax = p.y + s.y / 2f;
    t.localPosition = new Vector3(p.x, (minY + maxY) / 2f, p.z);
    t.localScale = new Vector3(s.x, maxY - minY, s.z);
    sb.AppendLine(t.name + ": y " + oldMin.ToString("F3") + ".." + oldMax.ToString("F3") + " -> " + minY.ToString("F3") + ".." + maxY.ToString("F3"));
}
foreach (var side in new[] { "Left", "Right" }) {
    SetY(root.transform.Find("Gutters/Gutter_" + side), -depth - 0.05f, -depth);     // Rinnenboden, 5 cm stark
    SetY(root.transform.Find("Bumpers/Bumper_" + side), -depth, 0.18f);              // Bumper reicht bis zum Rinnenboden
    SetY(root.transform.Find("Gutters/Capping_" + side), -depth - 0.05f, 0.05f);     // Abschlussleiste bis unter den Rinnenboden
    // Trigger-Zone: BoxCollider auf leerem Objekt, Größe über den Collider
    var zone = root.transform.Find("Zones/GutterZone_" + side);
    var col = zone.GetComponent<BoxCollider>();
    float top = zone.localPosition.y + col.center.y + col.size.y / 2f;
    float bottom = -depth;
    zone.localPosition = new Vector3(zone.localPosition.x, (top + bottom) / 2f, zone.localPosition.z);
    col.center = new Vector3(col.center.x, 0f, col.center.z);
    col.size = new Vector3(col.size.x, top - bottom, col.size.z);
    sb.AppendLine(zone.name + ": y " + bottom.ToString("F3") + ".." + top.ToString("F3"));
}
// Bahnkörper ebenfalls bis unter den Rinnenboden, damit keine Lücke unter der Bahnkante entsteht
SetY(root.transform.Find("Lane/LaneSurface"), -depth - 0.05f, 0f);
PrefabUtility.SaveAsPrefabAsset(root, path);
PrefabUtility.UnloadPrefabContents(root);
AssetDatabase.SaveAssets();
return sb.ToString();
