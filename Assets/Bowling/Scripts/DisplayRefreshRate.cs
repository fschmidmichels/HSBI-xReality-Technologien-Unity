using System.Collections;
using UnityEngine;

// Stellt die Bildwiederholrate der Quest ein (Standard der Quest: 72 Hz).
// Vorgehen nach Meta-Dokumentation "Set Display Refresh Rates":
// https://developers.meta.com/vr/documentation/unity/unity-set-disp-freq/
// Die Laufzeitumgebung lehnt Raten ab, die nicht in der Liste des Geräts stehen. Deshalb wird
// zuerst die Liste abgefragt und dann die gewünschte oder die nächst niedrigere verfügbare Rate gesetzt.
public class DisplayRefreshRate : MonoBehaviour
{
    [Tooltip("Gewünschte Bildwiederholrate in Hz. Höhere Raten verkürzen das Frame-Budget (90 Hz = 11,1 ms).")]
    [Min(60f)]
    public float targetRefreshRate = 90f;

    [Tooltip("So lange (Sekunden) wird nach dem Start auf die Liste der Raten gewartet. Sie ist erst nach dem Start der XR-Sitzung verfügbar.")]
    [Min(0f)]
    public float waitForListSeconds = 5f;

    protected void OnEnable()
    {
        // Meldet, wenn das System die Rate ändert, z. B. bei Überhitzung zurück auf 72 Hz.
        OVRManager.DisplayRefreshRateChanged += HandleRefreshRateChanged;
    }

    protected void OnDisable()
    {
        OVRManager.DisplayRefreshRateChanged -= HandleRefreshRateChanged;
    }

    protected IEnumerator Start()
    {
        // Liste der verfügbaren Raten abfragen. Im Editor ohne Headset bleibt sie leer.
        float[] available = OVRPlugin.systemDisplayFrequenciesAvailable;
        float waited = 0f;
        while (available.Length == 0 && waited < waitForListSeconds)
        {
            yield return null;
            waited += Time.unscaledDeltaTime;
            available = OVRPlugin.systemDisplayFrequenciesAvailable;
        }

        if (available.Length == 0)
        {
            Debug.Log("DisplayRefreshRate: Keine Liste verfügbarer Raten (z. B. im Editor ohne Headset), Rate bleibt unverändert.", this);
            yield break;
        }

        // Gewünschte Rate oder die nächst niedrigere verfügbare Rate suchen.
        float chosen = 0f;
        foreach (float rate in available)
        {
            if (rate <= targetRefreshRate + 0.5f && rate > chosen)
            {
                chosen = rate;
            }
        }

        if (chosen <= 0f)
        {
            Debug.LogWarning("DisplayRefreshRate: Keine verfügbare Rate bis " + targetRefreshRate + " Hz (verfügbar: " +
                             string.Join(", ", available) + " Hz), Rate bleibt unverändert.", this);
            yield break;
        }

        OVRPlugin.systemDisplayFrequency = chosen;
        Debug.Log("DisplayRefreshRate: verfügbar " + string.Join(", ", available) + " Hz, gewünscht " +
                  targetRefreshRate + " Hz, eingestellt " + chosen + " Hz.", this);
    }

    private void HandleRefreshRateChanged(float fromRefreshRate, float toRefreshRate)
    {
        Debug.Log("DisplayRefreshRate: Rate geändert von " + fromRefreshRate + " Hz auf " + toRefreshRate + " Hz.", this);
    }
}
