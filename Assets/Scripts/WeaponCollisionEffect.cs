using System.Collections;
using UnityEngine;

public class ImpactReceiver : MonoBehaviour
{
    [Tooltip("The layer mask for the weapon layer.")]
    public LayerMask weaponLayer;

    [Header("Impact Settings")]
    [Tooltip("Number of hits required to destroy the object.")]
    public int hitsToDestroy = 1;

    [Header("Impact Sound")]
    [Tooltip("AudioSource used to play impact sounds. If not assigned, the AudioSource on this GameObject is used.")]
    public AudioSource impactAudioSource;
    [Tooltip("Impact sounds to choose from when hit.")]
    public AudioClip[] impactClips;

    [Header("Impact Sound Variation")]
    [Tooltip("Minimum pitch multiplier (e.g., 0.95).")]
    public float minPitch = 0.95f;
    [Tooltip("Maximum pitch multiplier (e.g., 1.05).")]
    public float maxPitch = 1.05f;

    [Header("Attack & Effects Settings")]
    [Tooltip("Name of the bool in the Animator that indicates an attack is occurring.")]
    public string attackingBoolName;
    [Tooltip("Particle effects to play when hit.")]
    public ParticleSystem[] collisionEffects;

    [Header("Loot Settings")]
    [Tooltip("Array of loot prefabs that can drop when the object is destroyed. Assign prefab assets from your project.")]
    public GameObject[] lootPrefabs;
    [Tooltip("Chance for each loot prefab to drop (0 to 1).")]
    public float lootDropChance = 0.5f;
    [Tooltip("Random offset range for dropped loot on the X and Z axes.")]
    public float lootDropOffset = 0.5f;

    // Internal state
    private bool isEffectPlaying = false;
    private int hitCount = 0;

    private void Start()
    {
        // If no AudioSource is assigned, get the one on this GameObject.
        if (impactAudioSource == null)
        {
            impactAudioSource = GetComponent<AudioSource>();
            if (impactAudioSource == null)
            {
                Debug.LogWarning("No AudioSource component found on " + gameObject.name + ". Impact sound will not play.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1) Check if the colliding object is on the designated weapon layer.
        if (!IsOnWeaponLayer(collision.gameObject))
            return;

        // 2) Check if the colliding object's root is actually attacking.
        if (!IsAttacking(collision.gameObject))
            return;

        // 3) Conditions met: play impact sound (with pitch variation), particle effects, and increment hit count.
        if (impactAudioSource != null && impactClips != null && impactClips.Length > 0)
        {
            AudioClip selectedClip = impactClips[Random.Range(0, impactClips.Length)];
            float originalPitch = impactAudioSource.pitch;
            impactAudioSource.pitch = Random.Range(minPitch, maxPitch);
            impactAudioSource.PlayOneShot(selectedClip);
            impactAudioSource.pitch = originalPitch;
        }

        PlayEffectsAtPosition(collision.contacts[0].point);
        hitCount++;

        // 4) If hit count reaches or exceeds hitsToDestroy, start the destruction sequence.
        if (hitCount >= hitsToDestroy)
        {
            StartCoroutine(DestroyObjectAfterEffects());
        }
    }

    private bool IsOnWeaponLayer(GameObject obj)
    {
        return (weaponLayer.value & (1 << obj.layer)) != 0;
    }

    private bool IsAttacking(GameObject obj)
    {
        Animator anim = obj.transform.root.GetComponentInChildren<Animator>();
        if (anim == null)
            return false;
        return anim.GetBool(attackingBoolName);
    }

    private void PlayEffectsAtPosition(Vector3 position)
    {
        if (collisionEffects == null || collisionEffects.Length == 0)
            return;

        if (!isEffectPlaying)
        {
            float longestDuration = 0f;
            foreach (ParticleSystem ps in collisionEffects)
            {
                if (ps == null)
                    continue;
                ps.transform.position = position;
                ps.Play();
                float duration = ps.main.duration;
                if (duration > longestDuration)
                    longestDuration = duration;
            }
            isEffectPlaying = true;
            if (longestDuration > 0f)
                Invoke(nameof(ResetEffectFlag), longestDuration);
            else
                ResetEffectFlag();
        }
    }

    private void ResetEffectFlag()
    {
        isEffectPlaying = false;
    }

    private IEnumerator DestroyObjectAfterEffects()
    {
        // Drop loot before destruction.
        DropLoot();

        // Determine longest duration among collision effects.
        float longestDuration = 0f;
        if (collisionEffects != null)
        {
            foreach (ParticleSystem ps in collisionEffects)
            {
                if (ps == null)
                    continue;
                float duration = ps.main.duration;
                if (duration > longestDuration)
                    longestDuration = duration;
            }
        }
        yield return new WaitForSeconds(longestDuration);

        // Unparent particle effects so they persist after destruction.
        if (collisionEffects != null)
        {
            foreach (ParticleSystem ps in collisionEffects)
            {
                if (ps == null)
                    continue;
                ps.transform.SetParent(null, true);
            }
        }

        // Destroy the object.
        Destroy(gameObject);
    }

    private void DropLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0)
            return;

        foreach (GameObject lootPrefab in lootPrefabs)
        {
            if (lootPrefab == null)
                continue;
            float roll = Random.value;
            if (roll <= lootDropChance)
            {
                Vector3 dropPosition = transform.position;
                dropPosition.x += Random.Range(-lootDropOffset, lootDropOffset);
                dropPosition.z += Random.Range(-lootDropOffset, lootDropOffset);
                Instantiate(lootPrefab, dropPosition, Quaternion.identity);
            }
        }
    }
}


