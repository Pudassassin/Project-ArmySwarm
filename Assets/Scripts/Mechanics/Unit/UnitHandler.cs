using UnityEngine;

public class UnitHandler : MonoBehaviour
{
    // Focus on init and setting up [UNIT]'s properties
    // >>> potential script to handle object pooling and re-init / reset

    public UnitStatsBasicSO unitData;

    // (wip temp) team data
    public int ownerID = -2;
    public Color teamColorLight = Color.white;
    public Color teamColorMain  = Color.white;
    public Color teamColorShade = Color.white;

    public GameObject rallyTarget;

    // (temp solution) visual
    public SpriteRenderer sprite;

    public void Setup()
    {
        OwnershipScript ownership       = gameObject.GetComponent<OwnershipScript>();
        HealthHandler healthHandler     = gameObject.GetComponent<HealthHandler>();
        MeleeCombatScript meleeCombat   = gameObject.GetComponent<MeleeCombatScript>();
        UnitMovementScript movement     = gameObject.GetComponent<UnitMovementScript>();
        CrowdPhysicScript crowdPhysic   = gameObject.GetComponent<CrowdPhysicScript>();

        // Setup components
        // > Ownership
        ownership.ownerID       = ownerID;
        ownership.colorLight    = teamColorLight;
        ownership.colorMain     = teamColorMain;
        ownership.colorShade    = teamColorShade;
        ownership.Setup();

        // > Health
        healthHandler.maxHP = unitData.maxHP;
        healthHandler.Setup();

        // > Melee Combat
        meleeCombat.damage              = unitData.meleePower;
        meleeCombat.knockbackForce      = unitData.knockbackForce;
        meleeCombat.knockbackDuration   = unitData.knockbackDuration;
        meleeCombat.graceTime           = unitData.graceTime;

        // > Movement
        movement.targetObject   = rallyTarget;
        movement.moveSpeed      = unitData.moveSpeed;
        movement.stopDistance   = unitData.stopDistance;
        movement.stopbyTime     = unitData.stopbyTime;

        // > Crowd Physics
        crowdPhysic.weight              = unitData.crowdWeight;
        crowdPhysic.repelForce          = unitData.crowdRepelForce;
        crowdPhysic.radius              = unitData.crowdRepelRadius;
        crowdPhysic.distanceMultiplier  = unitData.crowdDistanceMultiplier;

        // (temp solution) setup sprite
        sprite.color = teamColorLight;

        meleeCombat.enabled = true;
        movement.enabled = true;
        crowdPhysic.enabled = true;
    }
}
