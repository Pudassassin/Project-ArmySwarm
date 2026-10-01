using System.Collections.Generic;
using UnityEngine;

public class CrowdPhysicScript : MonoBehaviour
{
    // Lighten and simplify game object's crowding interactions without having to use the entirely of Unity's physics engine
    // > work by "moving this entity away" from other entities
    // > also handling and considering [KNOCKBACK] effects (simpler to be merged here)

    public class KnockbackData
    {
        // direction and magnitude of the force
        public Vector3 vector;

        // weight to consider coeff. of the knockback
        public float weight;

        // duration of knockback
        public float duration;

        // force curve (future)

        public KnockbackData(Vector3 vector, float weight, float duration)
        {
            this.vector = vector;
            this.weight = weight;
            this.duration = duration;
        }

        public bool Tick()
        {
            duration -= Time.deltaTime;
            return duration < 0;
        }
    }


    //====================================
    // Entity's Physic Property
    public float weight = 1;

    // Crowd "elastic" property
    public float repelForce = 0.1f;
    public float radius = 0.1f;
    public float distanceMultiplier = 1;

    // if true: this entity will not move itself away from other entities with crowd physics 
    //          also clear and immune to [KNOCKBACK]
    public bool resistCrowding = false;

    // vars for crowd physics calculation
    List<GameObject> clippingList = new List<GameObject>();
    List<GameObject> clippingListCal = new List<GameObject>();
    List<Vector3> repelVectors = new List<Vector3>();
    Vector3 repelVectorSum;

    // if true: immune to [KNOCKBACK] and clear it out
    public bool resistKnockback = false;

    // vars for knockback effect calculation
    UnitMovementScript moveScript;
    List<KnockbackData> kbList = new List<KnockbackData>();
    List<Vector3> kbVectors = new List<Vector3>();
    Vector3 kbVectorSum;


    //====================================
    // Unity Messages
    void Update()
    {
        // [KNOCKBACK] disable movement (wip)
        if (kbList.Count > 0)
        {
            moveScript.canMove = false;
        }
        else
        {
            moveScript.canMove = true;
        }

        // clean up
        for (int i = clippingList.Count - 1; i >= 0; i--)
        {
            if (clippingList[i] == null)
            {
                clippingList.RemoveAt(i);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D otherCol)
    {
        // add to list the clipping object 
        //Debug.Log("collide enter");

        CrowdPhysicScript crowdScript = otherCol.gameObject.GetComponent<CrowdPhysicScript>();
        if (crowdScript == null)
        {
            //Debug.Log("no script");
            return;
        }

        clippingList.Add(otherCol.gameObject);
    }

    void OnTriggerExit2D(Collider2D otherCol)
    {
        // remove from list the clipping object
        //Debug.Log("collide exit");

        clippingList.Remove(otherCol.gameObject);
    }

    void LateUpdate()
    {
        repelVectors.Clear();
        repelVectorSum = Vector3.zero;

        kbVectors.Clear();
        kbVectorSum = Vector3.zero;

        if (resistCrowding)
        {
            kbList.Clear();
            return;
        }

        // make a copy of list to be resolved
        clippingListCal.Clear();
        for (int i = 0; i < clippingList.Count; i++)
        {
            clippingListCal.Add(clippingList[i]);
        }

        // resolve crowd physics
        // > make this game object moving itself away from other objects
        // > avoid crowd compression and objects stacking on top of each other
        // > (wip) flat scaling based on distance from each other
        foreach (var item in clippingListCal)
        {
            CrowdPhysicScript otherCrowd = item.GetComponent<CrowdPhysicScript>();
            Vector3 repelVector = transform.position - item.transform.position;
            repelVector.Scale(new Vector3(1, 1, 0));

            float clippingMul = 1.0f - (repelVector.magnitude / (radius + otherCrowd.radius));
            clippingMul = Mathf.Clamp(clippingMul, 0.0f, 1.0f);
            float force = otherCrowd.weight / weight * otherCrowd.repelForce * Time.deltaTime;

            repelVectors.Add(force * otherCrowd.distanceMultiplier * clippingMul * repelVector.normalized);
        }

        foreach (var item in repelVectors)
        {
            repelVectorSum += item;
        }

        transform.position += repelVectorSum;

        // resolve [KNOCKBACK]
        if (resistKnockback)
        {
            kbList.Clear();
            return;
        }

        for (int i = kbList.Count - 1; i >= 0 ; i--)
        {
            kbVectors.Add(kbList[i].weight / weight * kbList[i].vector);
            if (kbList[i].Tick())
            {
                kbList.RemoveAt(i);
            }
        }

        foreach (var item in kbVectors)
        {
            kbVectorSum += item;
        }

        transform.position += kbVectorSum;
    }


    //====================================
    // Custom methods
    public void Setup()
    {
        moveScript = GetComponent<UnitMovementScript>();
    }


    //====================================
    // Knockback method
    //====================================
    // Taking in knockback force, raw and without additional effects
    public void TakeKnockback(Vector3 forceVector, float weight, float duration)
    {
        kbList.Add(new KnockbackData(forceVector, weight, duration));
    }
}
