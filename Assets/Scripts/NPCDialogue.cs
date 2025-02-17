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
    public GameObject toggleGameObject; // e.g., a door to toggle

    [Header("Speech Bubble UI (Dialogue)")]
    [Tooltip("The UI element that displays dialogue (e.g., a world-space Panel with Text).")]
    public GameObject speechBubble;   // This is the speech bubble element (not the entire container)
    [Tooltip("The Text component inside the speech bubble.")]
    public Text bubbleText;
    [Tooltip("Delay (in seconds) for dialogue transitions.")]
    public float dialogueDelay = 3f;

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
                                InventoryManager.Instance.items.Contains(requiredItem));
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
                        InventoryManager.Instance.items.Contains(requiredItem));

        if (!hasDelivered)
        {
            if (hasItem)
            {
                // Run the delivery sequence if the player has the item.
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
                            InventoryManager.Instance.items.Contains(requiredItem));
            if (questIndicatorImage != null)
                questIndicatorImage.sprite = hasItem ? questionSprite : exclamationSprite;
            questIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Dialogue sequence when the player does not have the required item.
    /// It shows the greeting, then transitions to the quest dialogue,
    /// which remains until the player leaves the trigger.
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
    /// Dialogue sequence for when the player has the required item.
    /// It shows the delivery dialogue, removes the item, and toggles the assigned GameObject.
    /// </summary>
    IEnumerator DeliverItemSequence()
    {
        ShowDialogue(deliveredDialogue);

        // Remove the item from the player's inventory.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.items.Remove(requiredItem);
        }
        hasDelivered = true;

        // Toggle the assigned GameObject (if provided).
        if (toggleGameObject != null)
        {
            toggleGameObject.SetActive(!toggleGameObject.activeSelf);
        }
        yield return new WaitForSeconds(dialogueDelay);
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
        if(npcAnimator != null)
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
        if(npcAnimator != null)
        {
            npcAnimator.SetBool("talk", false);
        }
    }
}




