using System.Collections;
using UnityEngine;
using TMPro;

public class TextDisplayTrigger : MonoBehaviour
{
    [Header("Text Settings")]
    [Tooltip("The message to display when the player enters the trigger.")]
    public string message;
    [Tooltip("Reference to the TextMeshProUGUI component that displays the message.")]
    public TextMeshProUGUI textComponent;

    [Header("Fade Settings")]
    [Tooltip("Speed at which the text fades in (alpha per second).")]
    public float fadeInSpeed = 2f;
    [Tooltip("Speed at which the text fades out (alpha per second).")]
    public float fadeOutSpeed = 2f;
    [Tooltip("Delay before auto fading out the text (if waitForInput is false).")]
    public float autoFadeDelay = 2f;

    [Header("Input Settings")]
    [Tooltip("If true, the text will remain until the designated key is pressed to trigger fade-out and then destroy this GameObject.")]
    public bool waitForInput = false;
    [Tooltip("The key used to dismiss the text (e.g., E).")]
    public KeyCode dismissKey = KeyCode.E;

    private bool playerInside = false;
    private bool inputTriggeredDestroy = false;
    private Coroutine currentCoroutine;

    private void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("[TextDisplayTrigger] TextMeshProUGUI reference is not assigned!");
            return;
        }
        textComponent.text = message;
        SetTextAlpha(0f); // Start fully transparent.
        Debug.Log("[TextDisplayTrigger] Start: Text set, alpha = 0.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only respond if the collider is tagged "Player"
        if (!other.CompareTag("Player"))
            return;

        // Prevent re-triggering if we've already begun fade-out/destroy.
        if (inputTriggeredDestroy)
            return;

        // If already inside, ignore repeated triggers.
        if (playerInside)
            return;

        playerInside = true;
        inputTriggeredDestroy = false;
        Debug.Log("[TextDisplayTrigger] Player entered trigger.");

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeText(0f, 1f, fadeInSpeed));

        if (!waitForInput)
        {
            // If not waiting for input, auto-fade after delay.
            currentCoroutine = StartCoroutine(AutoFadeOut(autoFadeDelay));
        }
    }

    // When waitForInput is enabled, we ignore OnTriggerExit so that the text stays until the key is pressed.
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!waitForInput)
        {
            playerInside = false;
            Debug.Log("[TextDisplayTrigger] Player exited trigger.");
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            currentCoroutine = StartCoroutine(FadeText(textComponent.alpha, 0f, fadeOutSpeed));
        }
    }

    private void Update()
    {
        if (waitForInput && playerInside && !inputTriggeredDestroy)
        {
            if (Input.GetKeyDown(dismissKey))
            {
                Debug.Log("[TextDisplayTrigger] Dismiss key pressed. Initiating fade-out and destroy.");
                TriggerFadeOutAndDestroy();
            }
        }
    }

    private void TriggerFadeOutAndDestroy()
    {
        inputTriggeredDestroy = true;
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeOutAndDestroy(textComponent.alpha, 0f, fadeOutSpeed));
    }

    IEnumerator AutoFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerInside && !inputTriggeredDestroy)
        {
            Debug.Log("[TextDisplayTrigger] AutoFadeOut triggered.");
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(FadeText(textComponent.alpha, 0f, fadeOutSpeed));
        }
    }

    IEnumerator FadeText(float startAlpha, float endAlpha, float speed)
    {
        float alpha = startAlpha;
        while (!Mathf.Approximately(alpha, endAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, endAlpha, speed * Time.deltaTime);
            SetTextAlpha(alpha);
            yield return null;
        }
        currentCoroutine = null;
    }

    IEnumerator FadeOutAndDestroy(float startAlpha, float endAlpha, float speed)
    {
        float alpha = startAlpha;
        while (!Mathf.Approximately(alpha, endAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, endAlpha, speed * Time.deltaTime);
            SetTextAlpha(alpha);
            yield return null;
        }
        Debug.Log("[TextDisplayTrigger] Fade-out complete. Destroying GameObject.");
        Destroy(gameObject);
    }

    void SetTextAlpha(float alpha)
    {
        if (textComponent != null)
        {
            Color c = textComponent.color;
            c.a = alpha;
            textComponent.color = c;
        }
    }
}














