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
        string uiString = "Contain Unit:\n";
        for (int i = 0; i < unitTypeList.Count; i++)
        {
            uiString += unitTypeList[i].data.troopName + " x " + unitTypeList[i].headCount + "\n";
        }
        textUI.text = uiString;

        // (mock-up) update sprite (trigger every frame)

    }

    //=================================
    // CUSTOM METHODS
    //=================================

    // Look into the list for the specific [UNIT] type and do nothing else
    // > return BOOL search result
    // > OUTput the slot reference; result or NULL
    public bool QueryUnit(UnitStatsBasicSO unitData, out UnitTypeSlot typeSlotRef)
    {
        bool isSuccess = false;
        typeSlotRef = null;

        for (int i = 0; i < unitTypeList.Count; i++)
        {
            if (unitTypeList[i].data == unitData)
            {
                typeSlotRef = unitTypeList[i];

                isSuccess = true;

                break;
            }
        }

        return isSuccess;
    }

    // Try to allocate a fresh new slot for [UNIT] type if not already exist
    // > return BOOL search result
    // > OUTput the slot reference; existing or new one
    // > insertion position based on 'tactical priority'
    public bool Allocate(UnitStatsBasicSO unitData, out UnitTypeSlot typeSlotRef)
    {
        bool isAllocated = false;
        typeSlotRef = null;
        int index = 0;
        int insertPos = 0;

        for (; index < unitTypeList.Count; index++)
        {
            if (unitData == unitTypeList[index].data)
            {
                typeSlotRef = unitTypeList[index];
                break;
            }

            if (unitData.garrisonPriority <= unitTypeList[index].data.garrisonPriority)
            {
                insertPos = index + 1;
            }
        }

        if (index >= unitTypeList.Count)
        {
            typeSlotRef = new UnitTypeSlot(unitData);
            unitTypeList.Insert(insertPos, typeSlotRef);
            isAllocated = true;
        }

        return isAllocated;
    }

    //==============================================
    // Methods to add [UNIT] directly to container
    public void AddUnit(UnitTypeSlot typeSlotRef, int count = 1)
    {
        // Add directly to specified slot, after the checks
        // + recursion atom
        if (typeSlotRef == null)
        {
            Debug.LogWarning("UnitContainer.AddUnit - Attempted to add a unit to a NULL slot!");
            return;
        }
        if (count <= 0)
        {
            Debug.LogWarning($"UnitContainer.AddUnit - Invalid amount to add to the container: {count}");
            return;
        }

        if (unitTypeList.Contains(typeSlotRef))
        {
            typeSlotRef.headCount += count;
        }
        else
        {
            Debug.LogWarning($"UnitContainer.AddUnit - Attempted to add a unit to a slot not belong to this GameObject:\n\"{gameObject.name}\" [{transform.position}]\n");
        }
    }

    // MACRO method
    public void AddUnit(UnitStatsBasicSO unitData, out UnitTypeSlot typeSlotRef, int count = 1)
    {
        // MACRO method
        // 1> allocate [UNIT] slot
        // 2> add fresh new [UNIT](s)

        typeSlotRef = null;
        Allocate(unitData, out typeSlotRef);
        AddUnit(typeSlotRef, count);
    }


    //==============================================
    // Methods to remove and deduce directly from container
    public bool RemoveUnit(UnitTypeSlot typeSlotRef, out int actualCount, int count = 1)
    {
        // Remove directly from a specified slot, after the checks
        // + recursion atom
        bool isSuccess = false;
        actualCount = 0;

        if (typeSlotRef == null)
        {
            Debug.LogWarning("UnitContainer.RemoveUnit - Attempted to remove a unit from a NULL slot!");
            return false;
        }

        if (unitTypeList.Contains(typeSlotRef))
        {
            if (typeSlotRef.headCount <= 0)
            {
                actualCount = 0;
                isSuccess = false;
            }
            else if (typeSlotRef.headCount < count)
            {
                actualCount = typeSlotRef.headCount;
                typeSlotRef.headCount = 0;
                isSuccess = true;
            }
            else
            {
                actualCount = count;
                typeSlotRef.headCount -= count;
                isSuccess = true;
            }
        }
        else
        {
            Debug.LogWarning($"UnitContainer.RemoveUnit - Attempted to remove a unit from a slot not belong to this GameObject:\n\"{gameObject.name}\" [{transform.position}]\n");
        }

        return isSuccess;
    }

    // MACRO method
    public bool RemoveUnit(UnitStatsBasicSO unitData, out UnitTypeSlot typeSlotRef, out int actualCount, int count = 1)
    {
        // return BOOL whether or not the request can pull out at least one head of the specific [UNIT] type.
        bool isSuccess = false;
        actualCount = 0;
        typeSlotRef = null;

        if (QueryUnit(unitData, out typeSlotRef))
        {
            isSuccess = RemoveUnit(typeSlotRef, out actualCount, count);
        }

        return isSuccess;
    }


    //==============================================
    // Methods to make reservation 'ticket'

}
