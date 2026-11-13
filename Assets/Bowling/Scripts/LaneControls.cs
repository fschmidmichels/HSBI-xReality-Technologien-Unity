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

    // Legt die Kugel zurück auf die Ablage, ohne Pins und Score zu ändern (auch für BallReturn).
    public void ReturnBall()
    {
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.position = ballStartPosition;
        ball.rotation = ballStartRotation;
        ball.transform.SetPositionAndRotation(ballStartPosition, ballStartRotation);
    }
}
