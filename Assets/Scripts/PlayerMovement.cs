using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float lookSensitivity = 0.12f;
    [SerializeField] private float minPitch = -75f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float lookSmoothing = 12f;

    [Header("Camera Motion")]
    [SerializeField] private float cameraBobAmplitude = 0.035f;
    [SerializeField] private float cameraBobFrequency = 8f;
    [SerializeField] private float cameraReturnSpeed = 10f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float yaw;
    private float pitch;
    private Vector3 cameraStartLocalPos;
    private float bobTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraRoot = cam.transform;
            }
        }

        if (cameraRoot != null)
        {
            cameraStartLocalPos = cameraRoot.localPosition;
        }
    }

    private void Start()
    {
        // В режиме игры захватываем курсор для FPS-управления.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleCameraBob();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = ReadMoveInput();
        bool isRunning = IsRunPressed();

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        if (controller.isGrounded && IsJumpPressed())
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move((move + verticalVelocity) * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 lookDelta = Mouse.current.delta.ReadValue() * lookSensitivity;

        yaw += lookDelta.x;
        pitch -= lookDelta.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraRoot != null)
        {
            Quaternion targetRotation = Quaternion.Euler(pitch, 0f, 0f);
            cameraRoot.localRotation = Quaternion.Slerp(
                cameraRoot.localRotation,
                targetRotation,
                lookSmoothing * Time.deltaTime
            );
        }
    }

    private void HandleCameraBob()
    {
        if (cameraRoot == null)
        {
            return;
        }

        Vector2 moveInput = ReadMoveInput();
        bool moving = moveInput.sqrMagnitude > 0.01f && controller.isGrounded;

        Vector3 targetLocalPos = cameraStartLocalPos;

        if (moving)
        {
            float runFactor = IsRunPressed() ? 1.35f : 1f;
            bobTimer += Time.deltaTime * cameraBobFrequency * runFactor;
            targetLocalPos.y += Mathf.Sin(bobTimer) * cameraBobAmplitude;
        }
        else
        {
            bobTimer = 0f;
        }

        cameraRoot.localPosition = Vector3.Lerp(
            cameraRoot.localPosition,
            targetLocalPos,
            cameraReturnSpeed * Time.deltaTime
        );
    }

    private static Vector2 ReadMoveInput()
    {
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;

        Vector2 move = new Vector2(x, y);
        return move.sqrMagnitude > 1f ? move.normalized : move;
    }

    private static bool IsRunPressed()
    {
        return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
    }

    private static bool IsJumpPressed()
    {
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
    }
}

