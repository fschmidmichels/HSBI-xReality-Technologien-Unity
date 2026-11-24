using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
    const string k_ShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
    const float k_Space = 16f;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    // Wählt das Readme einmal pro Editor-Sitzung automatisch aus, damit es im Inspector erscheint.
    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(k_ShowedReadmeSessionStateName, false))
        {
            SelectReadme();
            SessionState.SetBool(k_ShowedReadmeSessionStateName, true);
        }
    }

    static Readme SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("Readme t:Readme");
        if (ids.Length != 1)
        {
            Debug.Log("Couldn't find a readme");
            return null;
        }

        var readme = AssetDatabase.LoadAssetAtPath<Readme>(AssetDatabase.GUIDToAssetPath(ids[0]));
        Selection.objects = new Object[] { readme };
        return readme;
    }

    static void OpenLink(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            Application.OpenURL(url);
            return;
        }

        // Pfad relativ zum Projektordner (dem Ordner mit Assets/)
        var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", url));
        if (File.Exists(path))
            EditorUtility.OpenWithDefaultApp(path);
        else
            Debug.LogWarning($"Readme: Datei nicht gefunden: {path}");
    }

    protected override void OnHeaderGUI()
    {
        var readme = (Readme)target;
        Init();

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            if (readme.icon != null)
            {
                GUILayout.Space(k_Space);
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            }
            GUILayout.Space(k_Space);
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(readme.title, m_TitleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        var readme = (Readme)target;
        Init();

        if (readme.sections == null)
            return;

        foreach (var section in readme.sections)
        {
            if (!string.IsNullOrEmpty(section.heading))
                GUILayout.Label(section.heading, m_HeadingStyle);

            if (!string.IsNullOrEmpty(section.text))
                GUILayout.Label(section.text, m_BodyStyle);

            if (!string.IsNullOrEmpty(section.linkText) && LinkLabel(new GUIContent(section.linkText)))
                OpenLink(section.url);

            GUILayout.Space(k_Space);
        }
    }

    bool m_Initialized;
    GUIStyle m_LinkStyle;
    GUIStyle m_TitleStyle;
    GUIStyle m_HeadingStyle;
    GUIStyle m_BodyStyle;

    void Init()
    {
        if (m_Initialized)
            return;

        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;
        m_BodyStyle.richText = true;

        m_TitleStyle = new GUIStyle(m_BodyStyle);
        m_TitleStyle.fontSize = 26;

        m_HeadingStyle = new GUIStyle(m_BodyStyle);
        m_HeadingStyle.fontStyle = FontStyle.Bold;
        m_HeadingStyle.fontSize = 18;

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;
        // Auswahlfarbe, gut lesbar im hellen und dunklen Editor-Theme
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

        m_Initialized = true;
    }

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, m_LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = m_LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, m_LinkStyle);
    }
}
