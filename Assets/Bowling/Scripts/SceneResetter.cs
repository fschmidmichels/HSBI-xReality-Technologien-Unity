using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneResetter : MonoBehaviour
{
    [Tooltip("Wie oft die Menütaste gedrückt werden muss.")]
    public int requiredPresses = 3;

    [Tooltip("Maximale Zeit in Sekunden zwischen zwei Tastendrücken.")]
    public float maxPressInterval = 0.5f;

    private int pressCount;
    private float lastPressTime;

    protected void Update()
    {
        // Button.Start ist die Menütaste auf dem linken Controller (bei Hand-Tracking: Menü-Geste der linken Hand).
        if (!OVRInput.GetDown(OVRInput.Button.Start))
        {
            return;
        }

        if (Time.unscaledTime - lastPressTime > maxPressInterval)
        {
            pressCount = 0;
        }

        pressCount++;
        lastPressTime = Time.unscaledTime;

        if (pressCount >= requiredPresses)
        {
            pressCount = 0;
            ResetScene();
        }
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
