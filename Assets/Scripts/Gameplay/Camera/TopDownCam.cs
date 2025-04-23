using UnityEngine;

public class TopDownCam : MonoBehaviour
{
    public float CameraScrollSpeed = 10f;
    public float CameraZoomSpeed = 10f;
    public GameObject Camera;

    [Header("Edge Scrolling")]
    public float edgeThickness = 10f; // Pixels from screen edge to trigger movement

    [Header("Movement Bounds")]
    public Vector2 xLimits = new Vector2(-80f, 80f);
    public Vector2 zLimits = new Vector2(-150f, -45f);

    private void MoveCamera()
    {
        Vector3 moveDir = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x >= Screen.width - edgeThickness)
            moveDir += Vector3.right;
        else if (mousePos.x <= edgeThickness)
            moveDir += Vector3.left;

        if (mousePos.y >= Screen.height - edgeThickness)
            moveDir += Vector3.forward;
        else if (mousePos.y <= edgeThickness)
            moveDir += Vector3.back;

        Vector3 newPosition = transform.position + moveDir.normalized * CameraScrollSpeed * Time.deltaTime;

        // Clamp within bounds
        newPosition.x = Mathf.Clamp(newPosition.x, xLimits.x, xLimits.y);
        newPosition.z = Mathf.Clamp(newPosition.z, zLimits.x, zLimits.y);
        transform.position = newPosition;

        // Zoom in/out
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Camera.transform.Translate(Vector3.forward * CameraZoomSpeed * Time.deltaTime * scrollInput);
    }

    private void Update()
    {
        MoveCamera();
    }
}
