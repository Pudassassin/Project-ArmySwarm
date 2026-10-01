using System.Collections.Generic;
using UnityEngine;

public class MeleeCombatScript : MonoBehaviour
{
    // Handle exclusively [MELEE COMBAT] and [FORT INVASION] registers via contact collider
    // > also handle grace-period as part of [MELEE COMBAT] post-resolution

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
    public int damage = 0;

    public float knockbackForce = 0.25f;
    public float knockbackDuration = 0.3f;

    public float graceTime = 0.5f;

    // association / dependency
    OwnershipScript ownership;
    HealthHandler healthHandler;
    CrowdPhysicScript crowdPhysic;
    UnitMovementScript unitMovement;

    // var
    List<MeleeGraceData> graceList = new List<MeleeGraceData>();

    List<GameObject> engageList = new List<GameObject>();
    List<GameObject> engageListResolve = new List<GameObject>();

    GameObject fortSoonToEnterTarget = null;

    //====================================
    // Unity Messages
    void Start()
    {
        ownership = gameObject.GetComponent<OwnershipScript>();
        healthHandler = gameObject.GetComponent<HealthHandler>();
        crowdPhysic = gameObject.GetComponent<CrowdPhysicScript>();
        unitMovement = gameObject.GetComponent<UnitMovementScript>();
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

        if (fortSoonToEnterTarget == null)
        {
            for (int i = 0; i < engageListResolve.Count; i++)
            {
                // check if hostile
                OwnershipScript otherOwner = engageListResolve[i].GetComponent<OwnershipScript>();
                if (!otherOwner.CheckHostile(ownership.ownerID))
                {
                    continue;
                }

                // ignore troops entering fort
                MeleeCombatScript otherCombat = engageListResolve[i].GetComponent<MeleeCombatScript>();
                if (otherCombat.fortSoonToEnterTarget != null)
                {
                    continue;
                }

                // check for grace period
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
                MeleeCombatManager.instance.RegisterMeleeHit(gameObject, engageListResolve[i]);
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

    // Enlist anything that matters, entering colliders:
    // [UNIT], [FORT]
    void OnTriggerEnter2D(Collider2D otherCol)
    {
        OwnershipScript otherOwner = otherCol.GetComponent<OwnershipScript>();
        FortHandler fortHandler = otherCol.gameObject.GetComponent<FortHandler>();
        MeleeCombatScript otherCombat = otherCol.gameObject.GetComponent<MeleeCombatScript>();

        /// Troop vs Fort
        if (fortHandler != null)
        {
            if (fortSoonToEnterTarget == null)
            {
                // (future) alliance interaction to be considered

                if (otherOwner.CheckHostile(ownership.ownerID))
                {
                    // force to enter ANY enemy fort
                    MeleeHitManager.instance.RegisterFortEntry(gameObject, otherCol.gameObject);
                    fortSoonToEnterTarget = otherCol.gameObject;
                }
                else if (unitMovement.targetObject == otherCol.gameObject)
                {
                    // only enter TARGETTED ally fort
                    MeleeHitManager.instance.RegisterFortEntry(gameObject, otherCol.gameObject);
                    fortSoonToEnterTarget = otherCol.gameObject;
                }
            }
        }

        /// Continuous Troop vs Troop check
        if (otherCombat == null)
        {
            return;
        }
        else
        {
            if (fortHandler == null)
            {
                engageList.Add(otherCol.gameObject);
            }
        }

    }

    // Delist anything exiting colliders
    void OnTriggerExit2D(Collider2D otherCol)
    {
        engageList.Remove(otherCol.gameObject);
    }

    void LateUpdate()
    {

    }

    //====================================
    // Custom methods


    //====================================
    // Taking melee hit, no special effects
    public void TakeHit(GameObject sourceObject)
    {
        MeleeCombatScript sourceCombat = sourceObject.GetComponent<MeleeCombatScript>();
        CrowdPhysicScript sourceCrowd = sourceObject.GetComponent<CrowdPhysicScript>();

        // take damage (v0.1)
        healthHandler.TakeDamage(sourceCombat.damage);

        // resolve hit effect: grace time
        float graceTime = Mathf.Max(this.graceTime, sourceCombat.graceTime);
        graceList.Add(new MeleeGraceData(sourceObject, graceTime));

        // resolve hit effect: knockback
        Vector3 kbVector = transform.position - sourceObject.transform.position;
        kbVector.Scale(new Vector3(1, 1, 0));
        kbVector.Normalize();

        crowdPhysic.TakeKnockback(knockbackForce * kbVector, sourceCrowd.weight, knockbackDuration);
    }
}
