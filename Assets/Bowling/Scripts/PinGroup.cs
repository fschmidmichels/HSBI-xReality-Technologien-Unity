using UnityEngine;

public class PinGroup : MonoBehaviour
{
    [Tooltip("Würfe pro Frame, danach werden die Pins neu aufgestellt.")]
    public int throwsPerFrame = 2;

    private Pin[] pins;
    private Vector3[] positions;
    private Quaternion[] rotations;
    private int throwCount;
    private int pinsStandingBeforeThrow;

    // Nummer des nächsten Wurfs im aktuellen Frame, beginnend bei 1 (z. B. für die Anzeige).
    public int CurrentThrow
    {
        get { return throwCount + 1; }
    }

    protected void Awake()
    {
        pins = GetComponentsInChildren<Pin>();
        pinsStandingBeforeThrow = pins.Length;
        SavePositions();
    }

    public void SavePositions()
    {
        positions = new Vector3[pins.Length];
        rotations = new Quaternion[pins.Length];
        for (int index = 0; index < pins.Length; index++)
        {
            positions[index] = pins[index].transform.position;
            rotations[index] = pins[index].transform.rotation;
        }
    }

    // Anzahl der Pins, die noch nicht ausgeblendet sind.
    public int StandingPins()
    {
        int count = 0;
        foreach (Pin pin in pins)
        {
            if (pin.gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }

    // True, wenn alle Pins ruhig sind (siehe Pin.IsSettled). Von BallReturn vor der Auswertung abgefragt.
    public bool ArePinsSettled(float maxLinearSpeed, float maxAngularSpeed)
    {
        foreach (Pin pin in pins)
        {
            if (!pin.IsSettled(maxLinearSpeed, maxAngularSpeed))
            {
                return false;
            }
        }
        return true;
    }

    // Wird von BallReturn nach jedem Wurf aufgerufen (sobald alle Pins ruhig sind).
    // Liefert das Ergebnis des Wurfs, bevor die Pins gegebenenfalls neu aufgestellt werden.
    public ThrowResult RegisterThrow()
    {
        throwCount++;
        int standing = StandingPins();
        ThrowResult result = new ThrowResult(throwCount, pinsStandingBeforeThrow - standing, standing);
        pinsStandingBeforeThrow = standing;

        // Nach zwei Würfen oder bei einem Strike (alle Pins gefallen) ist der Frame vorbei.
        if (throwCount >= throwsPerFrame || standing == 0)
        {
            EndFrame();
        }
        return result;
    }

    // Score in die Liste eintragen, Score auf 0, Pins neu aufstellen. Auch für die X-Taste.
    public void EndFrame()
    {
        // Leere Frames (X-Taste ohne Wurf) nicht eintragen.
        if (throwCount > 0 || ScoreScript.scoreValue > 0)
        {
            ScoreScript.EndFrame();
        }
        ResetPositions();
    }

    public void ResetPositions()
    {
        throwCount = 0;
        pinsStandingBeforeThrow = pins.Length;
        for (int index = 0; index < pins.Length; index++)
        {
            // Zuerst aktivieren, sonst wirken die Rigidbody-Werte bei ausgeblendeten Pins nicht.
            pins[index].gameObject.SetActive(true);
            pins[index].CancelToppleCheck();
            Rigidbody pinRigidbody = pins[index].GetComponent<Rigidbody>();
            pinRigidbody.linearVelocity = Vector3.zero;
            pinRigidbody.angularVelocity = Vector3.zero;
            // Rigidbody und Transform setzen, damit die Interpolation die Pins nicht zurückzieht.
            pinRigidbody.position = positions[index];
            pinRigidbody.rotation = rotations[index];
            pins[index].transform.SetPositionAndRotation(positions[index], rotations[index]);
        }
    }
}
