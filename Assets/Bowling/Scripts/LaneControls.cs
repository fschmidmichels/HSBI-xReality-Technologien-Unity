using UnityEngine;

public class LaneControls : MonoBehaviour
{
    [Tooltip("Eltern-Objekt der Bumper (BowlingLane/Bumpers).")]
    public GameObject bumpers;

    [Tooltip("PinGroup an BowlingLane/Pins.")]
    public PinGroup pinGroup;

    [Tooltip("Rigidbody der Bowlingkugel.")]
    public Rigidbody ball;

    private Vector3 ballStartPosition;
    private Quaternion ballStartRotation;

    protected void Start()
    {
        ballStartPosition = ball.position;
        ballStartRotation = ball.rotation;
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
        pinGroup.ResetPositions();

        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.position = ballStartPosition;
        ball.rotation = ballStartRotation;
        ball.transform.SetPositionAndRotation(ballStartPosition, ballStartRotation);
    }
}
