var sb = new System.Text.StringBuilder();
sb.AppendLine("Build-Target: " + EditorUserBuildSettings.activeBuildTarget + ", aktive Qualitätsstufe: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
for (int i = 0; i < QualitySettings.names.Length; i++) {
    var rp = QualitySettings.GetRenderPipelineAssetAt(i) as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
    if (rp == null) { sb.AppendLine(QualitySettings.names[i] + ": kein URP-Asset"); continue; }
    sb.AppendLine(QualitySettings.names[i] + " -> " + rp.name
        + " | shadowDistance=" + rp.shadowDistance
        + " | mainLightShadowmapResolution=" + rp.mainLightShadowmapResolution
        + " | cascades=" + rp.shadowCascadeCount
        + " | softShadows=" + rp.supportsSoftShadows
        + " | mainLightShadows=" + rp.supportsMainLightShadows
        + " | depthBias=" + rp.shadowDepthBias + " normalBias=" + rp.shadowNormalBias
        + " | renderScale=" + rp.renderScale + " | MSAA=" + rp.msaaSampleCount);
}
foreach (var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
    sb.AppendLine("Licht " + l.name + ": type=" + l.type + " shadows=" + l.shadows + " rot=" + l.transform.eulerAngles + " resolution=" + l.shadowResolution + " bias=" + l.shadowBias + " normalBias=" + l.shadowNormalBias);
// Größe der Szene, die der Schatten abdecken muss
var b = new Bounds(Vector3.zero, Vector3.zero);
foreach (var r in GameObject.Find("BowlingLane").GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
sb.AppendLine("Bahn-Bounds: " + b.size.ToString("F1"));
return sb.ToString();
