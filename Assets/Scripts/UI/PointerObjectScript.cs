using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PointerObjectScript : MonoBehaviour
{
    // handle fort selection

    List<GameObject> hoveredObjects = new List<GameObject>();
    List<GameObject> selectedObjects = new List<GameObject>();

    bool modeDragSelect = false;
    int selectedTeamID = -1;

    void Start()
    {
        
    }

    // Update is called once per frame
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
    }

    void OnTriggerEnter2D(Collider2D otherCol)
    {
        // filter to select forts
        FortGarrisonScript garrisonScript = otherCol.gameObject.GetComponent<FortGarrisonScript>();
        if (garrisonScript == null)
        {
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
