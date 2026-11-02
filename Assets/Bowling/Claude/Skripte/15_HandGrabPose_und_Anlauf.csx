var sb = new System.Text.StringBuilder();

// ---------- 1) Hand Grab Pose an der Kugel ----------
const string ballPath = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var ball = PrefabUtility.LoadPrefabContents(ballPath);
var interaction = ball.transform.Find("GrabInteraction");
var old = interaction.Find("HandGrabPose");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

// Handgelenk-Pose im Koordinatensystem der Kugel (OpenXR-Hand: +Z zu den Fingern, -Y = Handfläche).
// Kugelmitte liegt 0,115 m unter dem Handgelenk und 0,06 m Richtung Finger -> Grifflöcher (+Y der Kugel) zeigen zur Handfläche,
// Fingerlöcher Richtung Fingerspitzen, Daumenloch Richtung Handgelenk. Linke und rechte Hand sind an X gespiegelt,
// die Lochanordnung ist dazu symmetrisch -> eine Pose ohne Fingerstellung gilt für beide Hände.
var poseGo = new GameObject("HandGrabPose");
poseGo.transform.SetParent(interaction, false);
poseGo.transform.localPosition = new Vector3(0f, 0.115f, -0.06f);
poseGo.transform.localRotation = Quaternion.identity;
var pose = poseGo.AddComponent<Oculus.Interaction.HandGrab.HandGrabPose>();
var soP = new SerializedObject(pose);
soP.FindProperty("_relativeTo").objectReferenceValue = ball.transform;
soP.FindProperty("_usesHandPose").boolValue = false;
soP.ApplyModifiedPropertiesWithoutUndo();

var hgi = interaction.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>();
var soH = new SerializedObject(hgi);
var list = soH.FindProperty("_handGrabPoses");
list.ClearArray();
list.InsertArrayElementAtIndex(0);
list.GetArrayElementAtIndex(0).objectReferenceValue = pose;
soH.FindProperty("_handAligment").enumValueIndex = (int)Oculus.Interaction.HandGrab.HandAlignType.AlignOnGrab;
soH.ApplyModifiedPropertiesWithoutUndo();
PrefabUtility.SaveAsPrefabAsset(ball, ballPath);
PrefabUtility.UnloadPrefabContents(ball);
sb.AppendLine("HandGrabPose an BowlingBall angelegt");

// ---------- 2) Anlauf auf 3 m kürzen ----------
const float approachL = 3f;
const string lanePath = "Assets/Bowling/Prefabs/BowlingLane.prefab";
var lane = PrefabUtility.LoadPrefabContents(lanePath);
var surf = lane.transform.Find("Approach/ApproachSurface");
var s = surf.localScale; var p = surf.localPosition;
surf.localScale = new Vector3(s.x, s.y, approachL);
surf.localPosition = new Vector3(p.x, p.y, -approachL / 2f);
PrefabUtility.SaveAsPrefabAsset(lane, lanePath);
PrefabUtility.UnloadPrefabContents(lane);
sb.AppendLine("Anlauf: z = -3 .. 0");

// ---------- 3) Rig, Ablage und Kugel in den neuen Anlauf ----------
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
foreach (var r in scene.GetRootGameObjects()) {
    if (r.name == "[BuildingBlock] Camera Rig") r.transform.position = new Vector3(0f, 0f, -1.5f);
    if (r.name == "BallRack") r.transform.position = new Vector3(1.0f, 0f, -1.5f);
    if (r.name == "BowlingBall") r.transform.position = new Vector3(1.0f, 0.911f, -1.5f);
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
sb.AppendLine("Rig, Ablage, Kugel bei z = -1,5");
return sb.ToString();
