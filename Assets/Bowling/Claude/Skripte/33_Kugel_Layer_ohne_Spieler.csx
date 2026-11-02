// Eigener Layer für die Kugel, keine Kollision mit dem Spieler (PlayerController im Rig, Layer "Water").
var sb = new System.Text.StringBuilder();
const string layerName = "BowlingBall";

// 1) Layer anlegen (erster freier Benutzer-Layer ab 8)
var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
var layers = tagManager.FindProperty("layers");
int ballLayer = LayerMask.NameToLayer(layerName);
if (ballLayer < 0) {
    for (int i = 8; i < 32; i++) {
        if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) {
            layers.GetArrayElementAtIndex(i).stringValue = layerName;
            ballLayer = i;
            break;
        }
    }
    tagManager.ApplyModifiedPropertiesWithoutUndo();
}
sb.AppendLine("Layer " + layerName + " = " + ballLayer);

// 2) Kugel-Prefab samt Kindern auf den Layer setzen
const string ballPath = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var ball = PrefabUtility.LoadPrefabContents(ballPath);
foreach (var t in ball.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ballLayer;
PrefabUtility.SaveAsPrefabAsset(ball, ballPath);
PrefabUtility.UnloadPrefabContents(ball);

// 3) Keine Kollision zwischen Kugel und Spieler-Capsule
var player = GameObject.Find("[BuildingBlock] Camera Rig").GetComponentInChildren<Oculus.Interaction.Locomotion.CharacterController>(true);
int playerLayer = player.gameObject.layer;
Physics.IgnoreLayerCollision(ballLayer, playerLayer, true);
AssetDatabase.SaveAssets();
sb.AppendLine("Kollision " + layerName + " <-> " + LayerMask.LayerToName(playerLayer) + ": " + (Physics.GetIgnoreLayerCollision(ballLayer, playerLayer) ? "aus" : "an"));

// 4) Prüfen: Bodenmaske des Spielers enthält den Kugel-Layer nicht
int mask = new SerializedObject(player).FindProperty("_layerMask").intValue;
sb.AppendLine("Bodenmaske des Spielers (" + mask + ") enthält Kugel-Layer: " + ((mask & (1 << ballLayer)) != 0));
var sceneBall = GameObject.Find("BowlingBall");
sb.AppendLine("Kugel in der Szene auf Layer: " + LayerMask.LayerToName(sceneBall.layer) + ", Kollision Kugel <-> Default (Ablage, Bahn): " + (Physics.GetIgnoreLayerCollision(ballLayer, 0) ? "aus" : "an"));
return sb.ToString();
