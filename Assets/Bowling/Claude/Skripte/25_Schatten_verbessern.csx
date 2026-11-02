var sb = new System.Text.StringBuilder();
for (int i = 0; i < QualitySettings.names.Length; i++) {
    var rp = QualitySettings.GetRenderPipelineAssetAt(i) as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
    if (rp == null) continue;
    var so = new SerializedObject(rp);
    // 1) Schattenbereich auf die Bahn begrenzen (Spieler bei z = -1,5, Pins bis z = 19,2)
    so.FindProperty("m_ShadowDistance").floatValue = 25f;
    if (rp.name.StartsWith("Mobile")) {
        // 2) Zwei Cascades: Nahbereich (Ablage, Kugel in der Hand) bekommt eigene, schärfere Shadow Map
        so.FindProperty("m_ShadowCascadeCount").intValue = 2;
        so.FindProperty("m_Cascade2Split").floatValue = 0.25f;   // erste Cascade bis ca. 6 m
        // 3) Weiche Schatten, niedrigste Qualitätsstufe wegen der Quest-GPU
        so.FindProperty("m_SoftShadowsSupported").boolValue = true;
        so.FindProperty("m_SoftShadowQuality").intValue = 1;     // 1 = Low
    }
    so.ApplyModifiedPropertiesWithoutUndo();
    EditorUtility.SetDirty(rp);
    sb.AppendLine(QualitySettings.names[i] + " (" + rp.name + "): distance=" + rp.shadowDistance + " cascades=" + rp.shadowCascadeCount
        + " split2=" + rp.cascade2Split + " soft=" + rp.supportsSoftShadows + " softQuality=" + so.FindProperty("m_SoftShadowQuality").intValue
        + " resolution=" + rp.mainLightShadowmapResolution);
}
AssetDatabase.SaveAssets();

// 4) Licht steiler: 70° statt 50°, Richtung (Y = 330°) bleibt
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
var light = GameObject.Find("Directional Light");
var e = light.transform.eulerAngles;
light.transform.rotation = Quaternion.Euler(70f, e.y, e.z);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
sb.AppendLine("Licht: " + light.transform.eulerAngles.ToString("F0"));
return sb.ToString();
