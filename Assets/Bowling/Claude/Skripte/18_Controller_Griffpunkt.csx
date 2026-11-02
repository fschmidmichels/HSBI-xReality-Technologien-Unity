const string path = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var root = PrefabUtility.LoadPrefabContents(path);
var interaction = root.transform.Find("GrabInteraction");

// 1) Hand-Tracking: Align On Grab wieder aktivieren
var hgi = interaction.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>();
var soH = new SerializedObject(hgi);
soH.FindProperty("_handAligment").enumValueIndex = (int)Oculus.Interaction.HandGrab.HandAlignType.AlignOnGrab;
soH.ApplyModifiedPropertiesWithoutUndo();

// 2) Controller: Griffpunkt auf der Kugel. Beim Greifen wird dieser Punkt (Position und Drehung)
//    auf den Griffpunkt des Controllers gezogen. Liegt er 0,15 m über der Kugelmitte (Seite der Grifflöcher),
//    hängt die Kugel mit den Löchern nach oben unter dem Controller, Fingerlöcher nach vorn.
//    Die Lage ist symmetrisch und gilt deshalb für beide Hände.
var old = interaction.Find("ControllerGrabSource");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var src = new GameObject("ControllerGrabSource").transform;
src.SetParent(interaction, false);
src.localPosition = new Vector3(0f, 0.15f, 0f);
src.localRotation = Quaternion.identity;

var gi = interaction.GetComponent<Oculus.Interaction.GrabInteractable>();
var soG = new SerializedObject(gi);
soG.FindProperty("_grabSource").objectReferenceValue = src;
soG.FindProperty("_useClosestPointAsGrabSource").boolValue = false;
soG.ApplyModifiedPropertiesWithoutUndo();

PrefabUtility.SaveAsPrefabAsset(root, path);
PrefabUtility.UnloadPrefabContents(root);
AssetDatabase.SaveAssets();

var inst = GameObject.Find("BowlingBall");
var instG = inst.GetComponentInChildren<Oculus.Interaction.GrabInteractable>();
var instH = inst.GetComponentInChildren<Oculus.Interaction.HandGrab.HandGrabInteractable>();
var gs = new SerializedObject(instG).FindProperty("_grabSource").objectReferenceValue as Transform;
return "Szene: HandAlignment=" + instH.HandAlignment + ", GrabSource=" + (gs ? gs.name + " " + gs.localPosition.ToString("F2") : "null");
