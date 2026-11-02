// Erzeugt eine Verlaufstextur für den Lichtschein um die Deckenpaneele und weist sie dem Material CeilingGlow zu.
const string folder = "Assets/Bowling/Textures";
if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Bowling", "Textures");
const string texPath = folder + "/CeilingGlow.png";
const int w = 256, h = 160;                       // Seitenverhältnis wie die Glow-Fläche (2,0 x 1,2 m)
// Paneel liegt in der Mitte und ist 1,2 x 0,4 m groß -> innerer Bereich in Texturkoordinaten
float halfPanelX = 0.6f / 2.0f, halfPanelY = 0.2f / 1.2f;
var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
for (int y = 0; y < h; y++) {
    for (int x = 0; x < w; x++) {
        float u = Mathf.Abs((x + 0.5f) / w - 0.5f), v = Mathf.Abs((y + 0.5f) / h - 0.5f);
        // Abstand vom Paneelrand (0 am Paneel, 1 am Rand der Fläche)
        float dx = Mathf.Max(0f, u - halfPanelX) / (0.5f - halfPanelX);
        float dy = Mathf.Max(0f, v - halfPanelY) / (0.5f - halfPanelY);
        float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
        float a = Mathf.Pow(1f - d, 2.2f);       // weicher Abfall
        tex.SetPixel(x, y, new Color(a, a, a, 1f));
    }
}
tex.Apply();
System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(tex);
AssetDatabase.ImportAsset(texPath);
var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
imp.wrapMode = TextureWrapMode.Clamp;
imp.mipmapEnabled = true;
imp.sRGBTexture = true;
imp.SaveAndReimport();

var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/CeilingGlow.mat");
var ceiling = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/HallCeiling.mat");
m.SetColor("_BaseColor", ceiling.GetColor("_BaseColor"));   // Rand geht unsichtbar in die Decke über
m.SetFloat("_Smoothness", ceiling.GetFloat("_Smoothness"));
m.SetTexture("_EmissionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));
m.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.85f) * 0.8f);
m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
m.SetKeyword(new UnityEngine.Rendering.LocalKeyword(m.shader, "_EMISSION"), true);
EditorUtility.SetDirty(m);
AssetDatabase.SaveAssets();
return "Textur " + texPath + ", Material CeilingGlow aktualisiert";
