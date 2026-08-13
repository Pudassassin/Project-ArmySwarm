using System.Collections.Generic;
using UnityEngine;

public class CrowdPhysicScript : MonoBehaviour
{
    public class KnockbackData
    {
        public GameObject source;
        public Vector3 vector;
        public float weight;
        public float duration;

        public KnockbackData(GameObject source, Vector3 vector, float weight, float duration)
        {
            this.source = source;
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

    // temp
    public float weight = 1;
    public float repelForce = 0.1f;
    public float radius = 0.1f;
    public float distanceMultiplier = 1;
    public bool resistCrowding = false;

    List<GameObject> clippingList = new List<GameObject>();
    List<GameObject> clippingListCal = new List<GameObject>();
    List<Vector3> repelVectors = new List<Vector3>();
    Vector3 repelVectorSum;

    UnitMovementScript moveScript;
    List<KnockbackData> kbList = new List<KnockbackData>();
    List<Vector3> kbVectors = new List<Vector3>();
    Vector3 kbVectorSum;

    void OnEnable()
    {
        moveScript = GetComponent<UnitMovementScript>();
    }

    void Update()
    {
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
        if (resistCrowding) return;

        // copy list
        clippingListCal.Clear();
        for (int i = 0; i < clippingList.Count; i++)
        {
            clippingListCal.Add(clippingList[i]);
        }

        // resolve crowd physics
        repelVectors.Clear();

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

        repelVectorSum = Vector3.zero;
        foreach (var item in repelVectors)
        {
            repelVectorSum += item;
        }

        transform.position += repelVectorSum;

        // resolve knockback
        kbVectors.Clear();
        for (int i = kbList.Count - 1; i >= 0 ; i--)
        {
            kbVectors.Add(kbList[i].weight / weight * kbList[i].vector);
            if (kbList[i].Tick())
            {
                kbList.RemoveAt(i);
            }
        }

        kbVectorSum = Vector3.zero;
        foreach (var item in kbVectors)
        {
            kbVectorSum += item;
        }

        transform.position += kbVectorSum;
    }

    // custom methods
    public bool ApplyKnockback(GameObject source, Vector3 forceVector, float weight, float duration)
    {
        for (int i = 0; i < kbList.Count; i++)
        {
            if (kbList[i].source == source) 
            {
                return false;
            }
        }

        kbList.Add(new KnockbackData(source, forceVector, weight, duration));
        return true;
    }
}
