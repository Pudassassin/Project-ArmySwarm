using UnityEngine;

[CreateAssetMenu(fileName = "UnitStatsBasicSO", menuName = "Scriptable Objects/Basic Unit Stats SO")]
public class UnitStatsBasicSO : ScriptableObject
{
    // Basic data
    public string troopName;

    // Movement Stats (prototype)
    [Range(0f, 100f)]
    public float moveSpeed = 1.0f;
    [Range(0f, 100f)]
    public float stopDistance = 1.5f;
    [Range(0f, 100f)]
    public float stopbyTime = 1.5f;

    // Combat Stats
    [Range(1, 100)]
    public int maxHP = 1;
    [Range(0, 100)]
    public int meleePower = 0;
    [Range(0, 100)]
    public int armor = 0;
    // deployment order

    // Garrison Stats (wip)
    public int garrisonPriority = 0;

    // Melee knockback & grace timing
    [Range(0.0f, 10.0f)]
    public float knockbackForce = 0.02f;
    [Range(0.0f, 10.0f)]
    public float knockbackDuration = 0.05f;
    [Range(0.0f, 10.0f)]
    public float graceTime = 0.05f;

    // Crowd physic data
    [Range(0.01f, 100f)]
    public float crowdWeight = 1.0f;
    [Range(0.01f, 10.0f)]
    public float crowdRepelForce = 1.0f;
    [Range(0.01f, 10.0f)]
    public float crowdRepelRadius = 0.1f;
    [Range(0f, 10.0f)]
    public float crowdDistanceMultiplier = 2.5f;
}
