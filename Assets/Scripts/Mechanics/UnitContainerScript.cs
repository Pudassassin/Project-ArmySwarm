using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitContainerScript : MonoBehaviour
{
    // Represent and deal with [UNIT] storages and accounting
    // - [UNIT] types and amount (head count) of each
    // - logging and dealing with [UNIT] reservation

    public class ReserveTicket
    {
        public object requester = null;
        public int reserveCount = 0;

        public ReserveTicket(object requester, int reservedCount)
        {
            this.requester = requester;
            this.reserveCount = reservedCount;
        }
    }

    public class UnitTypeSlot
    {
        public UnitStatsBasicSO data;
        public int headCount = 0;

        // its data changes will be handled by external requester
        List<ReserveTicket> reserveTickets = new List<ReserveTicket>();
        
        public int busyCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < reserveTickets.Count; i++)
                {
                    count += reserveTickets[i].reserveCount;
                }
                return count;
            }
        }

        public int readyCount
        {
            get
            {
                return headCount - busyCount;
            }
        }

        public UnitTypeSlot(UnitStatsBasicSO data)
        {
            this.data = data;
        }

    }

    List<UnitTypeSlot> unitTypeList = new List<UnitTypeSlot>();

    public TextMeshProUGUI textUI;

    void Start()
    {
        
    }

    void Update()
    {
        // (mock-up) [UNIT CONTAINER] UI
        string uiString = "Troops\n";
        for (int i = 0; i < unitTypeList.Count; i++)
        {
            uiString += unitTypeList[i].data.troopName + " x " + unitTypeList[i].headCount + "\n";
        }
        textUI.text = uiString;

        // (mock-up) update sprite (trigger every frame)

    }

    // custom methods

    // Add directly to container
    public void AddUnit(UnitStatsBasicSO unitData, out UnitTypeSlot containerRef, int count = 1)
    {
        containerRef = null;
        int index = 0;
        int insertPos = 0;

        for (; index < unitTypeList.Count; index++)
        {
            if (unitData.troopName == unitTypeList[index].data.troopName)
            {
                containerRef = unitTypeList[index];
                break;
            }

            if (unitData.garrisonPriority <= unitTypeList[index].data.garrisonPriority)
            {
                insertPos = index + 1;
            }
        }

        if (index >= unitTypeList.Count)
        {
            containerRef = new UnitTypeSlot(unitData);
            unitTypeList.Insert(insertPos, containerRef);
        }

        containerRef.headCount += count;
    }

    // Remove and deduce directly from container, if available
    public bool RemoveUnit(UnitStatsBasicSO unitData, out UnitTypeSlot typeSlotRef, out int actualCount, int count = 1)
    {
        // whether or not the request can pull out at least one head of the specific [UNIT] type.
        bool isSuccess = false;
        actualCount = 0;
        typeSlotRef = null;

        for (int i = 0; i < unitTypeList.Count; i++)
        {
            if (unitTypeList[i].data == unitData)
            {
                typeSlotRef = unitTypeList[i];

                if (unitTypeList[i].headCount <= 0)
                {
                    actualCount = 0;
                    isSuccess = false;
                }
                else if (unitTypeList[i].headCount < count)
                {
                    actualCount = unitTypeList[i].headCount;
                    unitTypeList[i].headCount = 0;
                    isSuccess = true;
                }
                else
                {
                    actualCount = count;
                    unitTypeList[i].headCount -= count;
                    isSuccess = true;
                }

                break;
            }
        }

        return isSuccess;
    }


}
