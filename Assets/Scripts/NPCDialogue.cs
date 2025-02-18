using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue Texts")]
    public string greetingDialogue = "Hello there!";
    public string questDialogue = "I really need that special item.";
    public string deliveredDialogue = "Thank you! You've saved the day!";

    [Header("Item & Delivery")]
    public InventoryItem requiredItem;
    [Tooltip("Required quantity of the quest item (e.g., 5 coins).")]
    public int requiredQuantity = 1;
    [Tooltip("Array of GameObjects to toggle (e.g., doors) when the item is delivered.")]
    public GameObject[] toggleGameObjects;

    [Header("Speech Bubble UI (Dialogue)")]
    [Tooltip("The UI element that displays dialogue (e.g., a world-space Panel with Text).")]
    public GameObject speechBubble;   // This is the speech bubble element (not the entire container)
    [Tooltip("The Text component inside the speech bubble.")]
    public Text bubbleText;
    [Tooltip("Delay (in seconds) for dialogue transitions (for quest dialogue).")]
    public float dialogueDelay = 3f;
    [Tooltip("Delay (in seconds) after delivered dialogue before hiding the speech bubble.")]
    public float deliveryHideDelay = 1f;

    [Header("Quest Indicator UI")]
    [Tooltip("The quest indicator UI (e.g., an Image) that displays the quest icon.")]
    public GameObject questIndicator;   // This should be a separate UI element that remains enabled
    [Tooltip("The Image component of the quest indicator.")]
    public Image questIndicatorImage;
    [Tooltip("The default quest icon (exclamation mark).")]
    public Sprite exclamationSprite;
    [Tooltip("The quest icon when the player has the required item (question mark).")]
    public Sprite questionSprite;

    [Header("Animator")]
    [Tooltip("The NPC's Animator Controller with a 'talk' parameter.")]
    public Animator npcAnimator;

    [Header("Optional Delivery Animation")]
    [Tooltip("Set to true if you want to play an additional animation on quest delivery.")]
    public bool playDeliveryAnimation = false;
    [Tooltip("Animation trigger name to use on quest delivery.")]
    public string deliveryAnimationTrigger = "Delivery";

    [Header("Sound Settings")]
    [Tooltip("Sound to play when the quest is delivered.")]
    public AudioClip deliverySound;
    [Tooltip("Volume for the delivery sound (0 to 1).")]
    [Range(0f, 1f)]
    public float deliverySoundVolume = 1f;

    private bool hasDelivered = false;
    private bool isPlayerInTrigger = false;
    private Coroutine dialogueCoroutine;

    void Start()
    {
        // Hide the speech bubble at start.
        if (speechBubble != null)
            speechBubble.SetActive(false);

        // Ensure the quest indicator is active if the quest is not yet delivered.
        if (questIndicator != null && !hasDelivered)
        {
            questIndicator.SetActive(true);
            if (questIndicatorImage != null)
            {
                bool hasItem = (InventoryManager.Instance != null && requiredItem != null &&
                                InventoryManager.Instance.HasItem(requiredItem, requiredQuantity));
                questIndicatorImage.sprite = hasItem ? questionSprite : exclamationSprite;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInTrigger = true;

        // Hide the quest indicator when the player enters the trigger zone.
        if (questIndicator != null)
            questIndicator.SetActive(false);

        bool hasItem = (InventoryManager.Instance != null && requiredItem != null &&
                        InventoryManager.Instance.HasItem(requiredItem, requiredQuantity));

        if (!hasDelivered)
        {
            if (hasItem)
            {
                // Run the delivery sequence if the player has the required quantity.
                dialogueCoroutine = StartCoroutine(DeliverItemSequence());
            }
            else
            {
                // Run the quest dialogue sequence (greeting then quest dialogue).
                dialogueCoroutine = StartCoroutine(QuestDialogueSequence());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInTrigger = false;

        // Stop any ongoing dialogue sequence.
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        // Hide the speech bubble.
        HideDialogue();

        // When the player exits, re-enable the quest indicator if the quest is not delivered.
        if (questIndicator != null && !hasDelivered)
        {
            bool hasItem = (InventoryManager.Instance != null && requiredItem != null &&
                            InventoryManager.Instance.HasItem(requiredItem, requiredQuantity));
            if (questIndicatorImage != null)
                questIndicatorImage.sprite = hasItem ? questionSprite : exclamationSprite;
            questIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Dialogue sequence when the player does not have the required item.
    /// It shows the greeting, then transitions to the quest dialogue,
    /// which remains until the player exits the trigger.
    /// </summary>
    IEnumerator QuestDialogueSequence()
    {
        ShowDialogue(greetingDialogue);
        yield return new WaitForSeconds(dialogueDelay);
        ShowDialogue(questDialogue);
        // Remain on quest dialogue until the player exits the trigger.
        while (isPlayerInTrigger)
        {
            yield return null;
        }
        HideDialogue();
    }

    /// <summary>
    /// Dialogue sequence for when the player has the required quantity of the required item.
    /// It shows the delivery dialogue, removes the item(s), toggles the assigned GameObjects,
    /// optionally plays a delivery animation and sound, then hides the speech bubble after a short delay.
    /// </summary>
    IEnumerator DeliverItemSequence()
    {
        ShowDialogue(deliveredDialogue);

        // Play the delivery sound, if assigned.
        if (deliverySound != null)
        {
            AudioSource.PlayClipAtPoint(deliverySound, transform.position, deliverySoundVolume);
        }

        // Remove the required quantity of the item from the player's inventory.
        if (InventoryManager.Instance != null && requiredItem != null)
        {
            InventoryManager.Instance.RemoveItem(requiredItem, requiredQuantity);
        }
        hasDelivered = true;

        // Toggle each of the assigned GameObjects.
        if (toggleGameObjects != null)
        {
            foreach (GameObject obj in toggleGameObjects)
            {
                if (obj != null)
                    obj.SetActive(!obj.activeSelf);
            }
        }

        // Optionally, play the delivery animation.
        if (playDeliveryAnimation && npcAnimator != null)
        {
            npcAnimator.SetTrigger(deliveryAnimationTrigger);
        }

        // Wait for the specified delay, then hide the speech bubble.
        yield return new WaitForSeconds(deliveryHideDelay);
        HideDialogue();
    }

    /// <summary>
    /// Enables the speech bubble, sets its dialogue text, and triggers the NPC's "talk" animation.
    /// </summary>
    void ShowDialogue(string message)
    {
        if (speechBubble != null && bubbleText != null)
        {
            speechBubble.SetActive(true);
            bubbleText.text = message;
        }
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("talk", true);
        }
    }

    /// <summary>
    /// Hides the speech bubble and stops the NPC's "talk" animation.
    /// </summary>
    void HideDialogue()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("talk", false);
        }
    }
}








