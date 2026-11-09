// Score-Anzeige: World-Space-Canvas auf der Blende (MaskingUnit) über den Pins.
// Die Rückwand (z = 20,2) liegt hinter der Blende und ist vom Anlauf aus verdeckt.
// Voraussetzung: TMP Essential Resources importiert, ScoreScript.cs kompiliert.
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/BowlingVR.unity");

var old = GameObject.Find("ScoreBoard");
if (old != null) UnityEngine.Object.DestroyImmediate(old);

// Canvas: 480 x 128 Einheiten * 0,00625 = 3 m x 0,8 m, knapp vor der Blende (Vorderseite z = 17,25),
// zwischen Neon_Unten (y = 1,05) und Neon_Oben (y = 3,0).
// Rotation 0: Die Vorderseite eines Canvas zeigt in -z, also zum Spieler.
var board = new GameObject("ScoreBoard", typeof(RectTransform));
var canvas = board.AddComponent<Canvas>();
canvas.renderMode = RenderMode.WorldSpace;
var scaler = board.AddComponent<UnityEngine.UI.CanvasScaler>();
scaler.dynamicPixelsPerUnit = 10f;   // schärfere Darstellung im World Space
var boardRect = board.GetComponent<RectTransform>();
boardRect.sizeDelta = new Vector2(480f, 128f);
boardRect.position = new Vector3(0f, 2.0f, 17.24f);
boardRect.rotation = Quaternion.identity;
boardRect.localScale = Vector3.one * 0.00625f;

// Dunkler Hintergrund wie ein Monitor.
var bg = new GameObject("Background", typeof(RectTransform));
bg.transform.SetParent(board.transform, false);
var bgRect = bg.GetComponent<RectTransform>();
bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
var image = bg.AddComponent<UnityEngine.UI.Image>();
image.color = new Color(0.05f, 0.05f, 0.12f, 0.92f);
image.raycastTarget = false;

// Text mit ScoreScript.
var textGo = new GameObject("ScoreText", typeof(RectTransform));
textGo.transform.SetParent(board.transform, false);
var textRect = textGo.GetComponent<RectTransform>();
textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
textRect.offsetMin = new Vector2(10f, 5f); textRect.offsetMax = new Vector2(-10f, -5f);
var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
tmp.text = "Score: 0";
tmp.fontSize = 96f;
tmp.fontStyle = TMPro.FontStyles.Bold;
tmp.alignment = TMPro.TextAlignmentOptions.Center;
tmp.color = Color.white;
tmp.raycastTarget = false;
textGo.AddComponent<ScoreScript>();

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "ScoreBoard angelegt";
