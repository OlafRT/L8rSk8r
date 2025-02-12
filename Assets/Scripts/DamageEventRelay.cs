using UnityEngine;

public class DamageEventRelayMulti : MonoBehaviour
{
    [Header("Sword Damage Dealers")]
    [Tooltip("Assign all NPCDamageDealer components for the sword attacks here.")]
    public NPCDamageDealer[] swordDamageDealers;

    [Header("Kick Damage Dealers")]
    [Tooltip("Assign all NPCDamageDealer components for the kick attacks here.")]
    public NPCDamageDealer[] kickDamageDealers;

    /// <summary>
    /// Called via Animation Event to enable sword damage.
    /// </summary>
    public void EnableDamageEvent()
    {
        foreach (NPCDamageDealer dealer in swordDamageDealers)
        {
            if (dealer != null)
            {
                dealer.EnableDamage();
            }
        }
    }

    /// <summary>
    /// Called via Animation Event to disable sword damage.
    /// </summary>
    public void DisableDamageEvent()
    {
        foreach (NPCDamageDealer dealer in swordDamageDealers)
        {
            if (dealer != null)
            {
                dealer.DisableDamage();
            }
        }
    }

    /// <summary>
    /// Called via Animation Event to enable kick damage.
    /// </summary>
    public void EnableKickEvent()
    {
        foreach (NPCDamageDealer dealer in kickDamageDealers)
        {
            if (dealer != null)
            {
                dealer.EnableDamage();
            }
        }
    }

    /// <summary>
    /// Called via Animation Event to disable kick damage.
    /// </summary>
    public void DisableKickEvent()
    {
        foreach (NPCDamageDealer dealer in kickDamageDealers)
        {
            if (dealer != null)
            {
                dealer.DisableDamage();
            }
        }
    }
}


