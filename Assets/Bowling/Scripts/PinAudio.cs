using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PinAudio : MonoBehaviour
{
    [Tooltip("Einzelne Anschläge Pin gegen Pin, es wird zufällig einer gewählt.")]
    public AudioClip[] hitClips;

    [Tooltip("Aufprallgeschwindigkeit in m/s, ab der ein Sound zu hören ist (verhindert Geräusche beim Aufstellen).")]
    public float minImpactSpeed = 0.3f;

    [Tooltip("Aufprallgeschwindigkeit in m/s für volle Lautstärke.")]
    public float fullVolumeSpeed = 4f;

    [Tooltip("Mindestabstand in Sekunden zwischen zwei Sounds desselben Pins.")]
    public float cooldown = 0.05f;

    private AudioSource source;
    private float lastHit = -1f;

    protected void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    protected void OnCollisionEnter(Collision collision)
    {
        // Treffer durch die Kugel übernimmt der Strike-Sound in BallAudio.
        if (collision.gameObject.GetComponent<BallAudio>() != null)
        {
            return;
        }

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactSpeed || Time.time - lastHit < cooldown || hitClips.Length == 0)
        {
            return;
        }

        lastHit = Time.time;
        source.pitch = Random.Range(0.92f, 1.08f);
        source.PlayOneShot(hitClips[Random.Range(0, hitClips.Length)], Mathf.Clamp01(speed / fullVolumeSpeed));
    }
}
