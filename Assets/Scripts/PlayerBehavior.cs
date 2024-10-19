using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerBehavior : MonoBehaviour
{
    // Movement speeds
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float crouchSpeed = 3.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    // Camera settings
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    // Crouch settings
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public float crouchTransitionSpeed = 5.0f;

    // Player states
    public enum PlayerState { Standing, Crouching, Jumping }
    public PlayerState CurrentState { get; private set; } = PlayerState.Standing;

    // Private variables
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool isRunning = false;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterController.height = standingHeight;
        characterController.center = new Vector3(0, standingHeight / 2f, 0);

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleStateChanges();
        Debug.Log(IsCrouching);
    }

    private void HandleMovement()
    {
        // Calculate movement direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        isRunning = canMove && Input.GetKey(KeyCode.LeftShift) && CurrentState == PlayerState.Standing;

        float curSpeedX = canMove ? GetCurrentSpeed() * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? GetCurrentSpeed() * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (canMove && characterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump") && CurrentState != PlayerState.Crouching)
            {
                moveDirection.y = jumpSpeed;
                CurrentState = PlayerState.Jumping;
            }
            else
            {
                moveDirection.y = -gravity * Time.deltaTime;
            }
        }
        else
        {
            moveDirection.y = movementDirectionY - gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void HandleStateChanges()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouch();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }

        // Smoothly adjust character controller height
        float targetHeight = (CurrentState == PlayerState.Crouching) ? crouchingHeight : standingHeight;
        if (Mathf.Abs(characterController.height - targetHeight) > 0.01f)
        {
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            characterController.center = new Vector3(0, characterController.height / 2f, 0);
        }

        // Reset state after jumping
        if (characterController.isGrounded && CurrentState == PlayerState.Jumping && moveDirection.y <= 0)
        {
            CurrentState = Input.GetKey(KeyCode.LeftControl) ? PlayerState.Crouching : PlayerState.Standing;
        }
    }

    private void StartCrouch()
    {
        CurrentState = PlayerState.Crouching;
    }

    private void StopCrouch()
    {
        // Check for obstacles above before standing up
        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchingHeight))
        {
            CurrentState = PlayerState.Standing;
        }
    }

    private float GetCurrentSpeed()
    {
        switch (CurrentState)
        {
            case PlayerState.Crouching:
                return crouchSpeed;
            case PlayerState.Jumping:
                return walkingSpeed; // Same speed as walking while in the air
            default:
                return isRunning ? runningSpeed : walkingSpeed;
        }
    }

    public bool IsCrouching
    {
        get { return CurrentState == PlayerState.Crouching; }
    }
}
