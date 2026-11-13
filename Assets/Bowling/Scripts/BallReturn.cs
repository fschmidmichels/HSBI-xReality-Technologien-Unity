using System;
using System.Collections;
using UnityEngine;

// Hängt an BowlingLane/Zones/PitZone (Trigger). Kommt die Kugel im Pit an,
// wird der Wurf gezählt, sobald alle Pins zur Ruhe gekommen sind (spätestens nach maxEvaluationTime).
// Ohne GameManager liegt die Kugel schon nach returnDelay wieder auf der Ablage.
// Mit GameManager kommt sie erst nach der Auswertung zurück, damit nicht vorher erneut geworfen wird.
public class BallReturn : MonoBehaviour
{
    [Tooltip("LaneControls in der Szene, kennt die Kugel, ihre Startposition und die PinGroup.")]
    public LaneControls laneControls;

    [Tooltip("Wartezeit in Sekunden, bis die Kugel zurück auf der Ablage liegt (nur ohne GameManager " +
             "oder wenn die Kugel außerhalb eines Wurfs in den Pit fällt).")]
    public float returnDelay = 2f;

    [Tooltip("So lange (Sekunden) müssen alle Pins ununterbrochen ruhig sein, bevor der Wurf gezählt wird. " +
             "Größer als 1 s wählen, da Pin.CheckRotation nur einmal pro Sekunde prüft.")]
    public float settleDuration = 1.5f;

    [Tooltip("Höchstgeschwindigkeit in m/s, bis zu der ein Pin als ruhig gilt.")]
    public float pinSettleSpeed = 0.05f;

    [Tooltip("Höchste Winkelgeschwindigkeit in rad/s, bis zu der ein Pin als ruhig gilt.")]
    public float pinSettleAngularSpeed = 0.5f;

    [Tooltip("Maximale Wartezeit in Sekunden bis zur Auswertung, falls ein Pin nicht zur Ruhe kommt.")]
    public float maxEvaluationTime = 10f;

    private Coroutine evaluationRoutine;

    // Ohne GameManager zählt jeder Pit-Eintritt als Wurf. Mit GameManager nur nach BeginThrow().
    private bool externallyControlled;
    private bool throwScheduled;
    private bool throwAllowed;
    private bool returnScheduled;

    public event Action BallReachedPit;
    // Ergebnis des Wurfs (Strike, Spare, Miss, gefallene Pins), siehe ThrowResult.
    public event Action<ThrowResult> ThrowEvaluated;
    public event Action ThrowCancelled;

    protected void OnTriggerEnter(Collider other)
    {
        // Nur die Kugel, nicht die Pins, die ebenfalls in den Pit fallen.
        if (laneControls == null || laneControls.ball == null ||
            other.attachedRigidbody != laneControls.ball)
        {
            return;
        }

        // Eine gehaltene Kugel (z. B. in den Pit gegriffen) löst weder Wurf noch Rückholung aus.
        if (laneControls.IsBallHeld)
        {
            return;
        }

        if (!externallyControlled)
        {
            // throwScheduled: Eine Kugel, die im Pit hüpft, zählt nur einmal.
            if (!throwScheduled)
            {
                ScheduleThrowCompletion();
                BallReachedPit?.Invoke();
            }
            return;
        }

        if (!throwScheduled)
        {
            if (throwAllowed)
            {
                ScheduleThrowCompletion();
                BallReachedPit?.Invoke();
            }
            else if (!returnScheduled)
            {
                ScheduleBallReturn();
            }
        }
    }

    // Vom GameManager in Awake aufgerufen: Würfe zählen dann nur noch nach BeginThrow().
    public void UseExternalThrowControl()
    {
        externallyControlled = true;
    }

    // Vom GameManager beim Start eines neuen Wurfs aufgerufen.
    public void BeginThrow()
    {
        if (returnScheduled)
        {
            CancelInvoke(nameof(ReturnBall));
            returnScheduled = false;
        }

        throwAllowed = true;
    }

    // Von LaneControls (X-Taste), damit ein noch offener Wurf nicht im neuen Frame zählt.
    public void CancelPendingThrow()
    {
        CancelInvoke(nameof(ReturnBall));
        StopEvaluation();
        throwScheduled = false;
        throwAllowed = false;
        returnScheduled = false;
        ThrowCancelled?.Invoke();
    }

    // Vom GameManager aufgerufen, wenn die Kugel die PitZone nicht erreicht (liegt still oder Obergrenze erreicht).
    // Die Kugel wird nach der Auswertung zurückgelegt.
    public void EndThrowAfterTimeout()
    {
        if (!throwScheduled)
        {
            throwScheduled = true;
            throwAllowed = true;
            StartEvaluation();
        }
    }

    private void ScheduleThrowCompletion()
    {
        throwScheduled = true;
        if (!externallyControlled)
        {
            ScheduleBallReturn();
        }
        StartEvaluation();
    }

    private void StartEvaluation()
    {
        StopEvaluation();
        evaluationRoutine = StartCoroutine(EvaluateWhenPinsSettled());
    }

    private void StopEvaluation()
    {
        if (evaluationRoutine != null)
        {
            StopCoroutine(evaluationRoutine);
            evaluationRoutine = null;
        }
    }

    // Wartet, bis alle Pins settleDuration Sekunden am Stück ruhig sind. Ein Pin, der kurz
    // zwischen zwei Wackelbewegungen stillsteht, setzt die Ruhezeit also nicht durch.
    private IEnumerator EvaluateWhenPinsSettled()
    {
        float elapsed = 0f;
        float settledTime = 0f;
        PinGroup pinGroup = laneControls.pinGroup;

        while (elapsed < maxEvaluationTime)
        {
            yield return null;
            elapsed += Time.deltaTime;

            if (pinGroup.ArePinsSettled(pinSettleSpeed, pinSettleAngularSpeed))
            {
                settledTime += Time.deltaTime;
                if (settledTime >= settleDuration)
                {
                    break;
                }
            }
            else
            {
                settledTime = 0f;
            }
        }

        evaluationRoutine = null;
        EndThrow();
    }

    private void ScheduleBallReturn()
    {
        CancelInvoke(nameof(ReturnBall));
        returnScheduled = true;
        Invoke(nameof(ReturnBall), returnDelay);
    }

    protected void ReturnBall()
    {
        returnScheduled = false;
        // Wurde die Kugel inzwischen gegriffen, hat die Person sie schon: nicht aus der Hand nehmen.
        if (laneControls.IsBallHeld)
        {
            return;
        }
        laneControls.ReturnBall();
    }

    protected void EndThrow()
    {
        // RegisterThrow wertet vor dem Neuaufstellen aus und stellt nach dem letzten Wurf die Pins neu auf.
        ThrowResult result = laneControls.pinGroup.RegisterThrow();
        throwScheduled = false;
        throwAllowed = false;

        if (externallyControlled)
        {
            CancelInvoke(nameof(ReturnBall));
            ReturnBall();
        }

        ThrowEvaluated?.Invoke(result);
    }
}
