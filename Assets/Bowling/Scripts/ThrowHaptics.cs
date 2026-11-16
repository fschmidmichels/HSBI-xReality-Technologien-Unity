using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

// Haptische Rückmeldung beim Loslassen der Kugel, abgestuft nach der Wurfgeschwindigkeit:
// langsamer Wurf = ein Impuls, schneller Wurf = zwei Impulse, sehr schneller Wurf = drei Impulse.
// Die Stärke steigt zusätzlich mit der Geschwindigkeit.
// Die Wurfgeschwindigkeit liefert das Grabbable des Interaction SDK über das Ereignis
// VelocityThrow.WhenThrown (dieselbe Geschwindigkeit, die auf den Rigidbody übertragen wird).
// Hinweis: Nach kurzen Tracking-Lücken begrenzt das SDK die geschätzte Geschwindigkeit auf 5 m/s
// (RANSACVelocity.MaxSyntheticSpeed); dann wird ein Wurf mit genau 5,00 m/s gemeldet, besonders über Link.
// Bei Hand-Tracking ist kein Controller verbunden; dann gibt es keinen Impuls.
public class ThrowHaptics : MonoBehaviour
{
    [Tooltip("Unter dieser Geschwindigkeit (m/s) gilt das Loslassen als Ablegen: kein Impuls.")]
    [Min(0f)]
    public float minSpeed = 1f;

    [Tooltip("Ab dieser Geschwindigkeit (m/s) gibt es zwei Impulse.")]
    [Min(0f)]
    public float doublePulseSpeed = 3f;

    [Tooltip("Ab dieser Geschwindigkeit (m/s) gibt es drei Impulse.")]
    [Min(0f)]
    public float triplePulseSpeed = 4.5f;

    [Tooltip("Ab dieser Geschwindigkeit (m/s) vibriert der Controller mit maximaler Stärke.")]
    [Min(0.1f)]
    public float maxSpeed = 5f;

    [Range(0f, 1f)]
    [Tooltip("Stärke bei minSpeed.")]
    public float minAmplitude = 0.4f;

    [Range(0f, 1f)]
    [Tooltip("Stärke bei maxSpeed.")]
    public float maxAmplitude = 1f;

    [Min(0.01f)]
    [Tooltip("Dauer eines einzelnen Impulses in Sekunden.")]
    public float pulseDuration = 0.08f;

    [Min(0.01f)]
    [Tooltip("Pause zwischen zwei Impulsen in Sekunden.")]
    public float pauseDuration = 0.08f;

    [Tooltip("Schreibt Geschwindigkeit und Stärke in die Console bzw. ins Logcat (Tag Unity). Nur zur Fehlersuche.")]
    public bool logDebug = true;

    private Grabbable grabbable;
    private GrabInteractable grabInteractable;
    private HandGrabInteractable handGrabInteractable;
    private ControllerRef lastController;
    private bool subscribed;

    protected void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        grabInteractable = GetComponentInChildren<GrabInteractable>();
        handGrabInteractable = GetComponentInChildren<HandGrabInteractable>();
    }

    protected void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorViewAdded += OnGrabbed;
        }
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenSelectingInteractorViewAdded += OnGrabbed;
        }
        if (grabbable != null && grabbable.VelocityThrow != null)
        {
            grabbable.VelocityThrow.WhenThrown += OnThrown;
            subscribed = true;
        }
        Log("Bereit: Grabbable=" + (grabbable != null) + ", Wurfereignis abonniert=" + subscribed);
    }

    protected void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorViewAdded -= OnGrabbed;
        }
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenSelectingInteractorViewAdded -= OnGrabbed;
        }
        if (subscribed && grabbable != null && grabbable.VelocityThrow != null)
        {
            grabbable.VelocityThrow.WhenThrown -= OnThrown;
        }
        subscribed = false;
    }

    // Merkt sich, welcher Controller die Kugel gegriffen hat.
    private void OnGrabbed(IInteractorView interactorView)
    {
        MonoBehaviour interactor = interactorView as MonoBehaviour;
        lastController = interactor != null ? interactor.GetComponentInParent<ControllerRef>() : null;
    }

    private void OnThrown(Vector3 velocity, Vector3 torque)
    {
        float speed = velocity.magnitude;
        if (lastController == null || !lastController.IsConnected)
        {
            Log("Wurf mit " + speed.ToString("0.00") + " m/s, kein Controller verbunden: kein Impuls");
            return;
        }
        if (speed < minSpeed)
        {
            Log("Ablegen mit " + speed.ToString("0.00") + " m/s (unter " + minSpeed + " m/s): kein Impuls");
            return;
        }

        // Anzahl der Impulse nach Geschwindigkeitsstufe.
        int pulses = speed >= triplePulseSpeed ? 3 : speed >= doublePulseSpeed ? 2 : 1;

        // Geschwindigkeit zwischen minSpeed und maxSpeed auf 0..1 abbilden, dann auf die Stärke.
        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        float amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, t);

        OVRInput.Controller touch = lastController.Handedness == Handedness.Left
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;

        Log("Wurf mit " + speed.ToString("0.00") + " m/s: " + pulses + " Impuls(e) an " + touch + ", Stärke " + amplitude.ToString("0.00"));
        StopAllCoroutines();
        StartCoroutine(Pulses(touch, amplitude, pulses));
    }

    private IEnumerator Pulses(OVRInput.Controller touch, float amplitude, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                yield return new WaitForSeconds(pauseDuration);
            }
            OVRInput.SetControllerVibration(1f, amplitude, touch);
            yield return new WaitForSeconds(pulseDuration);
            OVRInput.SetControllerVibration(1f, 0f, touch);
        }
    }

    private void Log(string message)
    {
        if (logDebug)
        {
            Debug.Log("[ThrowHaptics] " + message, this);
        }
    }
}
