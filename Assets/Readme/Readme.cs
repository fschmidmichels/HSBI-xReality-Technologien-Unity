using System;
using UnityEngine;

/// <summary>
/// Projekt-Readme, das im Inspector angezeigt wird (siehe Assets/Readme.asset).
/// Links ohne "http(s)://" werden als Pfad relativ zum Projektordner geöffnet.
/// </summary>
public class Readme : ScriptableObject
{
    public Texture2D icon;
    public string title;
    public Section[] sections;

    [Serializable]
    public class Section
    {
        public string heading, text, linkText, url;
    }
}
