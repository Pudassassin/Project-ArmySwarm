using UnityEngine;

public class TestScript : MonoBehaviour
{
    public GameObject target;
    public Vector3 vectorToTarget;
    public Vector3 vectorToSide;

    void Start()
    {
        
    }

    void Update()
    {
        vectorToTarget = target.transform.position - transform.position;
        vectorToSide = Vector3.Cross(vectorToTarget, new Vector3(0, 0, 1));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + (vectorToTarget.normalized * 2));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + vectorToSide.normalized);

    }
}

/// notes:
/// in 2D space, vector x +z get it perpend toward right hand side
