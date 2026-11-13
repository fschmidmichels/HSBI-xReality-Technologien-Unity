using UnityEngine;

public class Pin : MonoBehaviour
{
    public float pinFallThreshold = 0.02f;
    public float toppleLife = 3f;
    public int tries = 10;

    private Quaternion defaultRotation;
    private int currentTries;
    private Rigidbody pinRigidbody;

    protected void Awake()
    {
        defaultRotation = transform.localRotation;
        pinRigidbody = GetComponent<Rigidbody>();
    }

    // Ruhig = ausgeblendet oder kaum noch in Bewegung und kein Ausblenden mehr eingeplant.
    // Ein eingeplantes HidePin bedeutet: Der Pin ist gefallen, aber noch nicht gezählt.
    public bool IsSettled(float maxLinearSpeed, float maxAngularSpeed)
    {
        if (!gameObject.activeSelf)
        {
            return true;
        }

        if (IsInvoking(nameof(HidePin)))
        {
            return false;
        }

        if (pinRigidbody == null || pinRigidbody.IsSleeping())
        {
            return true;
        }

        return pinRigidbody.linearVelocity.sqrMagnitude <= maxLinearSpeed * maxLinearSpeed &&
               pinRigidbody.angularVelocity.sqrMagnitude <= maxAngularSpeed * maxAngularSpeed;
    }

    // Ersetzt den Zinnia CollisionNotifier: Nur Treffer durch bewegliche Objekte (Kugel, andere Pins) starten die Prüfung.
    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            CheckTopple();
        }
    }

    public void CheckTopple()
    {
        CancelToppleCheck();
        CheckRotation();
        InvokeRepeating("CheckRotation", 0f, 1f);
    }

    public void CancelToppleCheck()
    {
        currentTries = 0;
        CancelInvoke("CheckRotation");
        CancelInvoke("HidePin");
    }

    protected void CheckRotation()
    {
        currentTries++;
        if (Mathf.Abs(Mathf.Abs(Quaternion.Dot(defaultRotation, transform.localRotation)) - 1f) > pinFallThreshold)
        {
            Invoke("HidePin", toppleLife);
        }
        else
        {
            if (currentTries > tries)
            {
                CancelToppleCheck();
            }
        }
    }

    protected void HidePin()
    {
        // Nur zählen, wenn der Pin noch sichtbar ist, da HidePin mehrfach eingeplant werden kann.
        if (gameObject.activeSelf)
        {
            ScoreScript.scoreValue += 1;
        }
        gameObject.SetActive(false);
    }
}
