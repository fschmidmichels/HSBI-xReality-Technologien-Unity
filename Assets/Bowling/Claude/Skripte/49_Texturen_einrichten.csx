// Woche 6 Grafik, Punkt 3: CC0-Texturen von ambientCG (Assets/Bowling/Textures/<Id>/) in die Materialien einbauen.
// - Import: Normal Maps, lineare Masken, ASTC für Android
// - Roughness -> MetallicSmoothness-Map (URP liest Smoothness aus dem Alpha-Kanal, Metallic aus Rot)
// - Würfel bekommen eigene Meshes mit UVs in Metern (Standard-Würfel: 0..1 pro Seite -> Texturen wären gestreckt)
// Danach Licht neu backen (48_Licht_backen.csx), da sich die Albedo und damit das indirekte Licht ändert.
var sb = new System.Text.StringBuilder();
string texRoot = "Assets/Bowling/Textures/";

// Material, Textur-Id, Auflösung, Kachelgröße in m, Smoothness min/max, Grundfarbe
var specs = new (string mat, string id, string res, float tile, float sMin, float sMax, Color tint)[] {
    ("Lane",     "WoodFloor047",      "2K", 1.7f, 0.75f, 0.92f, Color.white),                     // ca. 2,7 cm breite Dielen
    ("Approach", "WoodFloor051",      "1K", 2.0f, 0.50f, 0.75f, Color.white),
    ("HallFloor","Carpet006",         "1K", 1.5f, 0.00f, 0.12f, new Color(0.75f, 0.75f, 0.8f)),
    ("Gutter",   "Metal009",          "1K", 1.0f, 0.40f, 0.75f, Color.white),
    ("Ball",     "Marble016",         "1K", 1.0f, 0.75f, 0.95f, Color.white),
    ("HallWall", "PaintedPlaster017", "1K", 2.0f, 0.00f, 0.25f, new Color(0.30f, 0.27f, 0.50f)), // dunkelviolett wie bisher
};

void Import(string path, bool normal, bool linear, int maxSize) {
    var ti = (TextureImporter)AssetImporter.GetAtPath(path);
    ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
    ti.sRGBTexture = !linear && !normal;
    ti.mipmapEnabled = true; ti.maxTextureSize = maxSize;
    ti.anisoLevel = 4;   // Böden unter flachem Blickwinkel bleiben schärfer
    var android = ti.GetPlatformTextureSettings("Android");
    android.overridden = true; android.maxTextureSize = maxSize;
    android.format = normal ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_6x6;
    ti.SetPlatformTextureSettings(android);
    ti.SaveAndReimport();
}

foreach (var s in specs) {
    string dir = texRoot + s.id + "/"; string pre = dir + s.id + "_" + s.res + "-JPG_";
    int maxSize = s.res == "2K" ? 2048 : 1024;
    // MetallicSmoothness aus Roughness (+ Metalness) erzeugen
    var rough = new Texture2D(2, 2); rough.LoadImage(System.IO.File.ReadAllBytes(pre + "Roughness.jpg"));
    Texture2D metal = null;
    if (System.IO.File.Exists(pre + "Metalness.jpg")) { metal = new Texture2D(2, 2); metal.LoadImage(System.IO.File.ReadAllBytes(pre + "Metalness.jpg")); }
    var rp = rough.GetPixels32(); var mp = metal != null ? metal.GetPixels32() : null;
    var outPx = new Color32[rp.Length];
    for (int i = 0; i < rp.Length; i++) {
        float smooth = Mathf.Lerp(s.sMin, s.sMax, 1f - rp[i].r / 255f);
        outPx[i] = new Color32(mp != null ? mp[i].r : (byte)0, 0, 0, (byte)Mathf.RoundToInt(smooth * 255f));
    }
    var ms = new Texture2D(rough.width, rough.height, TextureFormat.RGBA32, false); ms.SetPixels32(outPx); ms.Apply();
    string msPath = dir + s.id + "_MetallicSmoothness.png";
    System.IO.File.WriteAllBytes(msPath, ms.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(rough); UnityEngine.Object.DestroyImmediate(ms); if (metal) UnityEngine.Object.DestroyImmediate(metal);
    AssetDatabase.ImportAsset(msPath);
    Import(pre + "Color.jpg", false, false, maxSize);
    Import(pre + "NormalGL.jpg", true, true, maxSize);   // GL-Variante = Unity-Konvention (Y nach oben)
    Import(msPath, false, true, maxSize);

    var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/" + s.mat + ".mat");
    m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(pre + "Color.jpg"));
    m.SetColor("_BaseColor", s.tint);
    m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(pre + "NormalGL.jpg"));
    m.SetFloat("_BumpScale", 1f); m.EnableKeyword("_NORMALMAP");
    m.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(msPath));
    m.EnableKeyword("_METALLICSPECGLOSSMAP");
    m.SetFloat("_Smoothness", 1f);            // Faktor auf den Alpha-Kanal, Werte stecken schon in der Map
    m.SetFloat("_SmoothnessTextureChannel", 0f);
    float t = s.mat == "Ball" ? 1f : 1f / s.tile;   // Kugel: Kugel-UVs, sonst UVs in Metern
    m.SetTextureScale("_BaseMap", new Vector2(t, t));
    EditorUtility.SetDirty(m);
    sb.AppendLine(s.mat + " <- " + s.id + " (" + s.res + ", Kachel " + s.tile + " m)");
}

