// Woche 5: Die beiden AudioSources der Kugel (Rollen, Treffer) auf zwei Kindobjekte verteilen.
// Grund: MetaXRAudioSource steuert nur die erste AudioSource eines Objekts (GetComponent<AudioSource>()).
// Wiederholbar: Sind die Kindobjekte schon vorhanden, wird nichts geändert.
const string path = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
var sb = new System.Text.StringBuilder();
try
{
    var ballAudio = root.GetComponent<BallAudio>();

    System.Func<string, UnityEngine.AudioSource, UnityEngine.AudioSource> move = (name, old) =>
    {
        if (old == null || old.gameObject != root) { sb.AppendLine(name + ": bereits umgebaut"); return old; }
        var child = new UnityEngine.GameObject(name);
        child.transform.SetParent(root.transform, false);
        child.layer = root.layer;
        var created = child.AddComponent<UnityEngine.AudioSource>();
        UnityEditor.EditorUtility.CopySerialized(old, created);
        UnityEngine.Object.DestroyImmediate(old);
        sb.AppendLine(name + ": AudioSource auf Kindobjekt verschoben");
        return created;
    };

    ballAudio.rollSource = move("RollAudio", ballAudio.rollSource);
    ballAudio.hitSource = move("HitAudio", ballAudio.hitSource);

    UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
    sb.AppendLine("AudioSources am Wurzelobjekt: " + root.GetComponents<UnityEngine.AudioSource>().Length);
}
finally
{
    UnityEditor.PrefabUtility.UnloadPrefabContents(root);
}
return sb.ToString();
