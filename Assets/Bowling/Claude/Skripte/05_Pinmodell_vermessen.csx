var sb = new System.Text.StringBuilder();
var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bowling/Models/BowlingPin.fbx");
foreach (var mf in model.GetComponentsInChildren<MeshFilter>(true)) {
    sb.AppendLine("Mesh " + mf.name + " bounds center=" + mf.sharedMesh.bounds.center.ToString("F4") + " size=" + mf.sharedMesh.bounds.size.ToString("F4") + " localScale=" + mf.transform.localScale + " rot=" + mf.transform.localEulerAngles);
}
var imp = (ModelImporter)AssetImporter.GetAtPath("Assets/Bowling/Models/BowlingPin.fbx");
sb.AppendLine("globalScale=" + imp.globalScale + " useFileScale=" + imp.useFileScale + " fileScale=" + imp.fileScale);
sb.AppendLine("Scripts in Assets/Scripts: " + string.Join(", ", System.IO.Directory.Exists("Assets/Scripts") ? System.IO.Directory.GetFiles("Assets/Scripts") : new string[0]));
return sb.ToString();
