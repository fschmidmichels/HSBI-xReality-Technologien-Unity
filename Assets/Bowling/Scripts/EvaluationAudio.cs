using System.Collections;
using UnityEngine;

// Spielt nach der Auswertung eines Wurfs Sounds ab: immer eine kurze Bestätigung,
// danach je nach Ergebnis Jubel (Strike), Klatschen (Spare) oder Enttäuschung (Miss).
// Hört nur auf das Ereignis BallReturn.ThrowEvaluated und funktioniert daher mit und ohne GameManager.
// Tipp: Das Objekt an die Anzeigetafel oder über die Pins setzen (Spatial Blend = 1),
// damit der Sound aus der Spielwelt kommt.
[RequireComponent(typeof(AudioSource))]
public class EvaluationAudio : MonoBehaviour
{
    [Tooltip("BallReturn an der PitZone, meldet das Ende der Auswertung.")]
    public BallReturn ballReturn;

    [Header("Clips")]
    [Tooltip("Kurze Bestätigung nach jeder Auswertung (optional).")]
    public AudioClip confirmationClip;

    [Tooltip("Alle Pins mit dem ersten Wurf des Frames.")]
    public AudioClip strikeClip;

    [Tooltip("Die restlichen Pins mit dem zweiten Wurf des Frames.")]
    public AudioClip spareClip;

    [Tooltip("Kein Pin gefallen.")]
    public AudioClip missClip;

    [Header("Lautstärke")]
    [Range(0f, 1f)]
    public float confirmationVolume = 0.7f;

    [Range(0f, 1f)]
    public float reactionVolume = 1f;

    [Header("Zeitverhalten")]
    [Tooltip("Verzögerung in Sekunden zwischen Bestätigung und Reaktion.")]
    [Min(0f)]
    public float reactionDelay = 0.4f;

    [Tooltip("Maximale Länge der Reaktion in Sekunden. Längere Clips werden ausgeblendet.")]
    [Min(0.1f)]
    public float maxReactionDuration = 3f;

    [Tooltip("Dauer des Ausblendens in Sekunden.")]
    [Min(0f)]
    public float fadeOutDuration = 0.75f;

    private AudioSource source;
    private Coroutine reactionRoutine;

    protected void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    protected void OnEnable()
    {
        if (ballReturn != null)
        {
            ballReturn.ThrowEvaluated += HandleThrowEvaluated;
        }
    }

    protected void OnDisable()
    {
        if (ballReturn != null)
        {
            ballReturn.ThrowEvaluated -= HandleThrowEvaluated;
        }
        StopReaction();
    }

    private void HandleThrowEvaluated(ThrowResult result)
    {
        StopReaction();

        if (confirmationClip != null)
        {
            source.PlayOneShot(confirmationClip, confirmationVolume);
        }

        AudioClip reaction = SelectReaction(result);
        if (reaction != null)
        {
            reactionRoutine = StartCoroutine(PlayReaction(reaction));
        }
    }

    private AudioClip SelectReaction(ThrowResult result)
    {
        if (result.IsStrike)
        {
            return strikeClip;
        }
        if (result.IsSpare)
        {
            return spareClip;
        }
        if (result.IsMiss)
        {
            return missClip;
        }
        return null;
    }

    // Spielt die Reaktion über source.clip (nicht PlayOneShot), damit sie gezielt
    // gekürzt und ausgeblendet werden kann.
    private IEnumerator PlayReaction(AudioClip clip)
    {
        if (reactionDelay > 0f)
        {
            yield return new WaitForSeconds(reactionDelay);
        }

        source.clip = clip;
        source.volume = reactionVolume;
        source.Play();

        float playTime = Mathf.Min(clip.length, maxReactionDuration);
        float fadeStart = Mathf.Max(0f, playTime - fadeOutDuration);
        yield return new WaitForSeconds(fadeStart);

        if (clip.length > maxReactionDuration)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(reactionVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            source.Stop();
        }

        source.volume = 1f;
        reactionRoutine = null;
    }

    private void StopReaction()
    {
        if (reactionRoutine != null)
        {
            StopCoroutine(reactionRoutine);
            reactionRoutine = null;
        }

        if (source != null)
        {
            if (source.clip != null)
            {
                source.Stop();
            }
            source.volume = 1f;
        }
    }
}
