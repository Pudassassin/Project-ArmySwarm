using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PointerObjectScript : MonoBehaviour
{
    // centralized class for player's control GameObject: should be compatible with M&K and touchscreen inputs

    public TroopCountUIHandler troopCountUI;

    // handle fort selection

    List<GameObject> hoveredObjects = new List<GameObject>();

    List<GameObject> selectedObjects = new List<GameObject>();
    GameObject rallyTargetObject = null;

    bool modeDragSelect = false;
    int selectedTeamID = -1;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetStartDragSelect()
    {
        //wip
        SetDeselect();

        if (hoveredObjects.Count > 0)
        {
            selectedObjects.Add(hoveredObjects[0]);
            FortGarrisonScript garrisonScript = hoveredObjects[0].GetComponent<FortGarrisonScript>();
            selectedTeamID = garrisonScript.teamID;
            FortVisualScript visualScript = hoveredObjects[0].GetComponent<FortVisualScript>();
            visualScript.SetSelected(true);
        }

        modeDragSelect = true;
    }

    public void SetStopDragSelect()
    {
        modeDragSelect = false;

        // if end point is "enemy" fort, immediately issue attack rally (?)

    }

    public void SetDeselect()
    {
        foreach (GameObject fort in selectedObjects)
        {
            FortVisualScript visualScript = fort.GetComponent<FortVisualScript>();
            visualScript.SetSelected(false);
        }
        selectedObjects.Clear();
        selectedTeamID = -1;

        rallyTargetObject = null;
    }

    public void SetIssueRallyOrder()
    {
        // usually by right-clicking for mouse, explicit telling fort(s) to rally at target point
        if (hoveredObjects.Count > 0)
        {
            // (wip) currently will only cout forts into list of hover-over objects
            rallyTargetObject = hoveredObjects[0];

            foreach (GameObject fort in selectedObjects)
            {
                FortGarrisonScript garrisonScript = fort.GetComponent<FortGarrisonScript>();
                garrisonScript.OrderTroopRally(rallyTargetObject, troopCountUI.SendValue, troopCountUI.SendAbsoluteActive, troopCountUI.SendAll);
            }
        }

    }

    void OnTriggerEnter2D(Collider2D otherCol)
    {
        // filter to select forts
        bool isFort = false;
        bool isRallyPoint = false;

        FortGarrisonScript garrisonScript = otherCol.gameObject.GetComponent<FortGarrisonScript>();
        if (garrisonScript == null)
        {
            // for now
            return;
        }

        FortVisualScript visualScript = otherCol.gameObject.GetComponent<FortVisualScript>();
        bool addToSelected = false;

        if (modeDragSelect)
        {
            if (selectedObjects.Count == 0)
            {
                addToSelected = true;
                selectedTeamID = garrisonScript.teamID;
            }
            else if (garrisonScript.teamID == selectedTeamID)
            {
                if (!selectedObjects.Contains(otherCol.gameObject))
                {
                    addToSelected = true;
                }
            }

            if (addToSelected)
            {
                selectedObjects.Add(otherCol.gameObject);
                visualScript.SetSelected(true);
            }
        }

        hoveredObjects.Add(otherCol.gameObject);
    }

    void OnTriggerExit2D(Collider2D otherCol)
    {
        hoveredObjects.Remove(otherCol.gameObject);
    }
}
