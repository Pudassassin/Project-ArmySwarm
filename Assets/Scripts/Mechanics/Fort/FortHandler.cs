using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FortHandler : MonoBehaviour
{
    // The brain of the [FORT]'s logics and mechanics, co-ordinating:
    // > [UNIT CONTAINER]
    // > [UNIT] recruiter
    // > [UNIT] converter (next version)
    // > a script to handle [UNIT] rallying
    // > [INVADER COMBAT]
    // > a dedicate script to handle team control / ownership data (next version)

    // for tracking [UNIT]'s stored within
    UnitContainerScript unitContainer;

    // for handling [UNIT] recruitment
    // 

    // for tracking [UNIT] deployments
    // UnitDeployerScript unitDeployer;

    // for tracking [INVADER] combat resolution
    // 

    // (temp) internal vars
    FortVisualScript visualScript;

    // (temp) fraction / team
    public int teamID = 0;
    public Color teamColorLight;
    public Color teamColorShade;

    // debug public vars
    public float fortRadius = 1.0f;
    public float troopGap = 0.025f;

    void Start()
    {
        // check and associate components
        unitContainer = GetComponent<UnitContainerScript>();

        // (temp)
        visualScript = GetComponent<FortVisualScript>();
    }

    void Update()
    {
        
    }

    // custom methods
    
}
