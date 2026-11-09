// Automatische Kugelrückholung: BallReturn an PitZone hängen.
// PitZone liegt im Prefab BowlingLane, die Komponente wird als Szenen-Override angelegt,
// weil sie auf LaneControls (Szenenobjekt) verweist.
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/BowlingVR.unity");

var pitZone = GameObject.Find("PitZone");
var laneControls = UnityEngine.Object.FindFirstObjectByType<LaneControls>();

var ballReturn = pitZone.GetComponent<BallReturn>();
if (ballReturn == null) ballReturn = pitZone.AddComponent<BallReturn>();
ballReturn.laneControls = laneControls;
ballReturn.returnDelay = 2f;
EditorUtility.SetDirty(ballReturn);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "BallReturn an " + pitZone.name + ", ball=" + laneControls.ball.name;
