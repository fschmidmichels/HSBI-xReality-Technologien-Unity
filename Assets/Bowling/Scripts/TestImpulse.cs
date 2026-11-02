using UnityEngine;

public class TestImpulse : MonoBehaviour
{
    public Vector3 velocity = new Vector3(0f, 0f, 7f);
    public float delay = 3f;   // Wartezeit in Sekunden, damit der Wurf beobachtet werden kann

    void Start()
    {
        Invoke(nameof(Launch), delay);
    }

    void Launch()
    {
        GetComponent<Rigidbody>().linearVelocity = velocity;
    }
}
