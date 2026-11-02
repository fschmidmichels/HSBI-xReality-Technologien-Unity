using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallAudio : MonoBehaviour
{
    [Header("Rollen")]
    [Tooltip("AudioSource mit dem Roll-Loop (Loop an, Play On Awake an).")]
    public AudioSource rollSource;

    [Tooltip("Geschwindigkeit in m/s, bei der der Roll-Sound die volle Lautstärke erreicht.")]
    public float fullVolumeSpeed = 8f;

    [Tooltip("Unterhalb dieser Geschwindigkeit in m/s ist kein Rollen zu hören.")]
    public float minRollSpeed = 0.2f;

    public float maxRollVolume = 0.8f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.4f;

    [Header("Treffer")]
    [Tooltip("AudioSource für den Strike-Sound.")]
    public AudioSource hitSource;
    public AudioClip strikeClip;

    [Tooltip("Mindestabstand in Sekunden zwischen zwei Strike-Sounds.")]
    public float strikeCooldown = 2f;

    private Rigidbody body;
    private float lastGroundContact = -1f;
    private float lastStrike = -10f;

    protected void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    protected void Update()
    {
        // Rollen nur bei Bodenkontakt und nicht, während die Kugel gehalten wird (dann ist sie kinematisch).
        bool grounded = Time.time - lastGroundContact < 0.1f && !body.isKinematic;
        Vector3 velocity = body.linearVelocity;
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        float t = grounded && speed > minRollSpeed ? Mathf.Clamp01(speed / fullVolumeSpeed) : 0f;

        // Weich nachführen, damit es bei kurzen Hüpfern nicht knackt.
        rollSource.volume = Mathf.MoveTowards(rollSource.volume, t * maxRollVolume, Time.deltaTime * 4f);
        rollSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
    }

    protected void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody == null)
        {
            lastGroundContact = Time.time;
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null)
        {
            lastGroundContact = Time.time;
            return;
        }

        // Erster Kontakt mit einem Pin: einmal den Strike-Sound abspielen.
        if (collision.gameObject.GetComponent<Pin>() != null && Time.time - lastStrike > strikeCooldown)
        {
            lastStrike = Time.time;
            float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / fullVolumeSpeed + 0.3f);
            hitSource.PlayOneShot(strikeClip, volume);
        }
    }
}
