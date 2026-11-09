using UnityEngine;

// Hängt an BowlingLane/Zones/PitZone (Trigger). Kommt die Kugel im Pit an,
// wird sie nach kurzer Wartezeit zurück auf die Ablage gelegt und der Wurf gezählt.
public class BallReturn : MonoBehaviour
{
    [Tooltip("LaneControls in der Szene, kennt die Kugel, ihre Startposition und die PinGroup.")]
    public LaneControls laneControls;

    [Tooltip("Wartezeit in Sekunden, bis die Kugel zurück auf der Ablage liegt.")]
    public float returnDelay = 2f;

    [Tooltip("Wartezeit in Sekunden, bis der Wurf gezählt wird. Muss größer sein als Pin.toppleLife + 1 s, " +
             "sonst werden umfallende Pins beim Neuaufstellen nicht mehr gezählt.")]
    public float pinResetDelay = 5f;

    protected void OnTriggerEnter(Collider other)
    {
        // Nur die Kugel, nicht die Pins, die ebenfalls in den Pit fallen.
        if (other.attachedRigidbody == laneControls.ball)
        {
            // CancelInvoke: Eine Kugel, die im Pit hüpft, zählt nur einmal.
            CancelInvoke(nameof(ReturnBall));
            Invoke(nameof(ReturnBall), returnDelay);
            CancelInvoke(nameof(EndThrow));
            Invoke(nameof(EndThrow), pinResetDelay);
        }
    }

    // Von LaneControls (X-Taste), damit ein noch offener Wurf nicht im neuen Frame zählt.
    public void CancelPendingThrow()
    {
        CancelInvoke(nameof(EndThrow));
    }

    protected void ReturnBall()
    {
        laneControls.ReturnBall();
    }

    protected void EndThrow()
    {
        laneControls.pinGroup.RegisterThrow();
    }
}
