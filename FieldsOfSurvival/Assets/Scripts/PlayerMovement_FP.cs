using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement_FP : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    public Transform cameraTransform;
    public float lookSpeed = 0.1f;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 lookInput;
    private float xRotation = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Movement (AZERTY)
        float forward = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
        float strafe = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);

        Vector3 move = transform.right * strafe + transform.forward * forward;
        float speed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        cc.Move(move * speed * Time.deltaTime);

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        RotateCamera();
    }

    void RotateCamera()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * lookSpeed;
        float mouseY = Mouse.current.delta.y.ReadValue() * lookSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
