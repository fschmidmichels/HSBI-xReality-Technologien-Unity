using UnityEngine;
using TMPro;

// Zeigt die Scores der letzten Frames aus ScoreScript.history in zwei Spalten.
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreHistory : MonoBehaviour
{
    [Tooltip("Wie viele Frames angezeigt werden (die Hälfte pro Spalte).")]
    public int maxEntries = 10;

    private TextMeshProUGUI list;

    protected void Start()
    {
        list = GetComponent<TextMeshProUGUI>();
    }

    protected void Update()
    {
        int count = ScoreScript.history.Count;
        int first = Mathf.Max(0, count - maxEntries);   // ältere Frames fallen heraus
        int rows = (maxEntries + 1) / 2;

        string text = "Frames";
        for (int row = 0; row < rows; row++)
        {
            int left = first + row;
            int right = first + rows + row;
            text += "\n";
            if (left < count)
            {
                text += (left + 1) + ": " + ScoreScript.history[left];
            }
            if (right < count)
            {
                // <pos=50%> setzt die zweite Spalte in die Mitte des Textfelds.
                text += "<pos=50%>" + (right + 1) + ": " + ScoreScript.history[right];
            }
        }
        list.text = text;
    }
}
