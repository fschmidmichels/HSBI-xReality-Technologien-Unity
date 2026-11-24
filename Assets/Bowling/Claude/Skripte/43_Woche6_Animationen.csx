// Woche 6: Startpunkt für die animierte Rückholung der Kugel anlegen und an LaneControls zuweisen.
// Ausführen: Datei als .cs kopieren und mit "unity command script eval_file --file <Pfad>" ausführen.
// Voraussetzung: PinGroup.cs, LaneControls.cs und GameManager.cs mit Woche-6-Stand kompiliert, Szene ohne ungespeicherte Änderungen.
// Wiederholbar: Ein vorhandenes BallReturnExit wird wiederverwendet und neu positioniert.
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/Scenes/BowlingVR.unity") return "Abbruch: BowlingVR ist nicht die aktive Szene";
if (scene.isDirty) return "Abbruch: Szene hat ungespeicherte Änderungen";

var log = new System.Text.StringBuilder();
var laneControls = GameObject.FindFirstObjectByType<LaneControls>();
var rack = GameObject.Find("BallRack");
if (laneControls == null || rack == null) return "Abbruch: LaneControls oder BallRack fehlt";

// Die Ablage (Tray) verläuft entlang z von -1,8 bis -1,2; die Kugel liegt bei z = -1,5.
// Die Kugel startet hinter der Ablage (vom Spieler aus gesehen hinten rechts) knapp über den Rails
// und rollt nach vorn auf ihre Startposition.
Transform exit = rack.transform.Find("BallReturnExit");
if (exit == null)
{
    exit = new GameObject("BallReturnExit").transform;
    UnityEditor.Undo.RegisterCreatedObjectUndo(exit.gameObject, "BallReturnExit");
    exit.SetParent(rack.transform, false);
    log.AppendLine("BallReturnExit angelegt");
}
else log.AppendLine("BallReturnExit war schon vorhanden");
exit.position = new Vector3(1.0f, 0.98f, -2.1f);
exit.rotation = Quaternion.identity;

UnityEditor.Undo.RecordObject(laneControls, "LaneControls BallReturnExit");
laneControls.ballReturnExit = exit;
laneControls.ballReturnDuration = 0.8f;
UnityEditor.EditorUtility.SetDirty(laneControls);
log.AppendLine("LaneControls.ballReturnExit zugewiesen, Dauer 0,8 s");

var pinGroup = laneControls.pinGroup;
UnityEditor.Undo.RecordObject(pinGroup, "PinGroup Animation");
pinGroup.dropHeight = 1.0f;
pinGroup.resetDuration = 1.0f;
UnityEditor.EditorUtility.SetDirty(pinGroup);
log.AppendLine("PinGroup: dropHeight 1 m, resetDuration 1 s");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
log.AppendLine("Szene gespeichert");
return log.ToString();
