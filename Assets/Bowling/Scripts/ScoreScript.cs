using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreScript : MonoBehaviour
{
    // Statisch, damit jeder Pin ohne Referenz hochzählen kann (Pin.HidePin).
    public static int scoreValue = 0;

    // Scores aller beendeten Frames, angezeigt von ScoreHistory.
    public static List<int> history = new List<int>();

    private TextMeshProUGUI score;

    protected void Awake()
    {
        // Statische Werte überleben das Neuladen der Szene (SceneResetter), deshalb hier zurücksetzen.
        scoreValue = 0;
        history.Clear();
    }

    protected void Start()
    {
        score = GetComponent<TextMeshProUGUI>();
    }

    protected void Update()
    {
        score.text = "Score: " + scoreValue;
    }

    // Wird von PinGroup.EndFrame aufgerufen: aktuellen Score in die Liste, dann auf 0.
    public static void EndFrame()
    {
        history.Add(scoreValue);
        scoreValue = 0;
    }

    public static void ResetScore()
    {
        scoreValue = 0;
    }

    // Neues Spiel: aktuellen Score und Liste der Frames löschen.
    public static void ResetGame()
    {
        scoreValue = 0;
        history.Clear();
    }
}
