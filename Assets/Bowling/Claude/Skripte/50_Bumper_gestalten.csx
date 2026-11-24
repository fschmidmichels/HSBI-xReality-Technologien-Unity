// Woche 6 Grafik: Bumper als dunkler Hochglanz-Kunststoff mit leuchtendem orangen Streifen zur Bahn hin.
// Bumper werden zur Laufzeit ein-/ausgeschaltet (LaneControls.ToggleBumpers) und tragen deshalb NICHT zum
// gebackenen Licht bei, sonst bleiben ihre Schatten auf der Bahn, wenn sie ausgeschaltet sind. Beleuchtung über Light Probes.
// Danach Licht neu backen (48_Licht_backen.csx).
var sb = new System.Text.StringBuilder();

// 1) Material Bumper: dunkler Hochglanz-Kunststoff
var body = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/Bumper.mat");
body.SetColor("_BaseColor", new Color(0.035f, 0.035f, 0.045f));
body.SetFloat("_Smoothness", 0.85f); body.SetFloat("_Metallic", 0f);
EditorUtility.SetDirty(body);

// 2) Material für den Leuchtstreifen (neu, Kopie von Bumper als Basis)
string stripePath = "Assets/Bowling/Materials/BumperStripe.mat";
var stripe = AssetDatabase.LoadAssetAtPath<Material>(stripePath);
if (stripe == null) { stripe = new Material(body); AssetDatabase.CreateAsset(stripe, stripePath); }
stripe.CopyPropertiesFromMaterial(body);
stripe.SetColor("_BaseColor", new Color(1f, 0.35f, 0.02f));
stripe.SetFloat("_Smoothness", 0.6f);
stripe.EnableKeyword("_EMISSION");
stripe.SetColor("_EmissionColor", new Color(1f, 0.30f, 0.02f) * 1.1f);   // knapp über 1: ohne Tonemapping würden höhere Werte G mit abschneiden -> gelb statt orange
stripe.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;  // nicht "None", sonst schaltet URP die Emission ab
EditorUtility.SetDirty(stripe);

// 3) Prefab BowlingLane: Streifen an die Innenseite der Bumper, Bumper aus dem Lightmap nehmen
string prefabPath = "Assets/Bowling/Prefabs/BowlingLane.prefab";
var root = PrefabUtility.LoadPrefabContents(prefabPath);
var bumpers = root.transform.Find("Bumpers");
foreach (var side in new[] { "Left", "Right" }) {
    var b = bumpers.Find("Bumper_" + side);
    var r = b.GetComponent<MeshRenderer>();
    GameObjectUtility.SetStaticEditorFlags(b.gameObject, GameObjectUtility.GetStaticEditorFlags(b.gameObject) & ~StaticEditorFlags.ContributeGI);
    r.receiveGI = ReceiveGI.LightProbes;

    var old = bumpers.Find("Bumper_" + side + "_Streifen");
    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
    s.name = "Bumper_" + side + "_Streifen";
    UnityEngine.Object.DestroyImmediate(s.GetComponent<Collider>());   // Physik der Bumper bleibt unverändert
    s.transform.SetParent(bumpers, false);
    float sign = side == "Left" ? 1f : -1f;                             // Innenseite zeigt zur Bahnmitte
    float inner = b.localPosition.x + sign * b.localScale.x * 0.5f;
    float top = b.localPosition.y + b.localScale.y * 0.5f;
    s.transform.localPosition = new Vector3(inner + sign * 0.002f, top - 0.035f, b.localPosition.z);
    s.transform.localScale = new Vector3(0.006f, 0.022f, b.localScale.z);
    var sr = s.GetComponent<MeshRenderer>();
    sr.sharedMaterial = stripe;
    sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    sr.receiveGI = ReceiveGI.LightProbes;
    GameObjectUtility.SetStaticEditorFlags(s, GameObjectUtility.GetStaticEditorFlags(b.gameObject));   // wie Bumper, ohne ContributeGI
    sb.AppendLine(s.name + " pos=" + s.transform.localPosition.ToString("F3"));
}
PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
PrefabUtility.UnloadPrefabContents(root);
AssetDatabase.SaveAssets();
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return sb.ToString();
