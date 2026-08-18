using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour
{
    public GameObject pointerObject;

    public Vector3 worldPointerPos;

    InputAction mousePosAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mousePosAction = InputSystem.actions.FindAction("Point");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = mousePosAction.ReadValue<Vector2>();
        worldPointerPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPointerPos.Scale(new Vector3(1, 1, 0));
        pointerObject.transform.position = worldPointerPos;
    }
}
