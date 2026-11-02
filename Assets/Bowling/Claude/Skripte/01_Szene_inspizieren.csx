var sb = new System.Text.StringBuilder();
for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) {
  var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
  sb.AppendLine("Scene: " + s.path + " dirty=" + s.isDirty);
  foreach (var go in s.GetRootGameObjects())
    sb.AppendLine("  " + go.name + " active=" + go.activeSelf + " pos=" + go.transform.position + " rot=" + go.transform.eulerAngles + " scale=" + go.transform.localScale);
}
foreach (var cam in UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(UnityEngine.FindObjectsSortMode.None))
  sb.AppendLine("Camera " + cam.name + " clear=" + cam.clearFlags + " bg=" + cam.backgroundColor);
return sb.ToString();
