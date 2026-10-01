using System.Collections.Generic;
using UnityEngine;

public class MeleeCombatManager : MonoBehaviour
{
    // Centralized and singleton class handling the [MELEE COMBAT]s of the game
    // > trade melee hits on units' collision; simultaneously!
    // ! grace period is consequence of [MELEE COMBAT] resolution
    //
    // > handle melee-hit exclusion on conditions:
    //   > Entering [FORT] takes priority


    // Local data structure
    public class CombatHitData
    {
        public GameObject entityA, entityB;

        public CombatHitData(GameObject entityA, GameObject entityB)
        {
            this.entityA = entityA;
            this.entityB = entityB;
        }

        public static bool CheckPair(CombatHitData data, GameObject entityA, GameObject entityB)
        {
            if (data.entityA == entityA && data.entityB == entityB) return true;
            else if (data.entityA == entityB && data.entityB == entityA) return true;
            return false;
        }

        public static bool CheckPairInList(List<CombatHitData> list, GameObject entityA, GameObject entityB)
        {
            bool result = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (CheckPair(list[i], entityA, entityB))
                {
                    return true;
                }
            }
        
            return result;
        }

        public static bool CheckEntity(CombatHitData data, GameObject entity)
        {
            return (data.entityA == entity || data.entityB == entity);
        }

        public static bool CheckEntityInList(List<CombatHitData> list, GameObject entity)
        {
            bool result = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (CheckEntity(list[i], entity))
                {
                    return true;
                }
            }

            return result;
        }
    }

    // Singleton
    public static MeleeCombatManager instance = null;

    // Vars
    List<CombatHitData> hitList = new List<CombatHitData>();
    List<CombatHitData> hitListToResolve = new List<CombatHitData>();

    List<CombatHitData> fortEntryList = new List<CombatHitData>();
    List<CombatHitData> fortEntryListToResolve = new List<CombatHitData>();


    //====================================
    // Unity Messages
    void Start()
    {
        // resolve conflict

        // assige new manager
        instance = this;
    }

    void Update()
    {
        // Copy lists to resolve
        fortEntryListToResolve.Clear();
        int resolveCount = fortEntryList.Count;
        for (int i = 0; i < resolveCount; i++)
        {
            fortEntryListToResolve.Add(fortEntryList[i]);
        }
        fortEntryList.RemoveRange(0, resolveCount);

        hitListToResolve.Clear();
        resolveCount = hitList.Count;
        for (int i = 0; i < resolveCount; i++)
        {
            hitListToResolve.Add(hitList[i]);
        }
        hitList.RemoveRange(0, resolveCount);

        /// [UNIT] vs [FORT]
        //  > excluding 'invading' [UNIT] from melee hit list
        for (int i = hitListToResolve.Count - 1; i >= 0; i--)
        {
            bool removeFlag = CombatHitData.CheckEntityInList(fortEntryListToResolve, hitListToResolve[i].entityA) ||
                              CombatHitData.CheckEntityInList(fortEntryListToResolve, hitListToResolve[i].entityB);

            if (removeFlag)
            {
                hitListToResolve.RemoveAt(i);
            }

        }

        // iterate [FORT] hits
        for (int i = 0; i < fortEntryListToResolve.Count; i++)
        {
            OwnershipScript unitOwner = fortEntryListToResolve[i].entityA.GetComponent<OwnershipScript>();
            OwnershipScript fortOwner = fortEntryListToResolve[i].entityB.GetComponent<OwnershipScript>();
            UnitContainerScript fortContainer = fortEntryListToResolve[i].entityB.GetComponent<UnitContainerScript>();

            if (unitOwner.ownerID == fortOwner.ownerID)
            {
                // temp solution
                fortContainer.AddUnit(fortEntryListToResolve[i].entityA);
            }
            else
            {
                // to be implemented : invasion
                // for now just disable combat
                MeleeCombatScript combat = fortEntryListToResolve[i].entityA.GetComponent<MeleeCombatScript>();
                combat.enabled = false;
            }
        }

        /// [UNIT] vs [UNIT]
        //  > iterate the hit list
        for (int i = 0; i < hitListToResolve.Count; i++)
        {
            // skip interactons against 'dead' entity (temp)
            if (hitListToResolve[i].entityA == null || hitListToResolve[i].entityB == null)
            {
                continue;
            }

            MeleeCombatScript   unitA_Combat = hitListToResolve[i].entityA.GetComponent<MeleeCombatScript>();
            HealthHandler       unitA_Health = hitListToResolve[i].entityA.GetComponent<HealthHandler>();

            MeleeCombatScript   unitB_Combat = hitListToResolve[i].entityB.GetComponent<MeleeCombatScript>();
            HealthHandler       unitB_Health = hitListToResolve[i].entityB.GetComponent<HealthHandler>();

            // if either entity is dying (HP already <= 0), ignore the exchange
            if (unitA_Health.hp <= 0 || unitB_Health.hp <= 0)
            {
                continue;
            }

            // resolve combat
            unitA_Combat.TakeHit(unitB_Combat.gameObject);
            unitB_Combat.TakeHit(unitA_Combat.gameObject);

        }

    }

    //====================================
    // Custom Methods

    //====================================
    // Register combat events
    //====================================

    // careful with this one; potental frequent calls
    // > DO NOT CHECK for alliance here!
    public bool RegisterMeleeHit(GameObject entityA, GameObject entityB)
    {
        // return true if successfully registered
        if (entityA == null || entityB == null)
        {
            return false;
        }

        if (CombatHitData.CheckPairInList(hitList, entityA, entityB))
        {
            // already in the list; prevent dupes / double regs
            return false;
        }

        hitList.Add(new CombatHitData(entityA, entityB));
        return true;
    }


    public bool RegisterFortEntry(GameObject entityObj,  GameObject fortObj)
    {
        if (entityObj == null || fortObj == null)
        {
            return false;
        }

        if (CombatHitData.CheckPairInList(fortEntryList, entityObj, fortObj))
        {
            // already in the list; prevent dupes / double regs
            return false;
        }

        fortEntryList.Add(new CombatHitData(entityObj, fortObj));
        return true;
    }
}
