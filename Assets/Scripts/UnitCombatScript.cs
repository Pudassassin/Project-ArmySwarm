using System.Collections.Generic;
using UnityEngine;

public class UnitCombatScript : MonoBehaviour
{
    class MeleeGraceData
    {
        public GameObject entity;
        public float duration;

        public MeleeGraceData(GameObject entity, float duration)
        {
            this.entity = entity;
            this.duration = duration;
        }

        public bool Tick()
        {
            duration -= Time.deltaTime;
            return duration < 0;
        }
    }

    // temp
    public int teamID;
    public int maxHP = 5;
    public int meleePower = 0;
    // public int rangedPower = 0;

    public float knockbackForce = 0.25f;
    public float knockbackDuration = 0.3f;

    public float graceTime = 0.5f;

    // var
    [HideInInspector]
    public int hp;
    List<MeleeGraceData> graceList = new List<MeleeGraceData>();

    UnitMovementScript moveScript;
    CrowdPhysicScript crowdScript;

    List<GameObject> engageList = new List<GameObject>();
    List<GameObject> engageListResolve = new List<GameObject>();

    GameObject fortEntryTarget = null;

    void OnEnable()
    {
        hp = maxHP;
        moveScript = gameObject.AddComponent<UnitMovementScript>();
        crowdScript = gameObject.GetComponent<CrowdPhysicScript>();

        // lookup and store manager script (actually just refer to class' static property)
    }

    void Update()
    {
        // clean up
        for (int i = engageList.Count - 1; i >= 0; i--)
        {
            if (engageList[i] == null)
            {
                engageList.RemoveAt(i);
            }
        }

        // copy list for resolve
        engageListResolve.Clear();
        for (int i = 0; i < engageList.Count; i++)
        {
            engageListResolve.Add(engageList[i]);
        }

        if (fortEntryTarget == null)
        {
            for (int i = 0; i < engageListResolve.Count; i++)
            {
                // same team!!
                UnitCombatScript otherCombat = engageListResolve[i].GetComponent<UnitCombatScript>();
                if (otherCombat.teamID == teamID)
                {
                    continue;
                }

                // ignore troops entering fort
                if (otherCombat.fortEntryTarget != null)
                {
                    continue;
                }

                // check for individual grace period
                bool skipFlag = false;
                for (int j = 0; j < graceList.Count; j++)
                {
                    if (engageListResolve[i] == graceList[j].entity)
                    {
                        // delay the previously hit entity
                        skipFlag = true;
                        break;
                    }
                }

                if (skipFlag)
                {
                    continue;
                }

                // send hit data to the manager
                MeleeHitManager.instance.RegisterMeleeHit(gameObject, engageListResolve[i]);
            }
        }

        // tick grace timers
        for (int i = graceList.Count - 1; i >= 0; i--)
        {
            if (graceList[i].entity == null)
            {
                graceList.RemoveAt(i);
            }
            else if (graceList[i].Tick() == true)
            {
                graceList.RemoveAt(i);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D otherCol)
    {
        /// Troop vs Fort
        if (fortEntryTarget == null)
        {
            FortGarrisonScript fortScript = otherCol.gameObject.GetComponent<FortGarrisonScript>();
            if (fortScript != null)
            {
                // only enter TARGETTED ally fort
                if ((fortScript.teamID == teamID) && moveScript.targetObject == fortScript.gameObject)
                {
                    MeleeHitManager.instance.RegisterFortEntry(gameObject, otherCol.gameObject);
                    fortEntryTarget = otherCol.gameObject;
                }

                // force to enter ANY enemy fort
                if (fortScript.teamID != teamID)
                {
                    MeleeHitManager.instance.RegisterFortEntry(gameObject, otherCol.gameObject);
                    fortEntryTarget = otherCol.gameObject;
                }
            }
        }

        /// Continuous Troop vs Troop check
        UnitCombatScript combatScript = otherCol.gameObject.GetComponent<UnitCombatScript>();
        if (combatScript == null)
        {
            return;
        }

        engageList.Add(otherCol.gameObject);
    }

    void OnTriggerExit2D(Collider2D otherCol)
    {
        engageList.Remove(otherCol.gameObject);
    }

    void LateUpdate()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    // custom methods
    public void TakeMeleeHit(UnitCombatScript sourceCombat)
    {
        // take damage
        hp -= sourceCombat.meleePower;

        // resolve hit effect: grace time
        float graceTime = Mathf.Max(this.graceTime, sourceCombat.graceTime);
        graceList.Add(new MeleeGraceData(sourceCombat.gameObject, graceTime));

        // resolve hit effect: knockback
        Vector3 kbVector = transform.position - sourceCombat.transform.position;
        kbVector.Scale(new Vector3(1, 1, 0));
        kbVector.Normalize();

        crowdScript.ApplyKnockback(sourceCombat.gameObject, knockbackForce * kbVector, crowdScript.weight, knockbackDuration);
    }
}
