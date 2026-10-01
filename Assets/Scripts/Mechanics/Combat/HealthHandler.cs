using System.Collections.Generic;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    // Handle core stats for ANY OBJECTs that has hit points, can be interacted with via [COMBAT] and then be removed from game
    // > Hit point
    // > death / destruction [v0.1: foundation that does nothing special, mechanically]
    //   > (future) tracking proposes
    //   > (future) 'triggers on death' effects
    // ? (future) association with defensive script (dmg reduction, reaction, etc.)
    // ? (optimize) object-pooling

    // Core stats
    public int maxHP = 5;

    // Current Health (avoid direct assignment from outside!)
    public int hp;

    bool isDying = false;

    //====================================
    // Unity Messages

    void LateUpdate()
    {
        if (isDying)
        {
            ResolveDeath();
        }
    }

    //====================================
    // Custom methods

    public void Setup()
    {
        hp = maxHP;
    }

    //====================================
    // Methods involve entity's destruction (death)
    //====================================

    // Trigger and mark as dead
    public void TriggerDeath()
    {
        isDying = true;
    }

    void ResolveDeath()
    {
        // other action(s) to be resolved on entity's destruction executed here

        Destroy(gameObject);
    }

    //====================================
    // Methods involve taking damage
    //====================================

    // Check if this is still 'alive'
    public bool CheckAlive(bool healthOnly = false)
    {
        if (healthOnly)
        {
            return hp > 0 ;
        }
        else
        {
            return !(isDying || hp <= 0);
        }
    }

    // Take damage; deducing HP without any additional effects
    public void TakeDamage(int damage)
    {
        // take damage
        hp -= damage;

        if (hp <= 0)
        {
            TriggerDeath();
        }
    }
}
