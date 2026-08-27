using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public DynamicCrosshair crosshair;
    public CharacterController controller;
    public Joystick joystick;
    public Vector2 keyboardInput;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;
    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 2f;
    [Header("Dash UI")]
    public UnityEngine.UI.Image dashCooldownImage;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    Vector3 velocity;
    bool isGrounded;

    void Update()
    {
        
        // Check if player is on ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownImage != null)
            {
                // 0 = no cooldown, 1 = full cooldown
                dashCooldownImage.fillAmount =
                    dashCooldownTimer / dashCooldown;
            }
        }
        else
        {
            if (dashCooldownImage != null)
                dashCooldownImage.fillAmount = 0f;
        }
        // Movement
        float x = joystick.Horizontal + keyboardInput.x;
        float z = joystick.Vertical + keyboardInput.y;
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        bool moving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;

        if(crosshair != null)
        {
            if(moving)
                crosshair.MoveExpand();
            else
                crosshair.Idle();
        }
        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    public void Dash()
    {
        if (isDashing)
            return;

        if (dashCooldownTimer > 0f)
            return;

        StartCoroutine(DashRoutine());
    }
    IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float x = joystick != null ? joystick.Horizontal : keyboardInput.x;
        float z = joystick != null ? joystick.Vertical : keyboardInput.y;

        Vector3 dashDirection;

        // Use movement direction if player is moving
        Vector3 moveDirection =
            transform.right * x +
            transform.forward * z;

        if (moveDirection.magnitude > 0.1f)
        {
            dashDirection = moveDirection.normalized;
        }
        else
        {
            // No movement = dash forward
            dashDirection = transform.forward;
        }

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float dashSpeed = dashDistance / dashDuration;

            controller.Move(
                dashDirection * dashSpeed * Time.deltaTime
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        isDashing = false;
    }
    public void OnMove(InputValue value)
    {
        keyboardInput = value.Get<Vector2>();
    }
}