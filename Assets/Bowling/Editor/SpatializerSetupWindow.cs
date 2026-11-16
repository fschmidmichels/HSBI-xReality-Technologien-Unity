using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-Fenster: listet alle AudioSources der geöffneten Szenen auf und versieht die
// ausgewählten mit dem Skript MetaXRAudioSource des Meta XR Audio Spatializers.
// Das Häkchen "Spatialize" der AudioSource wird weder gelesen noch geändert:
// MetaXRAudioSource setzt es zur Laufzeit selbst (Option "Enable Spatialization").
// Aufruf über das Menü: Tools > XRT > Meta Spatializer einrichten
public class SpatializerSetupWindow : EditorWindow
{
    private const string MetaSpatializerName = "Meta XR Audio";

    private readonly List<AudioSource> sources = new List<AudioSource>();
    private readonly HashSet<AudioSource> selected = new HashSet<AudioSource>();

    private Vector2 scroll;
    private bool setSpatialBlend = true;
    private bool addExperimentalFeatures;

    [MenuItem("Tools/XRT/Meta Spatializer einrichten")]
    private static void Open()
    {
        GetWindow<SpatializerSetupWindow>("Meta Spatializer").Refresh();
    }

    private void OnEnable()
    {
        Refresh();
        Undo.undoRedoPerformed += Repaint;
        EditorApplication.hierarchyChanged += Refresh;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
        EditorApplication.hierarchyChanged -= Refresh;
    }

    // Sucht alle AudioSources der geöffneten Szenen, auch an inaktiven Objekten.
    private void Refresh()
    {
        sources.Clear();
        sources.AddRange(FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        sources.Sort((a, b) => string.CompareOrdinal(GetPath(a), GetPath(b)));
        selected.RemoveWhere(s => s == null || !sources.Contains(s));
        Repaint();
    }

    private void OnGUI()
    {
        DrawProjectCheck();

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Aktualisieren", EditorStyles.toolbarButton)) Refresh();
            if (GUILayout.Button("Alle", EditorStyles.toolbarButton)) selected.UnionWith(sources);
            if (GUILayout.Button("Keine", EditorStyles.toolbarButton)) selected.Clear();
            if (GUILayout.Button("Nur ohne Meta-Skript", EditorStyles.toolbarButton))
            {
                selected.Clear();
                foreach (AudioSource s in sources)
                {
                    if (!IsSpatialized(s)) selected.Add(s);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(selected.Count + " von " + sources.Count + " ausgewählt", EditorStyles.miniLabel);
        }

        DrawHeader();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (AudioSource source in sources)
        {
            if (source != null) DrawRow(source);
        }
        if (sources.Count == 0)
        {
            EditorGUILayout.HelpBox("Keine AudioSource in den geöffneten Szenen gefunden.", MessageType.Info);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        setSpatialBlend = EditorGUILayout.ToggleLeft("Spatial Blend auf 1 (3D) setzen", setSpatialBlend);
        addExperimentalFeatures = EditorGUILayout.ToggleLeft(
            "Zusätzlich MetaXRAudioSourceExperimentalFeatures hinzufügen", addExperimentalFeatures);

        using (new EditorGUI.DisabledScope(selected.Count == 0))
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Spatializer auf Auswahl anwenden", GUILayout.Height(28f))) Apply();
            if (GUILayout.Button("Von Auswahl entfernen", GUILayout.Height(28f), GUILayout.Width(170f))) Remove();
        }

        EditorGUILayout.HelpBox(
            "MetaXRAudioSource schaltet den Spatializer zur Laufzeit selbst ein (Option „Enable Spatialization“); " +
            "das Häkchen „Spatialize“ der AudioSource wird hier nicht verändert. " +
            "Der Spatializer erwartet Mono-Clips (Import-Einstellung „Force To Mono“). " +
            "Bei Prefab-Instanzen entstehen Overrides in der Szene; das Prefab selbst bleibt unverändert. " +
            "Hat ein Objekt mehrere AudioSources, steuert das Meta-Skript nur die erste; weitere werden übersprungen. " +
            "Direkt nach dem Anwenden oder Entfernen lässt sich der Schritt mit Strg+Z zurücknehmen; " +
            "später (z. B. nach dem Schließen der Szene oder einem Neustart des Editors) nur noch über „Von Auswahl entfernen“.",
            MessageType.None);
    }

    private static void DrawProjectCheck()
    {
        string plugin = AudioSettings.GetSpatializerPluginName();
        if (plugin != MetaSpatializerName)
        {
            EditorGUILayout.HelpBox(
                "Spatializer Plugin ist „" + (string.IsNullOrEmpty(plugin) ? "None" : plugin) + "“. " +
                "Unter Edit > Project Settings > Audio „" + MetaSpatializerName + "“ auswählen, sonst hat das Meta-Skript keine Wirkung.",
                MessageType.Warning);
            if (GUILayout.Button("Jetzt auf „" + MetaSpatializerName + "“ stellen"))
            {
                AudioSettings.SetSpatializerPluginName(MetaSpatializerName);
            }
        }
    }

    private static void DrawHeader()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(22f);
            GUILayout.Label("AudioSource (Pfad)", EditorStyles.miniBoldLabel, GUILayout.MinWidth(160f));
            GUILayout.Label("Clip", EditorStyles.miniBoldLabel, GUILayout.Width(150f));
            GUILayout.Label("Blend", EditorStyles.miniBoldLabel, GUILayout.Width(40f));
            GUILayout.Label("Meta-Skript", EditorStyles.miniBoldLabel, GUILayout.Width(100f));
        }
    }

