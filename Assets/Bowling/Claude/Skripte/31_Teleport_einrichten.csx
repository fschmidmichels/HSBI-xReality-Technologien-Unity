// Installiert den Meta XR Building Block "Teleport" (Varianten NavMesh und Physics Layer Blocker) ohne Auswahlfenster,
// backt ein NavMesh nur für den Spielbereich hinter der Foul-Linie und stellt die Controller-Fortbewegung auf Teleport.
var sb = new System.Text.StringBuilder();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

const string blockPath = "Packages/com.meta.xr.sdk.interaction.ovr/Editor/Blocks/Teleport/Teleport.asset";
const string routinePath = "Packages/com.meta.xr.sdk.interaction.ovr/Editor/Blocks/Teleport/InstallationRoutines/Teleport Interactable.asset";
var block = AssetDatabase.LoadAssetAtPath<ScriptableObject>(blockPath);
var routine = AssetDatabase.LoadAssetAtPath<ScriptableObject>(routinePath);
if (block == null || routine == null) return "Block oder Routine nicht gefunden";

// Vorhandene Installation entfernen (Skript ist wiederholbar)
foreach (var r in scene.GetRootGameObjects())
    if (r.name == "Teleport NavMesh" || r.name == "Teleport Physics Blocker") UnityEngine.Object.DestroyImmediate(r);

System.Collections.Generic.List<GameObject> Install(string variantName) {
    var variantField = routine.GetType().GetField("variant", flags);
    var oldVariant = variantField.GetValue(routine);
    variantField.SetValue(routine, System.Enum.Parse(variantField.FieldType, variantName));
    System.Type t = block.GetType();
    System.Reflection.FieldInfo selField = null;
    while (t != null && selField == null) { selField = t.GetField("_selectedRoutine", flags); t = t.BaseType; }
    selField.SetValue(block, routine);
    System.Reflection.MethodInfo install = null;
    t = block.GetType();
    while (t != null && install == null) { install = t.GetMethod("InstallBlockAsync", flags); t = t.BaseType; }
    var task = (System.Threading.Tasks.Task<System.Collections.Generic.List<GameObject>>)install.Invoke(block, new object[] { null });
    selField.SetValue(block, null);
    variantField.SetValue(routine, oldVariant);       // Paket-Asset nicht dauerhaft verändern
    if (!task.IsCompleted) throw new System.Exception("Installation " + variantName + " nicht synchron abgeschlossen");
    return task.Result;
}

var navBlocks = Install("NavMesh");
sb.AppendLine("NavMesh-Variante: " + string.Join(", ", navBlocks.ConvertAll(g => g.name)));
var blockerBlocks = Install("PhysicsLayerBlocker");
sb.AppendLine("Physics-Blocker-Variante: " + string.Join(", ", blockerBlocks.ConvertAll(g => g.name)));

// ---------- Unity-NavMesh nur für den Spielbereich (z -8 .. 0), Bahn ab z = 0 bleibt ausgeschlossen ----------
GameObject navRoot = null;
foreach (var r in scene.GetRootGameObjects()) if (r.name == "Teleport NavMesh") navRoot = r;
var unitySurface = navRoot.GetComponent<Unity.AI.Navigation.NavMeshSurface>() ?? navRoot.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
navRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
unitySurface.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
unitySurface.center = new Vector3(0f, 0.5f, -4f);
unitySurface.size = new Vector3(8f, 2f, 8f);
unitySurface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
// Das Camera Rig (PlayerController-Capsule am Startpunkt) darf beim Backen kein Hindernis sein
var rigGo = GameObject.Find("[BuildingBlock] Camera Rig");
var rigMod = rigGo.GetComponent<Unity.AI.Navigation.NavMeshModifier>() ?? rigGo.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
rigMod.ignoreFromBuild = true;
rigMod.applyToChildren = true;
EditorUtility.SetDirty(rigMod);
unitySurface.BuildNavMesh();
const string navFolder = "Assets/Scenes/BowlingVR";
if (!AssetDatabase.IsValidFolder(navFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "BowlingVR");
const string navPath = navFolder + "/NavMesh-Teleport.asset";
AssetDatabase.DeleteAsset(navPath);
AssetDatabase.CreateAsset(unitySurface.navMeshData, navPath);
EditorUtility.SetDirty(unitySurface);
var tri = UnityEngine.AI.NavMesh.CalculateTriangulation();
var nb = new Bounds(tri.vertices.Length > 0 ? tri.vertices[0] : Vector3.zero, Vector3.zero);
foreach (var v in tri.vertices) nb.Encapsulate(v);
sb.AppendLine("NavMesh gebacken: " + tri.indices.Length / 3 + " Dreiecke, Bereich x " + nb.min.x.ToString("F2") + ".." + nb.max.x.ToString("F2") + ", z " + nb.min.z.ToString("F2") + ".." + nb.max.z.ToString("F2"));

// ---------- Controller-Fortbewegung: Teleport statt Slide ----------
var moving = GameObject.Find("[BuildingBlock] Camera Rig/[BuildingBlock] OVRComprehensiveInteractionRig/Locomotor/LocomotionSettings/Moving").GetComponent<Oculus.Interaction.Locomotion.MovingSetting>();
var so = new SerializedObject(moving);
so.FindProperty("_controllerMovement._value").enumValueIndex = (int)Oculus.Interaction.Locomotion.MovingSetting.MovementStyle.Teleport;
so.ApplyModifiedPropertiesWithoutUndo();
// Objekte im Editor passend schalten (zur Laufzeit übernimmt MovingSetting das selbst)
var tp = so.FindProperty("_teleportGameObjects"); var sl = so.FindProperty("_slideGameObjects");
for (int i = 0; i < sl.arraySize; i++) ((GameObject)sl.GetArrayElementAtIndex(i).objectReferenceValue)?.SetActive(false);
for (int i = 0; i < tp.arraySize; i++) ((GameObject)tp.GetArrayElementAtIndex(i).objectReferenceValue)?.SetActive(true);
sb.AppendLine("Controller-Fortbewegung: " + moving.ControllerMovement.Value + " (Teleport-Objekte " + tp.arraySize + ", Slide-Objekte " + sl.arraySize + ")");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
return sb.ToString();
