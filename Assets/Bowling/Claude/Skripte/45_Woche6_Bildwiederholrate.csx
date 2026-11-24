// Woche 6: DisplayRefreshRate (Ziel 90 Hz) an das GameManager-Objekt hängen.
// Ausführen: Datei als .cs kopieren und mit "unity command script eval_file --file <Pfad>" ausführen.
// Voraussetzung: DisplayRefreshRate.cs kompiliert, Szene ohne ungespeicherte Änderungen.
// Wiederholbar: Eine vorhandene Komponente wird wiederverwendet.
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/Scenes/BowlingVR.unity") return "Abbruch: BowlingVR ist nicht die aktive Szene";
if (scene.isDirty) return "Abbruch: Szene hat ungespeicherte Änderungen";

var log = new System.Text.StringBuilder();
var gameManager = GameObject.FindFirstObjectByType<GameManager>();
if (gameManager == null) return "Abbruch: GameManager fehlt";

var refresh = gameManager.GetComponent<DisplayRefreshRate>();
if (refresh == null)
{
    refresh = UnityEditor.Undo.AddComponent<DisplayRefreshRate>(gameManager.gameObject);
    log.AppendLine("DisplayRefreshRate an " + gameManager.name + " ergänzt");
}
else log.AppendLine("DisplayRefreshRate war schon vorhanden");
UnityEditor.Undo.RecordObject(refresh, "DisplayRefreshRate 90 Hz");
refresh.targetRefreshRate = 90f;
UnityEditor.EditorUtility.SetDirty(refresh);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
log.AppendLine("Ziel 90 Hz, Szene gespeichert");
return log.ToString();
