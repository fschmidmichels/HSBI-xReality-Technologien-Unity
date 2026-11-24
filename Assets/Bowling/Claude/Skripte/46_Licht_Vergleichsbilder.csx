// Vergleichsbilder für Licht/Reflexionen (Woche 6). "tag" vor dem Ausführen auf "vorher" bzw. "nachher" setzen.
string tag = "vorher";
string d = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bowling") + "/";
System.IO.Directory.CreateDirectory(d);
void Shot(Vector3 pos, Vector3 lookAt, float fov, string file) {
    var go = new GameObject("TempShotCam"); go.hideFlags = HideFlags.HideAndDontSave;
    var cam = go.AddComponent<Camera>(); cam.fieldOfView = fov; cam.nearClipPlane = 0.02f;
    var data = go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    data.renderPostProcessing = false;   // wie die XR-Kamera (CenterEyeAnchor)
    go.transform.position = pos; go.transform.LookAt(lookAt);
    var rt = new RenderTexture(1280, 720, 24); rt.antiAliasing = 4; cam.targetTexture = rt; cam.Render();
    RenderTexture.active = rt;
    var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
    tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
    System.IO.File.WriteAllBytes(file, tex.EncodeToPNG());
    RenderTexture.active = null; cam.targetTexture = null;
    UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(go);
}
Shot(new Vector3(0f, 1.6f, -2.5f), new Vector3(0f, 0.9f, 12f), 70f, d + "licht_" + tag + "_spieler.png");
Shot(new Vector3(0f, 1.2f, 12f), new Vector3(0f, 0.4f, 18.7f), 50f, d + "licht_" + tag + "_pins.png");
Shot(new Vector3(0f, 0.5f, 2f), new Vector3(0f, 0.25f, 18.5f), 45f, d + "licht_" + tag + "_bahn_flach.png");
Shot(new Vector3(-3.5f, 2.8f, 6f), new Vector3(1.5f, 0.8f, -5f), 75f, d + "licht_" + tag + "_rueckblick.png");
return d;
