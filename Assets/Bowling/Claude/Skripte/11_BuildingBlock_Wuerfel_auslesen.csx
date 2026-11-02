var sb = new System.Text.StringBuilder();
var extra = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.AdditiveWithoutLoading); extra = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Additive); GameObject cube = null; foreach (var g in extra.GetRootGameObjects()) if (g.name == "[BuildingBlock] Cube") cube = g;
void Dump(Transform t, string indent) {
    sb.AppendLine(indent + t.name + " pos=" + t.localPosition.ToString("F3") + " scale=" + t.localScale.ToString("F3") + " active=" + t.gameObject.activeSelf);
    foreach (var c in t.GetComponents<Component>()) {
        if (c == null) { sb.AppendLine(indent + "  - <missing>"); continue; }
        sb.AppendLine(indent + "  - " + c.GetType().FullName);
        if (c is MonoBehaviour) {
            var so = new SerializedObject(c);
            var it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter)) {
                enter = false;
                if (it.name == "m_Script") continue;
                string v = it.propertyType == SerializedPropertyType.ObjectReference ? (it.objectReferenceValue ? it.objectReferenceValue.name + " (" + it.objectReferenceValue.GetType().Name + ")" : "null")
                    : it.propertyType == SerializedPropertyType.Boolean ? it.boolValue.ToString()
                    : it.propertyType == SerializedPropertyType.Float ? it.floatValue.ToString()
                    : it.propertyType == SerializedPropertyType.Enum ? it.enumDisplayNames.Length > it.enumValueIndex && it.enumValueIndex >= 0 ? it.enumDisplayNames[it.enumValueIndex] : it.intValue.ToString()
                    : it.propertyType == SerializedPropertyType.Integer ? it.intValue.ToString()
                    : it.propertyType.ToString();
                sb.AppendLine(indent + "      " + it.name + " = " + v);
            }
        }
    }
    foreach (Transform ch in t) Dump(ch, indent + "  ");
}
Dump(cube.transform, "");
sb.AppendLine("Prefab source: " + PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cube));
UnityEditor.SceneManagement.EditorSceneManager.CloseScene(extra, true);
return sb.ToString();
