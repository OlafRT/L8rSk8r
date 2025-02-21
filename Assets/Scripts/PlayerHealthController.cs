using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the player.")]
    public int maxHealth = 10;
    [Tooltip("Starting health of the player.")]
    public int startingHealth = 10;
    private int currentHealth;

    [Header("UI Settings")]
    [Tooltip("Slider UI element to display health (set Direction to Vertical in Inspector).")]
    public Slider healthBar;
    [Tooltip("TextMeshProUGUI element to display health (e.g., '8/10').")]
    public TextMeshProUGUI healthText;

    [Header("Regeneration Settings")]
    [Tooltip("Time (in seconds) between each health regeneration.")]
    public float regenInterval = 20f;
    [Tooltip("Amount of health regained per interval.")]
    public int regenAmount = 1;

    [Header("Death Settings")]
    [Tooltip("Player's Animator that will play the death animation.")]
    public Animator playerAnimator;
    [Tooltip("Name of the trigger to play the player's death animation.")]
    public string deathAnimationTrigger = "Death";
    [Tooltip("Reference to the player's controller script to disable upon death.")]
    public MonoBehaviour playerController;
    [Tooltip("Delay (in seconds) before disabling the player's Animator after death (to let the death animation play).")]
    public float disableAnimatorDelay = 2f;

    [Header("Audio Settings")]
    [Tooltip("AudioSource to play hit and death sounds.")]
    public AudioSource audioSource;
    [Tooltip("Array of hit sounds (a random one will be played on each hit).")]
    public AudioClip[] hitSounds;
    [Tooltip("Sound to play when the player dies.")]
    public AudioClip deathSound;

    [Header("Death UI")]
    [Tooltip("TextMeshProUGUI element that displays the 'You Died' message.")]
    public TextMeshProUGUI youDiedText;
    [Tooltip("Image component on the death screen panel (the black background).")]
    public Image deathScreenPanel;
    [Tooltip("UI panel with buttons (e.g., restart) to display after death.")]
    public GameObject restartPanel;

    // Static flag so other scripts know if the player is dead.
    public static bool isPlayerDead = false;

    void Start()
    {
        currentHealth = startingHealth;
        UpdateHealthUI();
        StartCoroutine(RegenerateHealth());

        // Initialize UI: Hide "You Died" text, death screen panel, and restart panel.
        if (youDiedText != null)
        {
            Color tColor = youDiedText.color;
            tColor.a = 0f;
            youDiedText.color = tColor;
            youDiedText.gameObject.SetActive(false);
        }
        if (deathScreenPanel != null)
        {
            // Set initial alpha to 0.
            Color dColor = deathScreenPanel.color;
            dColor.a = 0f;
            deathScreenPanel.color = dColor;
            deathScreenPanel.gameObject.SetActive(false);
        }
        if (restartPanel != null)
        {
            restartPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Deducts health from the player and handles death.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isPlayerDead)
            return;

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;
        UpdateHealthUI();

        // Play a random hit sound if still alive.
        if (currentHealth > 0 && hitSounds != null && hitSounds.Length > 0)
        {
            int idx = Random.Range(0, hitSounds.Length);
            audioSource.PlayOneShot(hitSounds[idx]);
        }
        else if (currentHealth <= 0)
        {
            isPlayerDead = true;

            // Play the player's death sound.
            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);

            if (playerAnimator != null)
                playerAnimator.SetTrigger(deathAnimationTrigger);

            if (playerController != null)
                playerController.enabled = false;

            // NEW: If a boss delivered the final blow, play its kill sound.
            if (BossController.lastAttacker != null)
            {
                BossController.lastAttacker.PlayKillSound();
                BossController.lastAttacker = null;
            }

            // Start UI fade-in for death elements.
            StartCoroutine(FadeInDeathUI());
            // Disable the animator after a delay.
            StartCoroutine(DisableAnimatorAfterDelay(disableAnimatorDelay));
            // Mute all audio in the game after 15 seconds.
            StartCoroutine(MuteAllAudioAfterDelay(15f));
        }
    }

    /// <summary>
    /// Increases the player's health.
    /// </summary>
    public void Heal(int amount)
    {
        if (isPlayerDead)
            return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        UpdateHealthUI();
    }

    /// <summary>
    /// Updates the UI elements to reflect current health.
    /// </summary>
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    /// <summary>
    /// Regenerates health every regenInterval seconds.
    /// </summary>
    IEnumerator RegenerateHealth()
    {
        while (!isPlayerDead)
        {
            yield return new WaitForSeconds(regenInterval);
            if (currentHealth < maxHealth)
                Heal(regenAmount);
        }
    }

    /// <summary>
    /// Fades in the death UI (black screen, "You Died" text, and restart panel).
    /// </summary>
    IEnumerator FadeInDeathUI()
    {
        float duration = 2f;
        float elapsed = 0f;

        if (deathScreenPanel != null)
        {
            deathScreenPanel.gameObject.SetActive(true);
        }
        if (youDiedText != null)
        {
            youDiedText.gameObject.SetActive(true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            if (deathScreenPanel != null)
            {
                Color dColor = deathScreenPanel.color;
                dColor.a = alpha;
                deathScreenPanel.color = dColor;
            }
            if (youDiedText != null)
            {
                Color tColor = youDiedText.color;
                tColor.a = alpha;
                youDiedText.color = tColor;
            }
            yield return null;
        }
        // Ensure full alpha.
        if (deathScreenPanel != null)
        {
            Color dColor = deathScreenPanel.color;
            dColor.a = 1f;
            deathScreenPanel.color = dColor;
        }
        if (youDiedText != null)
        {
            Color tColor = youDiedText.color;
            tColor.a = 1f;
            youDiedText.color = tColor;
        }
        if (restartPanel != null)
            restartPanel.SetActive(true);

        // Show the cursor and unlock it so you can click on the UI.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Waits for a given delay before disabling the player's Animator.
    /// </summary>
    IEnumerator DisableAnimatorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerAnimator != null)
            playerAnimator.enabled = false;
    }

    /// <summary>
    /// Waits for a given delay then mutes all audio by setting AudioListener.volume to 0.
    /// </summary>
    IEnumerator MuteAllAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioListener.volume = 0f;
    }
}






