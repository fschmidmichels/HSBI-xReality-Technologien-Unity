var temp = new System.Collections.Generic.List<GameObject>();
var ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bowling/Prefabs/BowlingBall.prefab");
var ghostBase = "Packages/com.meta.xr.sdk.interaction/Runtime/Prefabs/HandGrab/OpenXRGhost-Hand";

GameObject Spawn(GameObject prefab) {
    var g = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    foreach (var t in g.GetComponentsInChildren<Transform>(true)) t.gameObject.hideFlags = HideFlags.HideAndDontSave;
    temp.Add(g);
    return g;
}
// Kugel rechts (Ball wie gegriffen gehalten, Handfläche nach oben/hinten wie beim Ausholen) und links daneben
Vector3[] centers = { new Vector3(10.3f, 1.2f, -6f), new Vector3(9.7f, 1.2f, -6f) };
string[] sides = { "Right", "Left" };
for (int i = 0; i < 2; i++) {
    var ball = Spawn(ballPrefab);
    ball.transform.position = centers[i];
    ball.transform.rotation = Quaternion.Euler(200f, 0f, 0f);   // gekippt, damit Hand und Löcher von der Seite sichtbar sind
    var pose = ball.GetComponentInChildren<Oculus.Interaction.HandGrab.HandGrabPose>().transform;
    var ghost = Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(ghostBase + sides[i] + ".prefab"));
    ghost.transform.SetPositionAndRotation(pose.position, pose.rotation);
}

void Shot(Vector3 pos, Vector3 lookAt, string file) {
    var go = new GameObject("TempShotCam"); go.hideFlags = HideFlags.HideAndDontSave;
    var cam = go.AddComponent<Camera>(); cam.fieldOfView = 40f; cam.nearClipPlane = 0.02f;
    go.transform.position = pos; go.transform.LookAt(lookAt);
    var rt = new RenderTexture(1280, 720, 24); cam.targetTexture = rt; cam.Render();
    RenderTexture.active = rt;
    var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
    tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
    System.IO.File.WriteAllBytes(file, tex.EncodeToPNG());
    RenderTexture.active = null; cam.targetTexture = null;
    UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(go);
}
string d = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bowling") + "/"; System.IO.Directory.CreateDirectory(d);
Shot(new Vector3(10f, 1.35f, -7.1f), new Vector3(10f, 1.2f, -6f), d + "pose_front.png");
Shot(new Vector3(11.2f, 1.25f, -6f), new Vector3(10.3f, 1.2f, -6f), d + "pose_side.png");
foreach (var g in temp) UnityEngine.Object.DestroyImmediate(g);
return "ok";
