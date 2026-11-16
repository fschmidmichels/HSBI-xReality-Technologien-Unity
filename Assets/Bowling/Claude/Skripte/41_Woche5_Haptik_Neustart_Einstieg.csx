// Woche 5: Haptik beim Greifen, Neustart-Schaltfläche (Poke) und Einstiegshinweis.
// Ausführen: Datei als .cs kopieren und mit "unity command script eval_file --file <Pfad>" ausführen.
// Voraussetzung: GrabHaptics.cs, ThrowHaptics.cs und OnboardingHint.cs kompiliert, Szene ohne ungespeicherte Änderungen.
// Wiederholbar: vorhandene RestartButton/OnboardingHint werden ersetzt.
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/Scenes/BowlingVR.unity") return "Abbruch: BowlingVR ist nicht die aktive Szene";
if (scene.isDirty) return "Abbruch: Szene hat ungespeicherte Änderungen";

var log = new System.Text.StringBuilder();
var laneControls = GameObject.FindFirstObjectByType<LaneControls>();
var gameManager = GameObject.FindFirstObjectByType<GameManager>();
var font = GameObject.Find("ScoreText").GetComponent<TMPro.TextMeshProUGUI>().font;
var head = new Vector3(0f, 1.6f, -1.5f);   // ungefähre Kopfposition auf dem Anlauf

// ---------- 1. GrabHaptics am Kugel-Prefab ----------
const string ballPrefabPath = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var ballRoot = UnityEditor.PrefabUtility.LoadPrefabContents(ballPrefabPath);
if (ballRoot.GetComponent<GrabHaptics>() == null)
{
    var haptics = ballRoot.AddComponent<GrabHaptics>();
    haptics.amplitude = 0.6f;
    haptics.duration = 0.15f;
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(ballRoot, ballPrefabPath);
    log.AppendLine("GrabHaptics an BowlingBall.prefab ergänzt");
}
else log.AppendLine("GrabHaptics war schon vorhanden");
// ThrowHaptics: Impuls beim Loslassen, Stärke abhängig von der Wurfgeschwindigkeit.
if (ballRoot.GetComponent<ThrowHaptics>() == null)
{
    ballRoot.AddComponent<ThrowHaptics>();
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(ballRoot, ballPrefabPath);
    log.AppendLine("ThrowHaptics an BowlingBall.prefab ergänzt");
}
else log.AppendLine("ThrowHaptics war schon vorhanden");
UnityEditor.PrefabUtility.UnloadPrefabContents(ballRoot);

// ---------- Hilfsfunktionen ----------
GameObject Child(string name, Transform parent)
{
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent, false);
    return go;
}
void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero; }
TMPro.TextMeshProUGUI Text(Transform parent, string content, float size)
{
    var go = Child("Text", parent);
    Stretch(go.GetComponent<RectTransform>());
    var t = go.AddComponent<TMPro.TextMeshProUGUI>();
    t.font = font; t.text = content; t.fontSize = size; t.color = Color.white;
    t.alignment = TMPro.TextAlignmentOptions.Center; t.raycastTarget = false;
    return t;
}
GameObject WorldCanvas(string name, Vector2 size, Vector3 position)
{
    var old = GameObject.Find(name);
    if (old != null) UnityEngine.Object.DestroyImmediate(old);
    var go = new GameObject(name, typeof(RectTransform));
    var canvas = go.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    go.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 10f;
    var rect = go.GetComponent<RectTransform>();
    rect.sizeDelta = size;
    rect.localScale = Vector3.one * 0.0005f;   // 1000 Einheiten = 0,5 m
    rect.position = position;
    rect.rotation = Quaternion.LookRotation(position - head);   // zur spielenden Person geneigt
    return go;
}

// ---------- 2. Neustart-Schaltfläche (24 cm x 12 cm) vor der Kugelablage ----------
var restart = WorldCanvas("RestartButton", new Vector2(480f, 240f), new Vector3(0.95f, 0.95f, -1.05f));
var buttonGo = Child("Button", restart.transform);
var buttonRect = buttonGo.GetComponent<RectTransform>();
Stretch(buttonRect); buttonRect.offsetMin = new Vector2(20f, 20f); buttonRect.offsetMax = new Vector2(-20f, -20f);
var image = buttonGo.AddComponent<UnityEngine.UI.Image>();
image.color = Color.white;
var button = buttonGo.AddComponent<UnityEngine.UI.Button>();
var colors = button.colors;
colors.normalColor = new Color(0.12f, 0.35f, 0.75f);
colors.highlightedColor = new Color(0.2f, 0.5f, 0.95f);
colors.pressedColor = new Color(0.08f, 0.22f, 0.5f);
colors.selectedColor = colors.normalColor;
button.colors = colors;
Text(buttonGo.transform, "Neustart", 72f);
UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, laneControls.ResetBallAndPins);

var created = Oculus.Interaction.Editor.QuickActions.QuickActionsAPI.AddPokeCanvasInteraction(restart);
log.AppendLine("Poke Quick Action: " + string.Join(", ", created.ConvertAll(g => g.name)));

// ---------- 3. Einstiegshinweis (45 cm x 15 cm) über der Kugelablage ----------
var hint = WorldCanvas("OnboardingHint", new Vector2(900f, 300f), new Vector3(1.0f, 1.4f, -1.25f));
var bg = Child("Background", hint.transform);
Stretch(bg.GetComponent<RectTransform>());
var bgImage = bg.AddComponent<UnityEngine.UI.Image>();
bgImage.color = new Color(0.05f, 0.05f, 0.12f, 0.85f);
bgImage.raycastTarget = false;
var hintText = Text(bg.transform, "Greife die Kugel und rolle sie auf die Bahn in Richtung der Pins.\n<size=70%>Neustart: Schaltfläche neben der Ablage</size>", 56f);
hintText.margin = new Vector4(30f, 20f, 30f, 20f);
var onboarding = hint.AddComponent<OnboardingHint>();   // fügt CanvasGroup hinzu
onboarding.gameManager = gameManager;
var group = hint.GetComponent<CanvasGroup>();
group.interactable = false; group.blocksRaycasts = false;

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
log.AppendLine("Szene gespeichert");
return log.ToString();
