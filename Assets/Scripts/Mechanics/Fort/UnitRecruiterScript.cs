using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitRecruiterScript : MonoBehaviour
{
    // Handle solely on [UNIT] recruitment
    // (v0.1) handle only one unit type recruitment + fix rate
    // 
    // (future) dynamic changes depends on limiters / modifiers
    // (future) multiple recruitments in parallel

    // for tracking fort's [UNIT CONTAINER]
    public UnitContainerScript unitContainer;

    // type and stats of [UNIT] to be recruited
    public UnitStatsBasicSO recruitUnitData;

    // recruitment rate per second
    public float recruitRate = 1.0f;

    // internal vars
    float recruitTimer = 0.0f;
    float recruitInterval
    {
        get
        {
            return 1.0f / recruitRate;
        }
    }

    UnitContainerScript.UnitTypeSlot unitTypeSlot = null;

    void Start()
    {
        // check and associate components
        unitContainer = GetComponent<UnitContainerScript>();

        // (temp)

    }

    void Update()
    {
        /// tick the recruitment
        if (recruitTimer >= recruitInterval)
        {
            if (unitTypeSlot == null)
            {
                unitContainer.AddUnit(recruitUnitData, 1, out unitTypeSlot);
            }
            else
            {
                unitContainer.AddUnit(unitTypeSlot, 1);
            }

            recruitTimer -= recruitInterval;
        }
        recruitTimer += Time.deltaTime;

    }
}
