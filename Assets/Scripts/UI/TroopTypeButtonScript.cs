using TMPro;
using UnityEngine;

public class TroopTypeButtonScript : MonoBehaviour
{
    // Simple script to attach to UI buttons, that controls troop type to be deployed and rallied as part of the army
    // holds stats and button trigger, as well as updating text to the context

    // To be handled by a handler script for the UI/UX section for troop type control

    public UnitStatsBasicSO assignedType;

    public bool allowRally = false;

    // public scriptClass troopTypeHandler

    TextMeshProUGUI text_troopName;

    void Start()
    {
        text_troopName = GetComponentInChildren<TextMeshProUGUI>();
        text_troopName.text = assignedType.troopName;
    }

    void Update()
    {
        
    }

    // button method
    public void OnPressed()
    {
        // (wip)
        allowRally = !allowRally;

        if (allowRally)
        {
            text_troopName.text = $"[{assignedType.troopName}]";
        }
        else
        {
            text_troopName.text = assignedType.troopName;
        }

        // next step: pass along the message
    }
}
