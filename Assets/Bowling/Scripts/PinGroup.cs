using System.Collections;
using UnityEngine;

public class PinGroup : MonoBehaviour
{
    [Tooltip("Würfe pro Frame, danach werden die Pins neu aufgestellt.")]
    public int throwsPerFrame = 2;

    [Tooltip("Höhe in Metern, aus der sich die Pins beim Neuaufstellen absenken.")]
    [Min(0f)]
    public float dropHeight = 1.0f;

    [Tooltip("Dauer des Absenkens in Sekunden. 0 = sofort aufstellen (ohne Animation).")]
    [Min(0f)]
    public float resetDuration = 1.0f;

    private Pin[] pins;
    private Rigidbody[] pinBodies;
    private Vector3[] positions;
    private Quaternion[] rotations;
    private int throwCount;
    private int pinsStandingBeforeThrow;
    private Coroutine resetRoutine;

    // Nummer des nächsten Wurfs im aktuellen Frame, beginnend bei 1 (z. B. für die Anzeige).
    public int CurrentThrow
    {
        get { return throwCount + 1; }
    }

    // True, solange die Pins abgesenkt werden. Der GameManager wartet darauf, bevor der nächste Wurf möglich ist.
    public bool IsResetting
    {
        get { return resetRoutine != null; }
    }

    protected void Awake()
    {
        pins = GetComponentsInChildren<Pin>();
        pinBodies = new Rigidbody[pins.Length];
        for (int index = 0; index < pins.Length; index++)
        {
            pinBodies[index] = pins[index].GetComponent<Rigidbody>();
        }
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

    // Stellt alle Pins neu auf. Mit resetDuration > 0 senken sie sich aus dropHeight ab.
    public void ResetPositions()
    {
        throwCount = 0;
        pinsStandingBeforeThrow = pins.Length;

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        if (resetDuration <= 0f || dropHeight <= 0f || !isActiveAndEnabled)
        {
            PlacePins(0f, false);
            return;
        }
        resetRoutine = StartCoroutine(AnimateReset());
    }

    // Während der Animation sind die Pins kinematisch: Die Physik bewegt sie nicht,
    // das Skript bewegt sie über MovePosition. Erst am Ziel werden sie wieder freigegeben.
    private IEnumerator AnimateReset()
    {
        PlacePins(dropHeight, true);

        float time = 0f;
        while (time < resetDuration)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            // SmoothStep: langsam anfahren, langsam abbremsen.
            float height = dropHeight * (1f - Mathf.SmoothStep(0f, 1f, time / resetDuration));
            for (int index = 0; index < pins.Length; index++)
            {
                pinBodies[index].MovePosition(positions[index] + Vector3.up * height);
            }
        }

        PlacePins(0f, false);
        resetRoutine = null;
    }

    // Aktiviert alle Pins, stoppt ihre Bewegung und setzt sie height Meter über ihre gespeicherte Position.
    private void PlacePins(float height, bool kinematic)
    {
        for (int index = 0; index < pins.Length; index++)
        {
            // Zuerst aktivieren, sonst wirken die Rigidbody-Werte bei ausgeblendeten Pins nicht.
            pins[index].gameObject.SetActive(true);
            pins[index].CancelToppleCheck();
            Rigidbody pinRigidbody = pinBodies[index];
            // Geschwindigkeiten nur an dynamischen Rigidbodies setzen (sonst Warnung in der Console).
            if (!pinRigidbody.isKinematic)
            {
                pinRigidbody.linearVelocity = Vector3.zero;
                pinRigidbody.angularVelocity = Vector3.zero;
            }
            pinRigidbody.isKinematic = kinematic;
            Vector3 target = positions[index] + Vector3.up * height;
            // Rigidbody und Transform setzen, damit die Interpolation die Pins nicht zurückzieht.
            pinRigidbody.position = target;
            pinRigidbody.rotation = rotations[index];
            pins[index].transform.SetPositionAndRotation(target, rotations[index]);
        }
    }
}