    private void DrawRow(AudioSource source)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            bool isSelected = selected.Contains(source);
            bool toggled = EditorGUILayout.Toggle(isSelected, GUILayout.Width(18f));
            if (toggled != isSelected)
            {
                if (toggled) selected.Add(source); else selected.Remove(source);
            }

            // Klick auf den Namen wählt das Objekt in der Hierarchy aus.
            if (GUILayout.Button(new GUIContent(GetPath(source), GetPath(source)), EditorStyles.label, GUILayout.MinWidth(160f)))
            {
                Selection.activeGameObject = source.gameObject;
                EditorGUIUtility.PingObject(source.gameObject);
            }

            GUILayout.Label(ClipLabel(source), GUILayout.Width(150f));
            GUILayout.Label(source.spatialBlend.ToString("0.##"), GUILayout.Width(40f));
            GUILayout.Label(MetaLabel(source), GUILayout.Width(100f));
        }
    }

    private void Apply()
    {
        Undo.SetCurrentGroupName("Meta Spatializer anwenden");
        int group = Undo.GetCurrentGroup();
        int skipped = 0;

        foreach (AudioSource source in selected)
        {
            if (source == null) continue;
            if (!IsFirstSource(source))
            {
                skipped++;
                continue;
            }

            if (setSpatialBlend)
            {
                Undo.RecordObject(source, "Meta Spatializer anwenden");
                source.spatialBlend = 1f;
                PrefabUtility.RecordPrefabInstancePropertyModifications(source);
            }

            GameObject go = source.gameObject;
            if (go.GetComponent<MetaXRAudioSource>() == null)
            {
                Undo.AddComponent<MetaXRAudioSource>(go);
            }
            if (addExperimentalFeatures && go.GetComponent<MetaXRAudioSourceExperimentalFeatures>() == null)
            {
                Undo.AddComponent<MetaXRAudioSourceExperimentalFeatures>(go);
            }
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        Undo.CollapseUndoOperations(group);
        if (skipped > 0)
        {
            Debug.LogWarning("Meta Spatializer: " + skipped + " AudioSource(s) übersprungen, weil sie nicht die erste " +
                "AudioSource ihres Objekts sind. MetaXRAudioSource steuert nur die erste. " +
                "Abhilfe: jede AudioSource auf ein eigenes Kindobjekt legen.");
        }
        Repaint();
    }

    private void Remove()
    {
        Undo.SetCurrentGroupName("Meta Spatializer entfernen");
        int group = Undo.GetCurrentGroup();

        foreach (AudioSource source in selected)
        {
            if (source == null || !IsFirstSource(source)) continue;

            GameObject go = source.gameObject;
            // Reihenfolge wichtig: ExperimentalFeatures setzt MetaXRAudioSource voraus.
            MetaXRAudioSourceExperimentalFeatures experimental = go.GetComponent<MetaXRAudioSourceExperimentalFeatures>();
            if (experimental != null) Undo.DestroyObjectImmediate(experimental);
            MetaXRAudioSource meta = go.GetComponent<MetaXRAudioSource>();
            if (meta != null) Undo.DestroyObjectImmediate(meta);
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        Undo.CollapseUndoOperations(group);
        Repaint();
    }

    private static bool IsSpatialized(AudioSource source)
    {
        return IsFirstSource(source) && source.GetComponent<MetaXRAudioSource>() != null;
    }

    // MetaXRAudioSource holt seine Quelle mit GetComponent<AudioSource>() und steuert
    // deshalb nur die erste AudioSource eines Objekts.
    private static bool IsFirstSource(AudioSource source)
    {
        return source.GetComponent<AudioSource>() == source;
    }

    private static GUIContent MetaLabel(AudioSource source)
    {
        if (!IsFirstSource(source))
        {
            return new GUIContent("⚠ nicht möglich",
                "Weitere AudioSource am selben Objekt. MetaXRAudioSource steuert nur die erste. " +
                "Abhilfe: diese AudioSource auf ein eigenes Kindobjekt legen.");
        }
        return new GUIContent(source.GetComponent<MetaXRAudioSource>() != null ? "ja" : "–");
    }

    private static string ClipLabel(AudioSource source)
    {
        AudioClip clip = source.clip;
        if (clip == null) return "– (per Skript)";
        return clip.name + (clip.channels > 1 ? "  ⚠ Stereo" : "");
    }

    private static string GetPath(AudioSource source)
    {
        Transform t = source.transform;
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
