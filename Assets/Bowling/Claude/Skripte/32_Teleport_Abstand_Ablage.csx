// Verkleinert den Agent-Radius (Abstand des NavMesh zu Hindernissen) und backt das Teleport-NavMesh neu.
const float radius = 0.2f;
var sb = new System.Text.StringBuilder();
var navAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0];
var so = new SerializedObject(navAsset);
var settings = so.FindProperty("m_Settings");
for (int i = 0; i < settings.arraySize; i++) {
    var s = settings.GetArrayElementAtIndex(i);
    if (s.FindPropertyRelative("agentTypeID").intValue != 0) continue;   // Humanoid
    sb.AppendLine("Humanoid: Radius " + s.FindPropertyRelative("agentRadius").floatValue + " -> " + radius);
    s.FindPropertyRelative("agentRadius").floatValue = radius;
}
so.ApplyModifiedPropertiesWithoutUndo();
AssetDatabase.SaveAssets();

var surf = GameObject.Find("Teleport NavMesh").GetComponent<Unity.AI.Navigation.NavMeshSurface>();
var old = surf.navMeshData;
surf.RemoveData();
surf.BuildNavMesh();
const string navPath = "Assets/Scenes/BowlingVR/NavMesh-Teleport.asset";
AssetDatabase.DeleteAsset(navPath);
AssetDatabase.CreateAsset(surf.navMeshData, navPath);
EditorUtility.SetDirty(surf);
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

// Prüfpunkte rund um die Ablage (x 0,85..1,15, z -1,8..-1,2) und auf der Bahn
var pts = new (string, Vector3)[] {
    ("Startpunkt (0 / -1,5)", new Vector3(0f, 0f, -1.5f)),
    ("0,4 m links der Ablage (0,45 / -1,5)", new Vector3(0.45f, 0f, -1.5f)),
    ("0,25 m links der Ablage (0,60 / -1,5)", new Vector3(0.60f, 0f, -1.5f)),
    ("0,1 m links der Ablage (0,75 / -1,5)", new Vector3(0.75f, 0f, -1.5f)),
    ("Ablage selbst (1,0 / -1,5)", new Vector3(1.0f, 0f, -1.5f)),
    ("0,3 m hinter der Ablage (1,0 / -2,1)", new Vector3(1.0f, 0f, -2.1f)),
    ("Bahn (0 / 1)", new Vector3(0f, 0f, 1f)),
};
foreach (var (n, p) in pts)
    sb.AppendLine(n + ": " + (UnityEngine.AI.NavMesh.SamplePosition(p, out var hit, 0.05f, UnityEngine.AI.NavMesh.AllAreas) ? "Teleport möglich" : "gesperrt"));
return sb.ToString();
