using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody PlayerRB;
    [SerializeField] public Transform Right_Hand;
    [SerializeField] public Transform Left_Hand;
    [SerializeField] public int Life = 100;

    private void Awake()
    {
        this.PlayerRB = gameObject.GetComponent<Rigidbody>();

        this.SetWeapon(this.Weapon, this.Right_Hand.position);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GatherInputs();

        this.HandleGrounding();

        this.HandleMovement();

        this.HandleRotation();

        this.HandleWeaponVerticalMovement();

        this.HandleJumping();
    }

    #region Inputs

    private struct UserInputs
    {
        public int RawX;
        public int RawY;
        public int RawZ;

        public float X;
        public float Y;
        public float Z;
    }

    private UserInputs _inputs;
    private Vector3 _dir;

    private void GatherInputs()
    {
        this._inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        this._inputs.RawZ = (int)Input.GetAxisRaw("Vertical");
        this._inputs.X = Input.GetAxis("Horizontal");
        this._inputs.Z = Input.GetAxis("Vertical");
    }

    #endregion Inputs

    [Header("Detection")] [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _grounderOffset = -1.0f, _grounderRadius = 0.2f;
    [SerializeField] private float _wallCheckOffset = 0.5f, _wallCheckRadius = 0.38f;
    private bool IsGrounded;

    private readonly Collider[] _ground = new Collider[1];
    private readonly Collider[] _wall = new Collider[1];

    private void HandleGrounding()
    {
        // Grounder
        var grounded = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, _grounderOffset), _grounderRadius, _ground, _groundMask) <= 0;

        if (!IsGrounded && grounded)
        {
            IsGrounded = true;
            _hasJumped = false;
            _hasDoubleJumped = false;
        }
        else if (IsGrounded && !grounded)
        {
            IsGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position + new Vector3(0, _grounderOffset), _grounderRadius);
    }

    #region Weapon

    [Header("Weapon")] [SerializeField] public GameObject Weapon;
    [SerializeField] private float WeaponRotationSpeed = 5.0f;
    private float WeaponX = 0.0f, ReverseRotation = -1;

    private void SetWeapon(GameObject Weapon, Vector3 HandPosition)
    {
        this.Weapon = Weapon;

        Instantiate(this.Weapon, HandPosition, Quaternion.LookRotation(this.transform.right, this.transform.forward), this.Right_Hand.transform);

        this.WeaponX = this.Right_Hand.transform.localRotation.x;
    }

    private void HandleWeaponVerticalMovement()
    {
        float AngleDirection = Mathf.Clamp(Input.GetAxisRaw("Mouse Y"), -2.0f, 2.0f);

        this.WeaponX += AngleDirection * WeaponRotationSpeed * ReverseRotation;

        Vector3 direction = new Vector3(Mathf.Clamp(this.WeaponX, -60, 60), 0, 0);

        this.Right_Hand.transform.localRotation = Quaternion.Euler(direction);
    }

    #endregion Weapon


    #region Walking

    [Header("Walking")] [SerializeField] private float MaxSpeed = 10.0f;
    [SerializeField] private float MinSpeed = 4.0f;
    [SerializeField] private float Acceleration = 1.0f;
    private float Speed = 1.0f;

    private Vector3 MoveSideway;
    private Vector3 MoveForward;
    private Vector3 MoveVelocity;

    private void HandleMovement()
    {
        if (!this.IsGrounded) return;

        MoveSideway = transform.right * this._inputs.RawX;
        MoveForward = transform.forward * this._inputs.RawZ;

        this._dir = new Vector3(this._inputs.X, 0, this._inputs.Y);
        if (this._dir != Vector3.zero)
        {
            this.Speed += this.Acceleration * Time.deltaTime;
        }
        else
        {
            this.Speed -= this.Acceleration * Time.deltaTime;
        }

        this.Speed = Mathf.Clamp(this.Speed, this.MinSpeed, this.MaxSpeed);

        MoveVelocity = (this.MoveSideway + this.MoveForward).normalized * this.Speed;

        if (MoveVelocity != Vector3.zero)
        {
            this.PlayerRB.MovePosition(this.PlayerRB.position + MoveVelocity * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Wall"))
        {
            Debug.LogWarning($"Player hit wall");
            this.PlayerRB.AddForce(this.transform.forward * -10.0f);
        }
    }

    #endregion Walking

    #region Look Rotation

    [SerializeField] private float LookSpeed = 5.0f;

    private void HandleRotation()
    {        
        float LookDirection = Input.GetAxisRaw("Mouse X");

        Vector3 Direction = new Vector3(0, Mathf.Clamp(LookDirection, -1.0f, 1.0f), 0) * LookSpeed;

        this.PlayerRB.MoveRotation(this.PlayerRB.rotation * Quaternion.Euler(Direction));
    }

    #endregion Look Rotation

    #region Jumping

    [Header("Jumping")] [SerializeField] private float _jumpForce = 15;
    [SerializeField] private float _fallMultiplier = 7;
    [SerializeField] private float _jumpVelocityFalloff = 8;
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private Transform _jumpLaunchPoof;
    [SerializeField] private float _wallJumpLock = 0.25f;
    [SerializeField] private float _wallJumpMovementLerp = 20;
    [SerializeField] private float _coyoteTime = 0.3f;
    [SerializeField] private bool _enableDoubleJump = true;
    private float _timeLeftGrounded = -10;
    private float _timeLastWallJumped;
    private bool _hasJumped;
    private bool _hasDoubleJumped;

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (!IsGrounded)
            {
                _timeLastWallJumped = Time.time;
            }
            else if (IsGrounded 
                || Time.time < _timeLeftGrounded + _coyoteTime 
                || _enableDoubleJump && !_hasDoubleJumped)
            {
                if (!_hasJumped
                    || (_hasJumped
                        && !_hasDoubleJumped))
                {
                    ExecuteJump(new Vector2(this.PlayerRB.velocity.x, _jumpForce), _hasJumped);
                }
            }
        }

        if (this.PlayerRB.velocity.y < _jumpVelocityFalloff
            || (this.PlayerRB.velocity.y > 0
                && !Input.GetButton("Jump")))
        {
            this.PlayerRB.velocity += _fallMultiplier * Physics.gravity.y * Vector3.up * Time.deltaTime;
        }
    }

    private void ExecuteJump(Vector3 dir, bool doubleJump = false)
    {
        var jump_fx = Instantiate(this._jumpParticles, this.transform.position, Quaternion.identity);
        jump_fx.Play();
        Destroy(jump_fx, 0.2f);

        this.PlayerRB.velocity = dir;
        _hasDoubleJumped = doubleJump;
        _hasJumped = true;
        IsGrounded = false;
    }

    #endregion Jumping


}
