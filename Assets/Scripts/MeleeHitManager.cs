using System.Collections.Generic;
using UnityEngine;

public class MeleeHitManager : MonoBehaviour
{
    public class MeleeHitData
    {
        public GameObject entityA, entityB;
        // public float graceTimer;
        // public bool isResolved = false;

        Vector3 posA, posB;

        public MeleeHitData(GameObject entityA, GameObject entityB)
        {
            this.entityA = entityA;
            this.entityB = entityB;
            posA = entityA.transform.position;
            posB = entityB.transform.position;
            // this.graceTimer = graceTimer;
        }

        public static bool CheckPair(MeleeHitData data, GameObject entityA, GameObject entityB)
        {
            if (data.entityA == entityA && data.entityB == entityB) return true;
            else if (data.entityA == entityB && data.entityB == entityA) return true;
            return false;
        }

        public static bool CheckPairInList(List<MeleeHitData> list, GameObject entityA, GameObject entityB)
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

        public static bool CheckEntity(MeleeHitData data, GameObject entity)
        {
            return (data.entityA == entity || data.entityB == entity);
        }

        public static bool CheckEntityInList(List<MeleeHitData> list, GameObject entity)
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
        
        // public static void CleanupList(ref List<MeleeHitData> list)
        // {
        //     for (int i = list.Count - 1; i >= 0; i--)
        //     {
        //         if (list[i].graceTimer <= 0.0f)
        //         {
        //             list.RemoveAt(i);
        //         }
        //     }
        // }
    }

    // Singleton
    public static MeleeHitManager instance = null;

    List<MeleeHitData> hitList = new List<MeleeHitData>();
    List<MeleeHitData> hitListToResolve = new List<MeleeHitData>();

    List<MeleeHitData> fortEntryList = new List<MeleeHitData>();
    List<MeleeHitData> fortEntryListToResolve = new List<MeleeHitData>();

    // [HideInInspector]
    // public List<MeleeHitData> hitListGrace = new List<MeleeHitData>();

    void Start()
    {
        // resolve conflict

        // assige new manager
        MeleeHitManager.instance = this;
    }

    void Update()
    {
        /// Copy lists to resolve
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

        /// Troop vs Fort
        // clean up seige troops from TvT list
        for (int i = hitListToResolve.Count - 1; i >= 0; i--)
        {
            bool removeFlag = MeleeHitData.CheckEntityInList(fortEntryListToResolve, hitListToResolve[i].entityA) ||
                              MeleeHitData.CheckEntityInList(fortEntryListToResolve, hitListToResolve[i].entityB);

            if (removeFlag)
            {
                hitListToResolve.RemoveAt(i);
            }

        }

        // iterate fort hits
        for (int i = 0; i < fortEntryListToResolve.Count; i++)
        {
            UnitCombatScript troopCombat = fortEntryListToResolve[i].entityA.GetComponent<UnitCombatScript>();
            FortGarrisonScript fortScript = fortEntryListToResolve[i].entityB.GetComponent<FortGarrisonScript>();

            if (troopCombat.teamID == fortScript.teamID)
            {
                fortScript.TakeTroopAlly(troopCombat.gameObject);
            }
            else
            {
                fortScript.TakeTroopEnemy(troopCombat.gameObject);
            }
        }



        /// Troop vs Troop
        // iterate the hit list
        for (int i = 0; i < hitListToResolve.Count; i++)
        {
            // skip interactons against 'dead' entity (temp)
            if (hitListToResolve[i].entityA == null || hitListToResolve[i].entityB == null)
            {
                continue;
            }

            UnitCombatScript combatA = hitListToResolve[i].entityA.GetComponent<UnitCombatScript>();
            UnitCombatScript combatB = hitListToResolve[i].entityB.GetComponent<UnitCombatScript>();

            // if either entity is dying (HP already <= 0), ignore the exchange
            if (combatA.hp <= 0 || combatB.hp <= 0)
            {
                continue;
            }

            float graceTime = Mathf.Max(combatA.graceTime, combatB.graceTime);

            // resolve combat
            combatA.TakeMeleeHit(combatB);
            combatB.TakeMeleeHit(combatA);

        }

        // 
    }

    // custom methods

    // careful with this one; potental frequent calls
    public bool RegisterMeleeHit(GameObject entityA, GameObject entityB)
    {
        if (entityA == null || entityB == null)
        {
            return false;
        }

        if (MeleeHitData.CheckPairInList(hitList, entityA, entityB))
        {
            // already in the list; prevent dupes / double regs
            return false;
        }

        // UnitCombatScript combatA = entityA.GetComponent<UnitCombatScript>();
        // UnitCombatScript combatB = entityB.GetComponent<UnitCombatScript>();
        // if (combatA.teamID == combatB.teamID)
        // {
        //     // same team!
        //     return false;
        // }

        hitList.Add(new MeleeHitData(entityA, entityB));
        return true;
    }


    public bool RegisterFortEntry(GameObject troopObj,  GameObject fortObj)
    {
        if (troopObj == null || fortObj == null)
        {
            return false;
        }

        if (MeleeHitData.CheckPairInList(fortEntryList, troopObj, fortObj))
        {
            // already in the list; prevent dupes / double regs
            return false;
        }

        fortEntryList.Add(new MeleeHitData(troopObj, fortObj));
        return true;
    }
}
