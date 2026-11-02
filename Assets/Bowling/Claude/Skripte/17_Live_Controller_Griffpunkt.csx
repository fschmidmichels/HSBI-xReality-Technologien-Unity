var sb = new System.Text.StringBuilder();
Transform grabLoc = null;
foreach (var gi in UnityEngine.Object.FindObjectsByType<Oculus.Interaction.GrabInteractor>(FindObjectsSortMode.None)) {
    var p = gi.transform; bool right = false;
    while (p != null) { if (p.name.EndsWith("Right")) right = true; p = p.parent; }
    if (right) grabLoc = new SerializedObject(gi).FindProperty("_grabTarget").objectReferenceValue as Transform;
}
sb.AppendLine("GrabLocation R pos=" + grabLoc.position.ToString("F3") + " fwd=" + grabLoc.forward.ToString("F2") + " up=" + grabLoc.up.ToString("F2"));
// sichtbare Renderer in der Nähe des rechten Griffpunkts (Controller-Modell)
var min = Vector3.positiveInfinity; var max = Vector3.negativeInfinity; int n = 0;
foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) {
    if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
    if (r.transform.IsChildOf(GameObject.Find("BowlingBall").transform)) continue;
    if (Vector3.Distance(r.bounds.center, grabLoc.position) > 0.25f) continue;
    var m = r.GetComponent<MeshFilter>()?.sharedMesh ?? (r as SkinnedMeshRenderer)?.sharedMesh;
    if (m == null) continue;
    foreach (var v in m.vertices) {
        var lp = grabLoc.InverseTransformPoint(r.transform.TransformPoint(v));
        min = Vector3.Min(min, lp); max = Vector3.Max(max, lp);
    }
    n++; sb.AppendLine("  Renderer " + r.name);
}
sb.AppendLine("Controller-Modell im Griffpunkt-System (" + n + " Renderer): min=" + min.ToString("F3") + " max=" + max.ToString("F3"));
var ball = GameObject.Find("BowlingBall").transform;
sb.AppendLine("Kugelmitte im Griffpunkt-System: " + grabLoc.InverseTransformPoint(ball.position).ToString("F3"));
return sb.ToString();
