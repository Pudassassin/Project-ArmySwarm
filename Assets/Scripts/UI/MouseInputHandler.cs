using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour
{
    public GameObject pointerObject;

    public Vector3 worldPointerPos;

    InputAction mousePosAction;
    InputAction mouseClickAction;

    float LMBHeldTime = 0.0f;

    PointerObjectScript pointerScript;

    void Start()
    {
        pointerScript = pointerObject.GetComponent<PointerObjectScript>();

        // mouse pointer pos
        mousePosAction = InputSystem.actions.FindAction("Point");

        // left mouse click
        mouseClickAction = InputSystem.actions.FindAction("Click");
    }

    void Update()
    {
        Vector2 mousePos = mousePosAction.ReadValue<Vector2>();
        worldPointerPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPointerPos.Scale(new Vector3(1, 1, 0));
        pointerObject.transform.position = worldPointerPos;


        if (mouseClickAction.ReadValue<float>() > 0.0f)
        {
            // LMB clicked, and held down?
            if (LMBHeldTime == 0.0f)
            {
                pointerScript.SetStartDragSelect();
            }

            LMBHeldTime += Time.deltaTime;
        }
        else
        {
            if (LMBHeldTime > 0.0f)
            {
                pointerScript.SetStopDragSelect();
            }

            LMBHeldTime = 0;
        }
    }
}
