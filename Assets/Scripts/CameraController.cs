using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 10f;

    [Header("Rotation")]
    public float mouseSensitivity = 0.1f;
    [SerializeField] private float maxCam = 90f;

    private float pitch = 0f;
    private float yaw = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SyncRotation();
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();

        Debug.DrawRay(transform.position, transform.forward * 3f, Color.cyan);
    }

    private void HandleRotation()
    {
        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -maxCam, maxCam);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);

        if (direction.sqrMagnitude > 1f) 
            direction.Normalize();

        transform.Translate(direction * (moveSpeed * Time.deltaTime), Space.Self);
    }

    public void SyncRotation()
    {
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x > 180 ? angles.x - 360 : angles.x;
        yaw = angles.y;
    }

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
}