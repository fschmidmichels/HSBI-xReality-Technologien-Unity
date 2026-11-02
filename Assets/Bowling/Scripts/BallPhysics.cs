using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    [Tooltip("Maximale Winkelgeschwindigkeit in rad/s. Unity begrenzt standardmäßig auf 50 rad/s; " +
             "für reines Rollen braucht die Kugel (r = 0,109 m) bei 8 m/s etwa 73 rad/s.")]
    public float maxAngularVelocity = 100f;

    private Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.maxAngularVelocity = maxAngularVelocity;
    }

    // Änderungen im Inspector werden auch während des Play Mode sofort übernommen.
    void OnValidate()
    {
        if (body != null)
        {
            body.maxAngularVelocity = maxAngularVelocity;
        }
    }
}
