using UnityEngine;

public class UnitMovementScript : MonoBehaviour
{
    // temp
    public GameObject targetObject;
    public float moveSpeed = 1f;
    public float stopDistance = 2.0f;
    public float stopbyTime = 3.0f;

    float distanceToTarget;
    Vector3 targetVector, moveDirection, moveStep;

    public bool canMove = true;

    bool targetReached = false;
    float stayTimer = 0.0f;

    [HideInInspector]
    public Vector3 previousPosition = Vector3.zero;

    void OnEnable()
    {
        
    }

    void Update()
    {
        if (targetObject == null)
        {
            this.enabled = false;
            return;
        }
        if (!canMove) return;

        previousPosition = transform.position;

        targetVector = (targetObject.transform.position - transform.position);
        moveDirection = targetVector.normalized;
        distanceToTarget = targetVector.magnitude;

        if (targetReached)
        {
            if (distanceToTarget > stopDistance)
            {
                stayTimer = 0.0f;
                targetReached = false;
            }
            else return;
        }
        else if (distanceToTarget <= stopDistance)
        {
            if (stayTimer >= stopbyTime)
            {
                targetReached = true;
            }
            stayTimer += Time.deltaTime;
        }

        moveStep = moveSpeed * Time.deltaTime * moveDirection;
        if (distanceToTarget < moveStep.magnitude)
        {
            transform.position = targetObject.transform.position;
            targetReached = true;
        }
        else
        {
            transform.position += moveStep;
        }
    }
}
