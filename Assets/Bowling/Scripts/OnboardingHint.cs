using System.Collections;
using UnityEngine;

// Zeigt beim Start einen kurzen Hinweis im Raum und blendet ihn aus,
// sobald der erste Wurf beginnt (GameManager wechselt nach ThrowRunning).
[RequireComponent(typeof(CanvasGroup))]
public class OnboardingHint : MonoBehaviour
{
    [Tooltip("GameManager der Szene.")]
    public GameManager gameManager;

    [Min(0f)]
    [Tooltip("Dauer des Ausblendens in Sekunden.")]
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private bool hidden;

    protected void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
    }

    protected void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged += HandleStateChanged;
        }
    }

    protected void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (!hidden && state == GameManager.GameState.ThrowRunning)
        {
            hidden = true;
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
