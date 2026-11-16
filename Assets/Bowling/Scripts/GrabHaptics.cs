using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

// Kurzer haptischer Impuls am greifenden Controller, sobald die Kugel gegriffen wird.
// Hört auf GrabInteractable und HandGrabInteractable, weil das Comprehensive Interaction Rig
// auch mit Controllern über controllergesteuerte Hände greifen kann.
// Bei Hand-Tracking ist kein Controller verbunden; dann gibt es keinen Impuls.
public class GrabHaptics : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("Stärke der Vibration.")]
    public float amplitude = 0.6f;

    [Min(0.01f)]
    [Tooltip("Dauer des Impulses in Sekunden.")]
    public float duration = 0.15f;

    [Tooltip("Schreibt jeden Schritt in die Console bzw. ins Logcat (Tag Unity). Nur zur Fehlersuche.")]
    public bool logDebug = true;

    private GrabInteractable grabInteractable;
    private HandGrabInteractable handGrabInteractable;

    protected void Awake()
    {
        grabInteractable = GetComponentInChildren<GrabInteractable>();
        handGrabInteractable = GetComponentInChildren<HandGrabInteractable>();
        Log("Awake: GrabInteractable=" + (grabInteractable != null) + ", HandGrabInteractable=" + (handGrabInteractable != null));
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
    }

    private void OnGrabbed(IInteractorView interactorView)
    {
        // Vom greifenden Interactor aus den Controller (links/rechts) im Rig suchen.
        MonoBehaviour interactor = interactorView as MonoBehaviour;
        if (interactor == null)
        {
            Log("OnGrabbed: Interactor ist kein MonoBehaviour (" + interactorView + ")");
            return;
        }

        ControllerRef controller = interactor.GetComponentInParent<ControllerRef>();
        Log("OnGrabbed: Interactor=" + interactor.name + " (" + interactor.GetType().Name + ")"
            + ", ControllerRef=" + (controller != null ? controller.name : "keiner")
            + ", IsConnected=" + (controller != null && controller.IsConnected)
            + ", OVRInput verbunden=" + OVRInput.GetConnectedControllers()
            + ", aktiv=" + OVRInput.GetActiveController()
            + ", XR-Gerät=" + OVRManager.loadedXRDevice);
        if (controller == null || !controller.IsConnected)
        {
            return;
        }

        OVRInput.Controller touch = controller.Handedness == Handedness.Left
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;

        Log("OnGrabbed: Impuls an " + touch + ", Amplitude " + amplitude + ", Dauer " + duration + " s");
        StartCoroutine(Pulse(touch));
    }

    private void Log(string message)
    {
        if (logDebug)
        {
            Debug.Log("[GrabHaptics] " + message, this);
        }
    }

    private IEnumerator Pulse(OVRInput.Controller touch)
    {
        OVRInput.SetControllerVibration(1f, amplitude, touch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(1f, 0f, touch);
    }
}
