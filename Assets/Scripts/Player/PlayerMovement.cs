using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public DynamicCrosshair crosshair;
    public CharacterController controller;
    public Joystick joystick;

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
        float x = joystick.Horizontal;
        float z = joystick.Vertical;
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

}