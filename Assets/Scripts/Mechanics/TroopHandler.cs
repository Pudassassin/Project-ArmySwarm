using UnityEngine;

public class TroopHandler : MonoBehaviour
{
    // Focus on init and setting up troop game object's properties
    // >>> potential script to handle object pooling and re-init / reset

    public UnitStatsBasicSO troopData;

    // wip team data
    public int teamID = 0;
    public Color teamColorLight = Color.white;
    public Color teamColorShade = Color.white;

    public GameObject marchTarget;

    // debug public var
    public SpriteRenderer sprite;

    public void Setup()
    {
        UnitCombatScript combat = gameObject.GetComponent<UnitCombatScript>();
        UnitMovementScript movement = gameObject.GetComponent<UnitMovementScript>();
        CrowdPhysicScript physic = gameObject.GetComponent<CrowdPhysicScript>();

        // setup combat script
        combat.teamID = teamID;

        combat.maxHP = troopData.maxHP;
        combat.meleePower = troopData.meleePower;

        combat.knockbackForce = troopData.knockbackForce;
        combat.knockbackDuration = troopData.knockbackDuration;
        combat.graceTime = troopData.graceTime;

        // setup movement script
        movement.targetObject = marchTarget;
        movement.moveSpeed = troopData.moveSpeed;
        movement.stopDistance = troopData.stopDistance;
        movement.stopbyTime = troopData.stopbyTime;

        // setup crowd physics
        physic.weight = troopData.crowdWeight;
        physic.repelForce = troopData.crowdRepelForce;
        physic.radius = troopData.crowdRepelRadius;
        physic.distanceMultiplier = troopData.crowdDistanceMultiplier;

        // setup sprite (wip)
        sprite.color = teamColorLight;

        combat.enabled = true;
        movement.enabled = true;
        physic.enabled = true;
    }
}