// UVs in Metern: Würfel-Mesh kopieren, UV0 neu (Box-Mapping), UV2 (Lightmap) unverändert
string meshDir = "Assets/Bowling/Meshes/BoxUV";
if (!AssetDatabase.IsValidFolder("Assets/Bowling/Meshes")) AssetDatabase.CreateFolder("Assets/Bowling", "Meshes");
if (!AssetDatabase.IsValidFolder(meshDir)) AssetDatabase.CreateFolder("Assets/Bowling/Meshes", "BoxUV");
// Objekt -> UV drehen? (Dielen bzw. Bürstung längs der Bahn = entlang Z)
var targets = new System.Collections.Generic.Dictionary<string, bool> {
    {"LaneSurface", true}, {"ApproachSurface", true}, {"Gutter_Left", true}, {"Gutter_Right", true},
    {"PlayerAreaFloor", false}, {"Surround_Left", false}, {"Surround_Right", false},
    {"Wall_Left", false}, {"Wall_Right", false}, {"Wall_Back", false} };
var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
Mesh BoxMesh(Transform tr, bool rotate) {
    var src = tr.GetComponent<MeshFilter>().sharedMesh;
    if (src.name.StartsWith("BoxUV_")) src = cube;   // erneutes Ausführen: immer vom Würfel ausgehen
    var m = UnityEngine.Object.Instantiate(src); m.name = "BoxUV_" + tr.name;
    var v = m.vertices; var n = m.normals; var uv = new Vector2[v.Length];
    var sc = tr.lossyScale;
    for (int i = 0; i < v.Length; i++) {
        var p = Vector3.Scale(v[i], sc); var a = new Vector3(Mathf.Abs(n[i].x), Mathf.Abs(n[i].y), Mathf.Abs(n[i].z));
        if (a.y >= a.x && a.y >= a.z) uv[i] = rotate ? new Vector2(p.z, p.x) : new Vector2(p.x, p.z);
        else if (a.x >= a.z)          uv[i] = rotate ? new Vector2(p.z, p.y) : new Vector2(p.z, p.y);
        else                          uv[i] = new Vector2(p.x, p.y);
    }
    m.uv = uv; m.RecalculateTangents();   // Tangenten passend zu den neuen UVs (Normal Maps)
    string path = meshDir + "/" + m.name + ".asset";
    var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if (existing != null) { EditorUtility.CopySerialized(m, existing); UnityEngine.Object.DestroyImmediate(m); return existing; }
    AssetDatabase.CreateAsset(m, path); return m;
}
// Objekte aus Prefabs im Prefab-Asset ändern, sonst in der Szene
var prefabEdits = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)) {
    if (!targets.ContainsKey(mf.name)) continue;
    if (PrefabUtility.IsPartOfPrefabInstance(mf.gameObject)) {
        string ap = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(mf.gameObject);
        if (!prefabEdits.ContainsKey(ap)) prefabEdits[ap] = new System.Collections.Generic.List<string>();
        prefabEdits[ap].Add(mf.name);
    } else {
        Undo.RecordObject(mf, "BoxUV"); mf.sharedMesh = BoxMesh(mf.transform, targets[mf.name]);
        sb.AppendLine("Szene: " + mf.name);
    }
}
foreach (var kv in prefabEdits) {
    var root = PrefabUtility.LoadPrefabContents(kv.Key);
    foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        if (kv.Value.Contains(mf.name)) { mf.sharedMesh = BoxMesh(mf.transform, targets[mf.name]); sb.AppendLine("Prefab " + System.IO.Path.GetFileName(kv.Key) + ": " + mf.name); }
    PrefabUtility.SaveAsPrefabAsset(root, kv.Key); PrefabUtility.UnloadPrefabContents(root);
}
AssetDatabase.SaveAssets();
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return sb.ToString();
