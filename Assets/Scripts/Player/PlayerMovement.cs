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
    public void OnMove(InputValue value)
    {
        keyboardInput = value.Get<Vector2>();
    }
}