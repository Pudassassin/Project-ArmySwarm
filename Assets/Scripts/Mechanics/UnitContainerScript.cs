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
        public List<ReserveTicket> reserveTickets = new List<ReserveTicket>();
        
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

    // debug temp
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
    public bool AllocateSlot(UnitStatsBasicSO unitData, out UnitTypeSlot typeSlotRef)
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
    public void AddUnit(UnitTypeSlot typeSlotRef, int count)
    {
        // Add directly to specified slot, after the checks
        // + recursion atom
        if (typeSlotRef == null)
        {
            Debug.LogError($"UnitContainer.AddUnit - Attempted to add to a NULL slot!\n\"{gameObject.name}\" [{transform.position}]\n");
            return;
        }
        if (count <= 0)
        {
            Debug.LogError($"UnitContainer.AddUnit - Invalid amount to add: {count}\n\"{gameObject.name}\" [{transform.position}]\n");
            return;
        }

        if (unitTypeList.Contains(typeSlotRef))
        {
            typeSlotRef.headCount += count;
        }
        else
        {
            Debug.LogError($"UnitContainer.AddUnit - Attempted to add a unit to a slot not belong to this GameObject:\n\"{gameObject.name}\" [{transform.position}]\n");
        }
    }

    // MACRO method
    public void AddUnit(UnitStatsBasicSO unitData, int count, out UnitTypeSlot typeSlotRef)
    {
        // MACRO method
        // 1> allocate [UNIT] slot
        // 2> add fresh new [UNIT](s)

        typeSlotRef = null;
        AllocateSlot(unitData, out typeSlotRef);
        AddUnit(typeSlotRef, count);
    }


    //==============================================
    // Methods to remove and deduce directly from container
    public bool RemoveUnit(UnitTypeSlot typeSlotRef, int count, out int actualCount)
    {
        // Remove directly from a specified slot, after the checks
        // + recursion atom
        bool isSuccess = false;
        actualCount = 0;

        if (typeSlotRef == null)
        {
            Debug.LogError($"UnitContainer.RemoveUnit - Attempted to remove from a NULL slot!\n\"{gameObject.name}\" [{transform.position}]\n");
            return false;
        }
        if (count <= 0)
        {
            Debug.LogError($"UnitContainer.RemoveUnit - Invalid amount to remove: {count}\n\"{gameObject.name}\" [{transform.position}]\n");
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
            Debug.LogError($"UnitContainer.RemoveUnit - Attempted to remove from a slot not belong to this GameObject:\n\"{gameObject.name}\" [{transform.position}]\n");
        }

        return isSuccess;
    }

    // MACRO method
    public bool RemoveUnit(UnitStatsBasicSO unitData, int count, out UnitTypeSlot typeSlotRef, out int actualCount)
    {
        // return BOOL whether or not the request can pull out at least one head of the specific [UNIT] type.
        bool isSuccess = false;
        actualCount = 0;
        typeSlotRef = null;

        if (QueryUnit(unitData, out typeSlotRef))
        {
            isSuccess = RemoveUnit(typeSlotRef, count, out actualCount);
        }

        return isSuccess;
    }


    //==============================================
    // Methods involve around unit reservation

    // Check reserve ticket's validation
    public bool CheckReserve(UnitTypeSlot typeSlotRef, ReserveTicket ticket)
    {
        // return true if ticket is indeed linked to the slot reference AND the slot itself belong to this container
        // bool isSuccess = false;

        if (typeSlotRef == null || ticket == null)
        {
            return false;
        }
        else if (!unitTypeList.Contains(typeSlotRef))
        {
            return false;
        }
        else if (!typeSlotRef.reserveTickets.Contains(ticket))
        {
            return false;
        }

        return true;
    }


    // Remove reservation from a slot
    public bool CancelReserve(UnitTypeSlot typeSlotRef, ReserveTicket ticket)
    {
        // return BOOL true, if reservation is cancelled, without issue

        if (CheckReserve(typeSlotRef, ticket))
        {
            typeSlotRef.reserveTickets.Remove(ticket);
            return true;
        }
        
        return false;
    }


    //==============================================
    // Methods to make reservation 'ticket'
    public bool MakeReserve(UnitTypeSlot typeSlotRef, int count, object requesterObj, out int actualCount, out ReserveTicket ticket, bool ignoreLimit = false)
    {
        // return BOOL when reservation is made for at least one unit, without issue (respecting limitor flag)
        bool isSuccess = false;
        actualCount = 0;
        ticket = null;

        if (typeSlotRef == null)
        {
            Debug.LogError($"UnitContainer.RemoveUnit - Attempted to reserve from a NULL slot!\n\"{gameObject.name}\" [{transform.position}]\n");
            return false;
        }
        if (count <= 0)
        {
            Debug.LogError($"UnitContainer.RemoveUnit - Invalid amount to reserve: {count}\n\"{gameObject.name}\" [{transform.position}]\n");
            return false;
        }

        int readyCount = typeSlotRef.readyCount;
        if (ignoreLimit || readyCount >= count)
        {
            actualCount = count;
        }
        else if (readyCount > 0)
        {
            actualCount = readyCount;
        }

        if (actualCount > 0)
        {
            ticket = new ReserveTicket(requesterObj, actualCount);
            typeSlotRef.reserveTickets.Add(ticket);
            isSuccess = true;
        }

        return isSuccess;
    }

    // MACRO method
    public bool MakeReserve(UnitStatsBasicSO unitData, int count, object requesterObj, out UnitTypeSlot typeSlotRef, out int actualCount, out ReserveTicket ticket, bool ignoreLimit = false)
    {
        // return BOOL true, when reservation is made for at least one unit, without issue (respecting limitor flag)
        bool isSuccess = false;
        actualCount = 0;
        ticket = null;

        if (QueryUnit(unitData, out typeSlotRef))
        {
            isSuccess = MakeReserve(typeSlotRef, count, requesterObj, out actualCount, out ticket, ignoreLimit);
        }

        return isSuccess;
    }

    //==============================================
    // Methods to pull from reservation, redeeming 'ticket'
    // >> potentially be use repeatedly
    public bool ClaimReserve(UnitTypeSlot typeSlotRef, ReserveTicket ticket, int pullCount, out int actualPullCount, bool ignoreInsufficent = true)
    {
        // return BOOL true, if reservation is valid and has pull at least one unit from a container, without issue
        bool isSuccess = false;
        actualPullCount = 0;

        if (CheckReserve(typeSlotRef, ticket))
        {
            actualPullCount = Mathf.Min(ticket.reserveCount, pullCount);

            if (typeSlotRef.headCount >= actualPullCount)
            {
                typeSlotRef.headCount -= actualPullCount;
                ticket.reserveCount -= actualPullCount;

                isSuccess = true;
            }
            else if (typeSlotRef.headCount > 0)
            {
                if (ignoreInsufficent)
                {
                    actualPullCount = typeSlotRef.headCount;
                    typeSlotRef.headCount = 0;
                    ticket.reserveCount -= actualPullCount;

                    isSuccess = true;
                }
                else
                {
                    actualPullCount = 0;
                }
            }
        }

        return isSuccess;
    }
}
