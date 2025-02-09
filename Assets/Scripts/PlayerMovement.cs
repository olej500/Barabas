using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float horizontalMovement;
    [SerializeField] private float rotationSpeed = 20.0f;
    private Vector3 rotationSpeedV;
    private bool isFacingRight = true;
    private Quaternion targetRotation = Quaternion.identity;
    private bool rotate;

    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float airSpeed = 0.5f;
    [SerializeField] private float acceleration = 1.0f;
    [SerializeField] private float decceleration = 1.0f;
    [SerializeField] private float gravityIncrease = -5.0f;
    float targetSpeed, speedDif, accelRate, movement;

    [SerializeField] private float jumpForce = 10.0f;
    private bool jumped;

    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float coyoteTimeCounter;

    private bool canDash = true;
    private bool dashUp = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 42f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rotationSpeedV = new Vector3(0, rotationSpeed, 0);
    }

    
    void Update()
    {
        if (IsGrounded())
        {
            jumped = false;
            canDash = true;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (isDashing)
        {
            return;
        }

        if (!isFacingRight && rb.linearVelocity.x > 0f || isFacingRight && rb.linearVelocity.x < 0)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        targetSpeed = horizontalMovement * movementSpeed;
        speedDif = targetSpeed - rb.linearVelocity.x;
        accelRate = (Mathf.Abs(targetSpeed) > 0.0f) ? acceleration : decceleration;
        movement = Mathf.Abs(speedDif) * accelRate * Mathf.Sign(speedDif);

        if (IsGrounded())
        {
            rb.AddForce(movement * Vector2.right);
        }
        else
        {
            rb.AddForce(movement * Vector2.right * airSpeed);
        }

        if (rb.linearVelocity.y < 0)
            rb.AddForce(new Vector3(0, -gravityIncrease, 0), ForceMode.Force);

        if (rotate)
            Rotate();
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        horizontalMovement = ctx.ReadValue<Vector2>().x;
    }

    private void Flip()
    {
        if (isFacingRight)
        {
            targetRotation = Quaternion.AngleAxis(180f, transform.up);
        }
        else
        {
            targetRotation = Quaternion.AngleAxis(0f, transform.up);
        }
        isFacingRight = !isFacingRight;
        rotate = true;
    }

    private void Rotate()
    {
        if (Mathf.Abs(transform.eulerAngles.y - targetRotation.eulerAngles.y) <= 1f)
        {
            transform.rotation = targetRotation;
            rotate = false;
        }
        else if(!isFacingRight)
        {
            Quaternion deltaRotation = Quaternion.Euler(rotationSpeedV);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        else
        {
            Quaternion deltaRotation = Quaternion.Euler(-rotationSpeedV);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (coyoteTimeCounter > 0 && ctx.performed && !jumped)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }

        if (ctx.canceled)
        {
            jumped = true;
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canDash && dashUp)
        {
            canDash = false;
            dashUp = false;
            isDashing = true;
            rb.useGravity = false;
            rb.linearVelocity = new Vector2(horizontalMovement * dashingPower, 0f);
            StartCoroutine(DashCooldown());
        }
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashingTime);
        rb.useGravity = true;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        dashUp = true;
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.05f, groundLayer);
    }
}
