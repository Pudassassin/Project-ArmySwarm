using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UnitDeployerScript : MonoBehaviour
{
    // Handle solely on sending out [UNIT] over brief periods
    // [!] Hard dependency : [UNIT CONTAINER]
    // 
    // > reserve [UNIT]s and track deployment(s)
    //
    // (future) govern the deployment rate depends on limiter / stats / modifiers


    // base troop prefab
    public GameObject unitGO_Prefab;

    // for tracking fort's [UNIT CONTAINER]
    public UnitContainerScript unitContainer;

    // for tracking fort's ongoing troop deployment
    class UnitDeployOrder
    {
        public GameObject destination;
        public UnitContainerScript.UnitTypeSlot unitSlotRef;
        public UnitContainerScript.ReserveTicket reserveTicket;

        public int armyWidth;
        public float rowInterval;

        public float timer = 0.0f;
        public bool hasIssue = false;
    }

    // [UNIT] ongoing deployment list
    // v0.1 : one type + target, simutaneously without limit

    List<UnitDeployOrder> deployList = new List<UnitDeployOrder>();

    // [WIP]
    FortHandler fortHandler;
    OwnershipScript ownership;

    // debug public vars
    public float fortRadius = 1.0f;
    public float unitGap = 0.025f;

    void Start()
    {
        // check and associate components
        unitContainer = gameObject.GetComponent<UnitContainerScript>();
        fortHandler = gameObject.GetComponent<FortHandler>();
        ownership = gameObject.GetComponent<OwnershipScript>();

    }

    void Update()
    {
        /// tick the deployment(s)
        for (int i = 0; i < deployList.Count; i++)
        {
            if (deployList[i].timer >= deployList[i].rowInterval)
            {
                // determine number of troops to spawn
                int draftCount = 0;

                unitContainer.ClaimReserve(
                    deployList[i].unitSlotRef,
                    deployList[i].reserveTicket,
                    deployList[i].armyWidth,
                    out draftCount);

                if (draftCount <= 0)
                {
                    deployList[i].hasIssue = true;
                    continue;
                }
                
                // spawn troop GameObjects in formation
                // wip: use debug public vars
                Vector3 vecToTarget = deployList[i].destination.transform.position - transform.position;
                Vector3 vecToRight = Vector3.Cross(vecToTarget, new Vector3(0, 0, 1)).normalized;

                float unitDistancing = deployList[i].unitSlotRef.data.crowdRepelRadius * 2.0f + unitGap;
                Vector3 rightmostPos = (vecToTarget.normalized * (fortRadius + unitDistancing)) + ((float)(draftCount - 1) * 0.5f * unitDistancing * vecToRight);

                for (int spawn = 0; spawn < draftCount; spawn++)
                {
                    Vector3 pos = transform.position + (unitDistancing * spawn * -vecToRight) + rightmostPos;

                    // Spawn troop (wip: prototype prefabs)
                    GameObject unitObj = Instantiate(unitGO_Prefab);
                    unitObj.transform.position = pos;

                    // Setup Unit
                    UnitHandler handler = unitObj.GetComponent<UnitHandler>();
                    handler.unitData = deployList[i].unitSlotRef.data;

                    // > (wip) ownership assignment / data transfer
                    handler.ownerID = ownership.ownerID;
                    handler.teamColorLight = ownership.colorLight;
                    handler.teamColorMain  = ownership.colorMain;
                    handler.teamColorShade = ownership.colorShade;

                    // > set target
                    handler.rallyTarget = deployList[i].destination;
                    handler.Setup();
                }

                deployList[i].timer -= deployList[i].rowInterval;
            }

            deployList[i].timer += Time.deltaTime;
        }

        /// clean up completed / cannot complete deployment
        for (int i = deployList.Count - 1; i >= 0; i--)
        {
            bool removeFlag = false;

            if (deployList[i].hasIssue)
            {
                removeFlag = true;
            }
            else if (deployList[i].reserveTicket.reserveCount <= 0)
            {
                removeFlag = true;
            }

            // revoke reserve and remove from list
            if (removeFlag)
            {
                unitContainer.CancelReserve(
                    deployList[i].unitSlotRef,
                    deployList[i].reserveTicket);

                deployList.RemoveAt(i);
            }
        }

    }

    // custom methods
    public void OrderDeploy(GameObject targetObject, int number, bool isAbsolute, bool isSendAll = false)
    {
        UnitDeployOrder order = new UnitDeployOrder();
        int reserveCount = 0;

        // (wip) only viable destination is other forts
        order.destination = targetObject;

        // (wip) omitting unit type, always take the biggest 'available' headcount reserve
        order.unitSlotRef = null;

        int mostCount = 0;
        for (int i = 0; i < unitContainer.unitTypeList.Count; i++)
        {
            int headCount = unitContainer.unitTypeList[i].readyCount;
            if (headCount > mostCount)
            {
                order.unitSlotRef = unitContainer.unitTypeList[i];
                mostCount = headCount;
            }
        }

        if (order.unitSlotRef == null)
        {
            return;
        }

        // determine the rally headcount
        if (isSendAll || (isAbsolute && number >= mostCount))
        {
            reserveCount = mostCount;
        }
        else
        {
            if (!isAbsolute)
            {
                float currentCount = mostCount;
                float percentage = ((float)number) / 100.0f;
                reserveCount = Mathf.RoundToInt(currentCount * percentage);
            }
            else
            {
                reserveCount = number;
            }
        }

        if (reserveCount <= 0)
        {
            return;
        }

        // make reservation ticket
        unitContainer.MakeReserve(order.unitSlotRef, reserveCount, this, out _, out order.reserveTicket, ignoreLimit: true);

        // (wip) always rally in rows of five-troop formation, interval to keep troops grouped up
        order.armyWidth = 5;

        // (wip) adjust interval based on troop's speed and size
        float distancing = order.unitSlotRef.data.crowdRepelRadius * 2.2f;
        float speed = order.unitSlotRef.data.moveSpeed;
        order.rowInterval = distancing / speed;

        deployList.Add(order);
    }
}
