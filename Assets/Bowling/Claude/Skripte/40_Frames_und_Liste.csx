// Frames: Liste der alten Scores links neben dem ScoreBoard, LaneControls.ballReturn verdrahten.
// Voraussetzung: ScoreHistory.cs kompiliert, Szene ohne ungespeicherte Änderungen.
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) return "Abbruch: Szene hat ungespeicherte Änderungen";
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/BowlingVR.unity");

var old = GameObject.Find("ScoreHistoryBoard");
if (old != null) UnityEngine.Object.DestroyImmediate(old);

// Canvas: 352 x 296 Einheiten * 0,00625 = 2,2 m x 1,85 m, links vom ScoreBoard (x -1,5..1,5)
// auf der Blende (Vorderseite z = 17,25) zwischen Neon_Unten (y = 1,05) und Neon_Oben (y = 3,0).
var board = new GameObject("ScoreHistoryBoard", typeof(RectTransform));
var canvas = board.AddComponent<Canvas>();
canvas.renderMode = RenderMode.WorldSpace;
var scaler = board.AddComponent<UnityEngine.UI.CanvasScaler>();
scaler.dynamicPixelsPerUnit = 10f;
var boardRect = board.GetComponent<RectTransform>();
boardRect.sizeDelta = new Vector2(352f, 296f);
boardRect.position = new Vector3(-2.75f, 2.03f, 17.24f);
boardRect.rotation = Quaternion.identity;
boardRect.localScale = Vector3.one * 0.00625f;

var bg = new GameObject("Background", typeof(RectTransform));
bg.transform.SetParent(board.transform, false);
var bgRect = bg.GetComponent<RectTransform>();
bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
var image = bg.AddComponent<UnityEngine.UI.Image>();
image.color = new Color(0.05f, 0.05f, 0.12f, 0.92f);
image.raycastTarget = false;

var textGo = new GameObject("HistoryText", typeof(RectTransform));
textGo.transform.SetParent(board.transform, false);
var textRect = textGo.GetComponent<RectTransform>();
textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
textRect.offsetMin = new Vector2(20f, 10f); textRect.offsetMax = new Vector2(-20f, -10f);
var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
tmp.text = "Frames";
tmp.fontSize = 40f;
tmp.fontStyle = TMPro.FontStyles.Bold;
tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;
tmp.lineSpacing = 0f;
tmp.color = Color.white;
tmp.raycastTarget = false;
textGo.AddComponent<ScoreHistory>();

// X-Taste soll einen noch offenen Wurf abbrechen können.
var laneControls = UnityEngine.Object.FindFirstObjectByType<LaneControls>();
laneControls.ballReturn = GameObject.Find("PitZone").GetComponent<BallReturn>();
EditorUtility.SetDirty(laneControls);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "ScoreHistoryBoard angelegt, ballReturn=" + (laneControls.ballReturn != null);
