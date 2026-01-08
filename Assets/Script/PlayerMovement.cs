using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{

    // =================MOVEMENT==================//

    [Header("Movement")]
    [SerializeField] private float _MovementSpeed = 6f;
    [SerializeField] private float _Acceleration = 12f;

    [Header("Jumping")]
    [SerializeField] private float _fJumpForce = 5f;
    [SerializeField] private float _GroundCheckDistance = 0.2f;

    [Header("References")]
    [SerializeField] private Transform _GroundCheck;
    [SerializeField] private LayerMask _GroundLayer;
    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody rb;

    [Header("State/Event")]
    [SerializeField] bool _lastCaughtState;
    [SerializeField] bool isCaught;
    public UnityEvent<bool> OnCaughtStateChanged;

    //Input System
    private PlayerInput _PlayerInput;
    private InputAction _MoveAction;
    private InputAction _JumpAction;

    private Vector3 _MoveDirection;
    [SerializeField] private bool _isGrounded;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _PlayerInput = GetComponent<PlayerInput>();
        _MoveAction = _PlayerInput.actions["Move"];
        _JumpAction = _PlayerInput.actions["Jump"];
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        CaughtState();
        UpdateAnimations();
        //RotateCharacter();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    private void UpdateAnimations()
    {
        bool isRunning = _isGrounded && _MoveDirection.sqrMagnitude > 0.01f;



        // Jump start (only once)
        if (_JumpAction.triggered && _isGrounded)
        {
            _animator.SetBool("IsJumping", true);
        }
        // When grounded again
        if (_isGrounded)
        {
            _animator.SetBool("IsJumping", false);
        }
        if (!_isGrounded)
        {
            _animator.SetBool("IsJumping", true);
        }

        // Movement
        _animator.SetBool("IsMoving", isRunning);
        _animator.SetBool("IsIdle", _isGrounded && !isRunning);
    }

    private void CaughtState()
    {
        if (isCaught != _lastCaughtState)
        {
            OnCaughtStateChanged?.Invoke(isCaught);
            _lastCaughtState = isCaught;
        }
    }

    public void SetCaught(bool caught)
    {
        isCaught = caught;
    }

    private void ReadInput()
    {
        Vector2 input = _MoveAction.ReadValue<Vector2>();
        _MoveDirection = new Vector3(input.x, 0f, input.y).normalized;
    }

    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(_GroundCheck.position, Vector3.down, _GroundCheckDistance, _GroundLayer);

        if (_isGrounded && _JumpAction.triggered)
        {
            Jump();
        }
    }

    private void MoveCharacter()
    {
        if (_isGrounded)
        {
            Vector3 desiredVelocity = transform.TransformDirection(_MoveDirection) * _MovementSpeed;

            Vector3 velocityChange = desiredVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            rb.AddForce(velocityChange * rb.mass * _Acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    //private void RotateCharacter()
    //{
    //    if (_MoveDirection.sqrMagnitude < 0.01f)
    //        return;

    //    // Match old script rotation exactly
    //    Quaternion targetRotation = Quaternion.LookRotation(_MoveDirection, Vector3.up);

    //    float rotationSpeed = 720f;

    //    transform.rotation = Quaternion.RotateTowards(
    //        transform.rotation,
    //        targetRotation,
    //        rotationSpeed * Time.deltaTime
    //    );
    //}

    private void Jump()
    {
        rb.AddForce(Vector3.up * _fJumpForce, ForceMode.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (_GroundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_GroundCheck.position, _GroundCheck.position + Vector3.down * _GroundCheckDistance);
    }
}