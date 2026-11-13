using TMPro;
using UnityEngine;

// Zeigt an, der wievielte Wurf im aktuellen Frame als Nächstes kommt, z. B. "Wurf 1 / 2".
// Der Text wird nur neu gesetzt, wenn sich die Wurfnummer ändert (keine Allokation pro Frame).
[RequireComponent(typeof(TextMeshProUGUI))]
public class ThrowDisplay : MonoBehaviour
{
    [Tooltip("PinGroup an BowlingLane/Pins, kennt die Wurfnummer im Frame.")]
    public PinGroup pinGroup;

    private TextMeshProUGUI label;
    private int shownThrow = -1;
    private int shownThrowsPerFrame = -1;

    protected void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    protected void Update()
    {
        if (pinGroup == null)
        {
            return;
        }

        int current = pinGroup.CurrentThrow;
        int perFrame = pinGroup.throwsPerFrame;
        if (current != shownThrow || perFrame != shownThrowsPerFrame)
        {
            shownThrow = current;
            shownThrowsPerFrame = perFrame;
            label.text = "Wurf " + current + " / " + perFrame;
        }
    }
}
