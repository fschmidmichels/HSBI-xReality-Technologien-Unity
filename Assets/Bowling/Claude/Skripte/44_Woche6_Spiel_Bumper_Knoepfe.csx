// Woche 6: Unter der Neustart-Schaltfläche zwei weitere Poke-Schaltflächen anlegen:
// "Neues Spiel" (Score und Frame-Liste löschen) und "Bumper an/aus".
// Ausführen: Datei als .cs kopieren und mit "unity command script eval_file --file <Pfad>" ausführen.
// Voraussetzung: LaneControls.ResetGame und ScoreScript.ResetGame kompiliert, RestartButton aus Skript 41 vorhanden,
// Szene ohne ungespeicherte Änderungen.
// Wiederholbar: vorhandene GameResetButton/BumperButton werden ersetzt.
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/Scenes/BowlingVR.unity") return "Abbruch: BowlingVR ist nicht die aktive Szene";
if (scene.isDirty) return "Abbruch: Szene hat ungespeicherte Änderungen";

var log = new System.Text.StringBuilder();
var laneControls = GameObject.FindFirstObjectByType<LaneControls>();
var restartButton = GameObject.Find("RestartButton");
if (laneControls == null || restartButton == null) return "Abbruch: LaneControls oder RestartButton fehlt";
var font = GameObject.Find("ScoreText").GetComponent<TMPro.TextMeshProUGUI>().font;
var head = new Vector3(0f, 1.6f, -1.5f);   // ungefähre Kopfposition auf dem Anlauf (wie in Skript 41)

GameObject Child(string name, Transform parent)
{
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent, false);
    return go;
}
void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero; }

// Gleicher Aufbau wie die Neustart-Schaltfläche, aber 24 cm x 10 cm und eigene Farbe.
void PokeButton(string name, string label, Vector3 position, Color color, UnityEngine.Events.UnityAction action)
{
    var old = GameObject.Find(name);
    if (old != null) UnityEngine.Object.DestroyImmediate(old);
    var go = new GameObject(name, typeof(RectTransform));
    var canvas = go.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    go.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 10f;
    var rect = go.GetComponent<RectTransform>();
    rect.sizeDelta = new Vector2(480f, 200f);
    rect.localScale = Vector3.one * 0.0005f;   // 1000 Einheiten = 0,5 m
    rect.position = position;
    rect.rotation = Quaternion.LookRotation(position - head);

    var buttonGo = Child("Button", go.transform);
    var buttonRect = buttonGo.GetComponent<RectTransform>();
    Stretch(buttonRect); buttonRect.offsetMin = new Vector2(20f, 15f); buttonRect.offsetMax = new Vector2(-20f, -15f);
    buttonGo.AddComponent<UnityEngine.UI.Image>().color = Color.white;
    var button = buttonGo.AddComponent<UnityEngine.UI.Button>();
    var colors = button.colors;
    colors.normalColor = color;
    colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
    colors.pressedColor = Color.Lerp(color, Color.black, 0.35f);
    colors.selectedColor = colors.normalColor;
    button.colors = colors;

    var textGo = Child("Text", buttonGo.transform);
    Stretch(textGo.GetComponent<RectTransform>());
    var text = textGo.AddComponent<TMPro.TextMeshProUGUI>();
    text.font = font; text.text = label; text.fontSize = 60f; text.color = Color.white;
    text.alignment = TMPro.TextAlignmentOptions.Center; text.raycastTarget = false;

    UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
    var created = Oculus.Interaction.Editor.QuickActions.QuickActionsAPI.AddPokeCanvasInteraction(go);
    log.AppendLine(name + " angelegt, Poke: " + string.Join(", ", created.ConvertAll(g => g.name)));
}

// Neustart (12 cm hoch) aus Skript 41 von 0,95 m auf 1,20 m anheben, damit auch die Schaltflächen darunter
// bequem erreichbar bleiben. Darunter mit 1 cm Abstand je 10 cm hohe Schaltflächen.
UnityEditor.Undo.RecordObject(restartButton.transform, "RestartButton anheben");
Vector3 top = new Vector3(0.95f, 1.20f, -1.05f);
restartButton.transform.SetPositionAndRotation(top, Quaternion.LookRotation(top - head));
log.AppendLine("RestartButton auf 1,20 m angehoben");
PokeButton("GameResetButton", "Neues Spiel", top + new Vector3(0f, -0.12f, 0f),
           new Color(0.6f, 0.2f, 0.2f), laneControls.ResetGame);
PokeButton("BumperButton", "Bumper an/aus", top + new Vector3(0f, -0.23f, 0f),
           new Color(0.2f, 0.45f, 0.25f), laneControls.ToggleBumpers);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
log.AppendLine("Szene gespeichert");
return log.ToString();
