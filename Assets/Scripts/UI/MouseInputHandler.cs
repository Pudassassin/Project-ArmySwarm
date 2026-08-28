using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour
{
    public GameObject pointerObject;

    public Vector3 worldPointerPos;

    InputAction mousePosAction;
    InputAction mouseAction_LMB, mouseAction_RMB;

    float LMB_HeldTime = 0.0f, RMB_HeldTime = 0.0f;

    PointerObjectScript pointerScript;

    void Start()
    {
        pointerScript = pointerObject.GetComponent<PointerObjectScript>();

        // mouse pointer pos
        mousePosAction = InputSystem.actions.FindAction("Point");

        // left mouse click
        mouseAction_LMB = InputSystem.actions.FindAction("Click");

        // right mouse click
        mouseAction_RMB = InputSystem.actions.FindAction("RightClick");
    }

    void Update()
    {
        Vector2 mousePos = mousePosAction.ReadValue<Vector2>();
        worldPointerPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPointerPos.Scale(new Vector3(1, 1, 0));
        pointerObject.transform.position = worldPointerPos;

        // Mouse LEFT CLICK command handle
        // (for now treat quick click as a short-lived click-n-drag)
        if (mouseAction_LMB.ReadValue<float>() > 0.0f)
        {
            // clicked, and held down?
            if (LMB_HeldTime == 0.0f)
            {
                pointerScript.SetStartDragSelect();
            }

            LMB_HeldTime += Time.deltaTime;
        }
        else
        {
            if (LMB_HeldTime > 0.0f)
            {
                pointerScript.SetStopDragSelect();
            }

            LMB_HeldTime = 0;
        }

        // Mouse RIGHT CLICK command handle
        // >>> manually assign rally target
        if (mouseAction_RMB.ReadValue<float>() > 0.0f)
        {
            // clicked, and held down?
            if (RMB_HeldTime == 0.0f)
            {
                pointerScript.SetIssueRallyOrder();
            }

            RMB_HeldTime += Time.deltaTime;
        }
        else
        {
            if (RMB_HeldTime > 0.0f)
            {
                
            }

            RMB_HeldTime = 0;
        }
    }
}
