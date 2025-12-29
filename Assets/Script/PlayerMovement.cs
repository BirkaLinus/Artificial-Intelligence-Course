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

    [Header("State/Event")]
    [SerializeField] bool _lastCaughtState;
    [SerializeField] bool isCaught;
    public UnityEvent<bool> OnCaughtStateChanged;

    private Rigidbody rb;

    // Input System
    private PlayerInput _PlayerInput;
    private InputAction _MoveAction;
    private InputAction _JumpAction;

    private Vector3 _MoveDirection;
    [SerializeField] private bool _isGrounded;

    private void Awake()
    {
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
    }

    private void FixedUpdate()
    {
        MoveCharacter();
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