// Einzeiler, die direkt mit `unity command eval --code '...'` ausgeführt wurden.
// Jeder Block ist ein eigener Aufruf; zum Ausführen jeweils einen Block in eine eigene Datei kopieren
// und mit `unity command eval_file --file <Datei>` starten.

// ---------- A) Root-Objekte der aktiven Szene auflisten ----------
var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); var r = s.path + ": "; foreach (var g in s.GetRootGameObjects()) r += g.name + "(" + g.activeSelf + "), "; return r;

// ---------- B) Grab-Interactors im Rig finden ----------
var r = ""; foreach (var t in new System.Type[]{typeof(Oculus.Interaction.HandGrab.HandGrabInteractor), typeof(Oculus.Interaction.GrabInteractor), typeof(Oculus.Interaction.DistanceGrabInteractor), typeof(Oculus.Interaction.HandGrab.DistanceHandGrabInteractor)}) { foreach (var c in UnityEngine.Object.FindObjectsByType(t, FindObjectsInactive.Include, FindObjectsSortMode.None)) r += t.Name + ": " + ((Component)c).gameObject.name + " active=" + ((Component)c).gameObject.activeInHierarchy + "\n"; } return r == "" ? "keine Interactors" : r;

// ---------- C) Werte des neuen Pin-Prefabs prüfen ----------
var p = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bowling/Prefabs/CollidableBowlingPin.prefab"); var rb = p.GetComponent<Rigidbody>(); return "mass=" + rb.mass + " autoCOM=" + rb.automaticCenterOfMass + " box=" + p.GetComponent<BoxCollider>().size.ToString("F4") + " Pin=" + (p.GetComponent<Pin>() != null);

// ---------- D) Schwerpunkt eines Pins in der Szene prüfen ----------
var rb = GameObject.Find("BowlingLane/Pins/Pin_1").GetComponent<Rigidbody>(); return "COM=" + rb.centerOfMass.ToString("F4") + " worldCOM=" + rb.worldCenterOfMass.ToString("F4");

// ---------- E) ControllerGrabSource näher an den Controller (0,15 -> 0,10 m) ----------
const string path = "Assets/Bowling/Prefabs/BowlingBall.prefab"; var root = PrefabUtility.LoadPrefabContents(path); root.transform.Find("GrabInteraction/ControllerGrabSource").localPosition = new Vector3(0f, 0.10f, 0f); PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root); AssetDatabase.SaveAssets(); return "ok";

// ---------- F) Manuelle Justierung des Nutzers aus der Szene ins Prefab übernehmen ----------
var b = GameObject.Find("BowlingBall");
PrefabUtility.ApplyObjectOverride(b.transform.Find("GrabInteraction/ControllerGrabSource"), "Assets/Bowling/Prefabs/BowlingBall.prefab", InteractionMode.AutomatedAction);
var pose = b.transform.Find("GrabInteraction/HandGrabPose");
PrefabUtility.ApplyObjectOverride(pose, "Assets/Bowling/Prefabs/BowlingBall.prefab", InteractionMode.AutomatedAction);
PrefabUtility.RevertPropertyOverride(new SerializedObject(pose.GetComponent<Oculus.Interaction.HandGrab.HandGrabPose>()).FindProperty("_handGhostProvider"), InteractionMode.AutomatedAction);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
return "ok";
