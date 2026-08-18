using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FortGarrisonScript : MonoBehaviour
{
    // Troop recruit
    // Troop housing (army reserve)
    // Sending out ally troops
    // Taking in ally troops
    // Combat resolve enemy troops
    // 

    // for tracking fort's army reserve
    public class TroopReserve
    {
        public UnitStatsBasicSO data;
        public int headCount = 0;
        public int busyCount = 0;
        
        public TroopReserve(UnitStatsBasicSO data)
        {
            this.data = data;
        }

    }

    // for tracking fort's ongoing troop deployment
    class TroopDeployOrder
    {
        public GameObject destination;
        public TroopReserve reserveRef;
        public int headCount;

        public int armyWidth;
        public float rowInterval;

        public float timer = 0.0f;
    }

    // for tracking in combat and recover troops
    class ActiveTroopData
    {
        public UnitStatsBasicSO data;
        public int hp;

        public int teamID;
        public Color teamColorLight, teamColorShade;

        public ActiveTroopData(GameObject troopObj)
        {
            UnitCombatScript combat = troopObj.GetComponent<UnitCombatScript>();
            TroopHandler handler = troopObj.GetComponent<TroopHandler>();

            data = handler.troopData;
            teamID = handler.teamID;
            teamColorLight = handler.teamColorLight;
            teamColorShade = handler.teamColorShade;

            hp = combat.hp;
        }

        public ActiveTroopData() { }
    }

    // base troop prefab
    public GameObject troopPrefab;

    // base unit production
    public UnitStatsBasicSO recruitUnit;

    // troops per second
    public float recruitRate = 1.0f;


    // troop capacity: soft and hard over-pop penalty on recruitment rate

    // troop reserve list
    List<TroopReserve> reserveList = new List<TroopReserve>();
    TroopReserve recruitRef = null;

    // troop deployment list (wip: one type + target, simutaneously)
    List<TroopDeployOrder> deployList = new List<TroopDeployOrder>();

    // resolving combat
    List<ActiveTroopData> invaderList = new List<ActiveTroopData>();

    // internal vars
    float recruitTimer = 0.0f;
    float recruitInterval;

    ActiveTroopData currentDefender = null;

    // fraction / team (wip)
    public int teamID = 0;
    public Color teamColorLight;
    public Color teamColorShade;

    // debug public vars
    public float fortRadius = 1.0f;
    public float troopGap = 0.025f;
    public GameObject testTarget;
    public TextMeshProUGUI textUI;

    void Start()
    {
        // dev placeholder
        AddToTroopReserve(recruitUnit, out recruitRef, count: 10);

        TroopDeployOrder order = new TroopDeployOrder();
        order.reserveRef = recruitRef;
        order.armyWidth = 5;
        order.rowInterval = 1.0f;
        order.destination = testTarget;

        deployList.Add(order);
    }

    // Update is called once per frame
    void Update()
    {
        /// tick the recruitment
        recruitInterval = 1.0f / recruitRate;
        if (recruitTimer >= recruitInterval)
        {
            
            if (recruitRef == null || recruitRef.data.troopName != recruitUnit.troopName)
            {
                // scan list / update ref
                AddToTroopReserve(recruitUnit, out recruitRef);
            }
            else
            {
                // add new troop
                recruitRef.headCount++;
            }

            recruitTimer -= recruitInterval;
        }
        recruitTimer += Time.deltaTime;

        /// tick the deployment(s)
        for (int i = deployList.Count -1 ; i >= 0; i--)
        {
            if (deployList[i].timer >= deployList[i].rowInterval)
            {
                // check reserve and update
                int draftCount;
                if (deployList[i].reserveRef.headCount > deployList[i].armyWidth)
                {
                    deployList[i].reserveRef.headCount -= deployList[i].armyWidth;
                    draftCount = deployList[i].armyWidth;
                }
                else
                {
                    draftCount = deployList[i].reserveRef.headCount;
                    deployList[i].reserveRef.headCount = 0;
                }

                // update the reservation here

                // spawn troop GameObjects in formation
                // wip: use debug public vars
                Vector3 vecToTarget = deployList[i].destination.transform.position - transform.position;
                Vector3 vecToRight = Vector3.Cross(vecToTarget, new Vector3(0, 0, 1)).normalized;

                float troopDistancing = deployList[i].reserveRef.data.crowdRepelRadius * 2.0f + troopGap;
                Vector3 rightmostPos = (vecToTarget.normalized * (fortRadius + troopDistancing)) + ((float)(draftCount - 1) * 0.5f * troopDistancing * vecToRight);

                for (int spawn = 0; spawn < draftCount; spawn++)
                {
                    Vector3 pos = transform.position + (troopDistancing * spawn * -vecToRight) + rightmostPos;

                    // spawn troop (wip: prototype prefabs)
                    GameObject troopObj = Instantiate(troopPrefab);
                    troopObj.transform.position = pos;
                    
                    // setup troop
                    TroopHandler handler = troopObj.GetComponent<TroopHandler>();
                    handler.troopData = deployList[i].reserveRef.data;
                    handler.teamID = teamID;
                    handler.teamColorLight = teamColorLight;
                    handler.teamColorShade = teamColorShade;
                    handler.marchTarget = deployList[i].destination;
                    handler.Setup();
                }

                deployList[i].timer -= deployList[i].rowInterval;
            }

            deployList[i].timer += Time.deltaTime;

            // clean up resolved deploy orders

        }

        /// resolve against invaders
        // loop invaders
        while (invaderList.Count > 0)
        {
            ActiveTroopData currentInvader = invaderList[0];
            invaderList.RemoveAt(0);

            // invader to conquered reserve
            if (currentInvader.teamID == teamID)
            {
                // wip: go directly to reserve, full recovery
                AddToTroopReserve(currentInvader.data, out _);
                continue;
            }

            // resolve combat
            bool takeover = false;
            while (currentInvader.hp > 0)
            {
                // pull from reserve
                if (currentDefender == null)
                {
                    for (int i = 0; i < reserveList.Count; i++)
                    {
                        if (reserveList[i].headCount > 0)
                        {
                            UnitStatsBasicSO data = reserveList[i].data;
                            currentDefender = new ActiveTroopData();

                            currentDefender.data = data;
                            currentDefender.hp = data.maxHP;

                            currentDefender.teamID = teamID;
                            currentDefender.teamColorLight = teamColorLight;
                            currentDefender.teamColorShade = teamColorShade;

                            reserveList[i].headCount--;
                            break;
                        }
                    }
                    if (currentDefender == null)
                    {
                        takeover = true;
                        break;
                    }
                }

                // calculate outcome
                ResolveTroopDM(currentInvader, currentDefender);
                if (currentDefender.hp <= 0)
                {
                    currentDefender = null;
                }
            }

            // invader take over
            if (takeover)
            {
                teamID = currentInvader.teamID;
                teamColorLight = currentInvader.teamColorLight;
                teamColorShade = currentInvader.teamColorShade;

                AddToTroopReserve(currentInvader.data, out _);
            }
        }

        // (mock-up) UI
        string uiString = "Troops\n";
        for (int i = 0; i < reserveList.Count; i++)
        {
            uiString += reserveList[i].data.troopName + " x " + reserveList[i].headCount + "\n";
        }
        textUI.text = uiString;

        // (mock-up) update sprite
    }

    // custom methods
    public void AddToTroopReserve(UnitStatsBasicSO troopData, out TroopReserve reserveRef, int count = 1)
    {
        reserveRef = null;
        int index = 0;
        int insertPos = 0;

        for (; index < reserveList.Count; index++)
        {
            if (troopData.troopName == reserveList[index].data.troopName)
            {
                reserveRef = reserveList[index];
                break;
            }

            if (troopData.garrisonPriority <= reserveList[index].data.garrisonPriority)
            {
                insertPos = index + 1;
            }
        }

        if (index >= reserveList.Count)
        {
            reserveRef = new TroopReserve(troopData);
            reserveList.Insert(insertPos, reserveRef);
        }

        reserveRef.headCount += count;
    }

    public void TakeTroopAlly(GameObject troopObj)
    {
        // wip: go directly to reserve, full recovery, remove from game
        TroopHandler handler = troopObj.GetComponent<TroopHandler>();
        AddToTroopReserve(handler.troopData, out _);

        // wip
        Destroy(troopObj);
    }

    public void TakeTroopEnemy(GameObject troopObj)
    {
        // wip: add to 'invader' list, to be resolved
        ActiveTroopData invaderData = new ActiveTroopData(troopObj);
        invaderList.Add(invaderData);

        // wip
        Destroy(troopObj);
    }
    
    // [issue order] Send out troops (wip: fixed waypoint, identical deployment patterns, in burst)

    void ResolveTroopDM(ActiveTroopData troopA, ActiveTroopData troopB)
    {
        // gather and cast
        float troopA_HP = troopA.hp;
        float troopA_melee = troopA.data.meleePower;

        float troopB_HP = troopB.hp;
        float troopB_melee = troopB.data.meleePower;

        // resolve atk=0 case

        // calculate outcome
        int troopA_hits = Mathf.CeilToInt(troopB_HP / troopA_melee);
        int troopB_hits = Mathf.CeilToInt(troopA_HP / troopB_melee);
        int resultHits = Mathf.Min(troopA_hits, troopB_hits);

        troopA.hp -= troopB.data.meleePower * resultHits;
        troopB.hp -= troopA.data.meleePower * resultHits;
    }
}
