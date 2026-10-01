using System.Collections.Generic;
using UnityEngine;

public class OwnershipScript : MonoBehaviour
{
    // Handle ownership, telling apart ally from enemy
    // > player ID
    // > method to ask "should I attack this?"
    // > (future) team ID
    // > (future) compile list of:
    //   friendy, opponents, attack on contact, attack on sight, etc.

    // reserve number:
    // <-1: null / invalid (treated as true neutral)
    // -1 : common enemy / obstacle to anything else, require immediate action
    // 0  : true neutral; engage if need be (stuck, etc)
    // 1  : player exclusively

    //====================================
    // Data Variables associated with this ownerID
    public int ownerID = -2;

    // color palette: highlight - main - shade
    public Color colorLight, colorMain, colorShade;

    // sprite / emblem


    //====================================
    // Cache Variables (temp draft)
    // The entity's ally; will never attack them outside special conditions
    public List<int> allyIDs = new List<int>();

    // With whom this entity is considered as obstacle, allowing itself to be cleared away by them
    //  (aka. never be attacked on first contact, but be destroyed later to clear the way)
    public List<int> obstructIDs = new List<int>();


    //====================================
    // Unity Messages


    //====================================
    // Custom methods

    public void Setup()
    {
        // placeholder 
        allyIDs.Clear();
        obstructIDs.Clear();

        if (ownerID < -1)
        {
            Debug.LogWarning($"Entity without assigned owner ID!\n[{gameObject.name}] at {transform.position}");
            allyIDs.Add(ownerID);
            allyIDs.Add(0);
        }
        else if (ownerID <= 0) 
        {
            allyIDs.Add(ownerID);
        }
        else
        {
            allyIDs.Add(ownerID);
            obstructIDs.Add(0);
        }
    }

    //====================================
    // Methods asking friend or enemy
    //====================================

    // asking if this entity has to be dealt with "immediately"
    public bool CheckHostile(int requesterID)
    {
        switch (ownerID)
        {
            case -1:
                // "common enemy" is "active threat" and will always be dealt with on contact / on sight
                // > "common enemies" still team up and work with each other
                return requesterID != -1;

            case 0:
                // "true neutral" reserve for something non-threatening, but potential blocker to some actions
                // > roadblock, obstacle, etc.
                return false;
        }

        bool isObstacle = obstructIDs.Contains(requesterID);
        bool isAlly = allyIDs.Contains(requesterID);

        return !isAlly && !isObstacle;
    }

    // asking if this entity has to be dealt with "later"; being hindered by it (eg. roadblock, actual obstruction)
    public bool CheckObstacle(int requesterID)
    {
        switch (ownerID)
        {
            case -1:
                return false;

            case 0:
                return true;
        }

        return obstructIDs.Contains(requesterID);
    }

}
