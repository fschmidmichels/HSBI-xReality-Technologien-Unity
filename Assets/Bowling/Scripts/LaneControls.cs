using System.Collections;
using Oculus.Interaction;
using UnityEngine;

public class LaneControls : MonoBehaviour
{
    // Abstand in Metern, bis zu dem die Kugel noch als "auf der Ablage" gilt.
    private const float StartPositionTolerance = 0.1f;

    [Tooltip("Eltern-Objekt der Bumper (BowlingLane/Bumpers).")]
    public GameObject bumpers;

    [Tooltip("PinGroup an BowlingLane/Pins.")]
    public PinGroup pinGroup;

    [Tooltip("Rigidbody der Bowlingkugel.")]
    public Rigidbody ball;

    [Tooltip("BallReturn an BowlingLane/Zones/PitZone (optional).")]
    public BallReturn ballReturn;

    [Tooltip("Startpunkt der Rückholanimation, z. B. verdeckt hinter der Ablage. Leer = Kugel sofort auf die Ablage setzen.")]
    public Transform ballReturnExit;

    [Tooltip("Dauer der Rückholanimation in Sekunden.")]
    [Min(0f)]
    public float ballReturnDuration = 0.8f;

    private Coroutine ballReturnRoutine;
    private bool ballReturning;
    private IInteractable[] ballInteractables;
    private float ballRadius = 0.109f;

    // True, solange die Kugel zur Ablage zurückrollt. Der GameManager wartet darauf.
    public bool IsBallReturning
    {
        get { return ballReturning; }
    }

    private Vector3 ballStartPosition;
    private Quaternion ballStartRotation;
    private Grabbable ballGrabbable;

    // True, solange eine Hand oder ein Controller die Kugel greift (ISDK Grabbable).
    public bool IsBallHeld
    {
        get { return ballGrabbable != null && ballGrabbable.SelectingPointsCount > 0; }
    }

    // True, wenn die Kugel (ungefähr) auf der Ablage liegt.
    public bool IsBallAtStart
    {
        get
        {
            return (ball.position - ballStartPosition).sqrMagnitude <=
                   StartPositionTolerance * StartPositionTolerance;
        }
    }

    protected void Start()
    {
        ballStartPosition = ball.position;
        ballStartRotation = ball.rotation;
        ballGrabbable = ball.GetComponent<Grabbable>();

        // Alle ISDK-Interactables der Kugel (Grab, HandGrab, DistanceGrab), um das Greifen während der Animation zu sperren.
        ballInteractables = ball.GetComponentsInChildren<IInteractable>(true);

        // Radius für die sichtbare Rollbewegung aus dem Collider der Kugel.
        SphereCollider sphere = ball.GetComponentInChildren<SphereCollider>();
        if (sphere != null)
        {
            Vector3 scale = sphere.transform.lossyScale;
            ballRadius = sphere.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        }
    }

    protected void Update()
    {
        // Button.Four = Y-Taste, Button.Three = X-Taste (beide auf dem linken Controller).
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            ToggleBumpers();
        }

        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            ResetBallAndPins();
        }
    }

    public void ToggleBumpers()
    {
        bumpers.SetActive(!bumpers.activeSelf);
    }

    public void ResetBallAndPins()
    {
        // Beendet den Frame wie nach zwei Würfen: Score in die Liste, Score 0, Pins neu.
        if (ballReturn != null)
        {
            ballReturn.CancelPendingThrow();
        }
        pinGroup.EndFrame();
        ReturnBall();
    }

    // Neues Spiel: Score und Frame-Liste löschen, Pins neu aufstellen, Kugel zurück.
    // Anders als ResetBallAndPins wird der laufende Frame nicht in die Liste eingetragen.
    public void ResetGame()
    {
        if (ballReturn != null)
        {
            ballReturn.CancelPendingThrow();
        }
        ScoreScript.ResetGame();
        pinGroup.ResetPositions();
        ReturnBall();
    }

    // Legt die Kugel zurück auf die Ablage, ohne Pins und Score zu ändern (auch für BallReturn).
    // Mit ballReturnExit rollt sie von dort aus sichtbar auf die Ablage.
    public void ReturnBall()
    {
        StopBallReturn();

        if (ballReturnExit == null || ballReturnDuration <= 0f || !isActiveAndEnabled)
        {
            PlaceBall(ballStartPosition, ballStartRotation);
            return;
        }
        ballReturnRoutine = StartCoroutine(AnimateBallReturn());
    }

    private IEnumerator AnimateBallReturn()
    {
        // Vor dem Sperren setzen: Wird die Kugel dabei aus der Hand genommen, meldet das ISDK einen Wurf.
        // Der GameManager ignoriert ihn anhand von IsBallReturning.
        ballReturning = true;
        // Greifen sperren: Während der Animation soll niemand die Kugel aus der Bewegung reißen.
        SetBallGrabbable(false);
        // LockKinematic statt isKinematic = true: Das Interaction SDK nutzt denselben Zähler beim Greifen,
        // so stellen sich beide nicht gegenseitig den kinematischen Zustand um.
        PlaceBall(ballReturnExit.position, ballStartRotation);
        ball.LockKinematic();

        Vector3 from = ballReturnExit.position;
        Vector3 path = ballStartPosition - from;
        // Drehachse senkrecht zur Bewegung auf dem Boden, damit die Kugel sichtbar rollt.
        Vector3 horizontal = Vector3.ProjectOnPlane(path, Vector3.up);
        Vector3 axis = horizontal.sqrMagnitude > 0.0001f ? Vector3.Cross(Vector3.up, horizontal.normalized) : Vector3.right;
        float totalAngle = path.magnitude / ballRadius * Mathf.Rad2Deg;

        float time = 0f;
        while (time < ballReturnDuration)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / ballReturnDuration);
            ball.MovePosition(Vector3.Lerp(from, ballStartPosition, t));
            ball.MoveRotation(Quaternion.AngleAxis(totalAngle * t, axis) * ballStartRotation);
        }

        ball.UnlockKinematic();
        PlaceBall(ballStartPosition, ballStartRotation);
        SetBallGrabbable(true);
        ballReturnRoutine = null;
        ballReturning = false;
    }

    // Bricht eine laufende Rückholanimation ab und gibt Kugel und Greifen wieder frei.
    private void StopBallReturn()
    {
        if (ballReturnRoutine == null)
        {
            return;
        }
        StopCoroutine(ballReturnRoutine);
        ballReturnRoutine = null;
        ball.UnlockKinematic();
        SetBallGrabbable(true);
        ballReturning = false;
    }

    private void PlaceBall(Vector3 position, Quaternion rotation)
    {
        // Geschwindigkeiten nur an dynamischen Rigidbodies setzen (sonst Warnung in der Console).
        if (!ball.isKinematic)
        {
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
        }
        ball.position = position;
        ball.rotation = rotation;
        ball.transform.SetPositionAndRotation(position, rotation);
    }

    private void SetBallGrabbable(bool grabbable)
    {
        if (ballInteractables == null)
        {
            return;
        }
        foreach (IInteractable interactable in ballInteractables)
        {
            if (grabbable)
            {
                interactable.Enable();
            }
            else
            {
                interactable.Disable();
            }
        }
    }
}
