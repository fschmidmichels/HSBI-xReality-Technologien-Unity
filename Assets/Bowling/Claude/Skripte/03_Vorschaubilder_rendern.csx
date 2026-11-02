void Shot(Vector3 pos, Vector3 lookAt, float fov, string file) {
    var go = new GameObject("TempShotCam");
    go.hideFlags = HideFlags.HideAndDontSave;
    var cam = go.AddComponent<Camera>();
    cam.fieldOfView = fov;
    cam.nearClipPlane = 0.02f;
    go.transform.position = pos;
    go.transform.LookAt(lookAt);
    var rt = new RenderTexture(1280, 720, 24);
    cam.targetTexture = rt;
    cam.Render();
    RenderTexture.active = rt;
    var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
    tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
    tex.Apply();
    System.IO.File.WriteAllBytes(file, tex.EncodeToPNG());
    RenderTexture.active = null;
    cam.targetTexture = null;
    UnityEngine.Object.DestroyImmediate(rt);
    UnityEngine.Object.DestroyImmediate(tex);
    UnityEngine.Object.DestroyImmediate(go);
}
string d = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bowling") + "/"; System.IO.Directory.CreateDirectory(d);
Shot(new Vector3(0f, 1.6f, -3.5f), new Vector3(0f, 0f, 12f), 60f, d + "player.png");
Shot(new Vector3(-0.5f, 0.45f, 17.4f), new Vector3(0f, 0.15f, 18.7f), 50f, d + "pins.png");
Shot(new Vector3(0.55f, 1.25f, -3.75f), new Vector3(1.0f, 0.9f, -3.3f), 50f, d + "ball.png");
return "ok";
