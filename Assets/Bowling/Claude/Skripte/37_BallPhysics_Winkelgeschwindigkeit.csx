// Hängt BallPhysics (maxAngularVelocity, Standard 100 rad/s) an das Kugel-Prefab und an den TestBall.
var sb = new System.Text.StringBuilder();
const string path = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var root = PrefabUtility.LoadPrefabContents(path);
if (root.GetComponent<BallPhysics>() == null) root.AddComponent<BallPhysics>();
PrefabUtility.SaveAsPrefabAsset(root, path);
PrefabUtility.UnloadPrefabContents(root);
sb.AppendLine("BowlingBall.prefab: BallPhysics = " + AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<BallPhysics>().maxAngularVelocity);
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
foreach (var g in scene.GetRootGameObjects()) {
    if (g.name != "TestBall") continue;
    if (g.GetComponent<BallPhysics>() == null) g.AddComponent<BallPhysics>();
    sb.AppendLine("TestBall: BallPhysics = " + g.GetComponent<BallPhysics>().maxAngularVelocity + ", aktiv=" + g.activeSelf);
}
sb.AppendLine("Kugel in der Szene: " + (GameObject.Find("BowlingBall").GetComponent<BallPhysics>() != null));
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
return sb.ToString();
