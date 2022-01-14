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

    void Update()
    {
        this.GatherInputs();

        this.HandleGrounding();

        this.HandleMovement();

        this.HandleRotation();

        this.HandleWeaponVerticalMovement();

        this.HandleJumping();
    }

    private void FixedUpdate()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Wall"))
        {
            this.PlayerRB.AddForce(this.transform.forward * -10.0f);
        }
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

    private UserInputs Inputs;
    private Vector3 Direction;
    private bool Dahsing = false;

    private void GatherInputs()
    {
        this.Inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        this.Inputs.RawZ = (int)Input.GetAxisRaw("Vertical");
        this.Inputs.X = Input.GetAxis("Horizontal");
        this.Inputs.Z = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.LeftShift)) 
        {
            this.Dahsing = true;
        }
    }

    #endregion Inputs

    [Header("Detection")] [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float GroundOffset = -1.0f, GroundCheckRadius = 0.2f;
    [SerializeField] private float _wallCheckOffset = 0.5f, _wallCheckRadius = 0.38f;
    private bool IsGrounded;
    private bool GroundedCheck;

    private readonly Collider[] _ground = new Collider[1];
    private readonly Collider[] _wall = new Collider[1];

    private void HandleGrounding()
    {
        // Grounder
        this.GroundedCheck = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, GroundOffset), GroundCheckRadius, _ground, _groundMask) <= 0;

        if (!this.IsGrounded && this.GroundedCheck)
        {
            this.IsGrounded = true;
            this.HasJumped = false;
            this.HasDoubleJumped = false;
        }
        else if (this.IsGrounded && !this.GroundedCheck)
        {
            this.IsGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position + new Vector3(0, GroundOffset), GroundCheckRadius);
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

    [Header("Movement")] [SerializeField] private float MaxSpeed = 10.0f;
    [SerializeField] private float MinSpeed = 4.0f;
    [SerializeField] private float Acceleration = 1.0f;
    private float Speed = 1.0f;
    [SerializeField] private float DashSpeed = 20f;
    [SerializeField] private ParticleSystem Dash01FX;
    [SerializeField] private ParticleSystem Dash02FX;

    private Vector3 MoveSideway;
    private Vector3 MoveForward;
    private Vector3 MoveVelocity;

    private void HandleMovement()
    {
        if (!this.IsGrounded) return;

        this.MoveSideway = transform.right * this.Inputs.RawX;
        this.MoveForward = transform.forward * this.Inputs.RawZ;

        this.Direction = new Vector3(this.Inputs.X, 0, this.Inputs.Y);

        if (this.Dahsing)
        {
            this.Dash01FX.Play();
            this.Dash02FX.Play();

            this.Dahsing = false;

            this.Speed = this.DashSpeed;
        }
        else 
        {
            if (this.Direction != Vector3.zero)
            {
                this.Speed += this.Acceleration * Time.deltaTime;
            }
            else
            {
                this.Speed -= this.Acceleration * Time.deltaTime;
            }

            this.Speed = Mathf.Clamp(this.Speed, this.MinSpeed, this.MaxSpeed);
        }

        this.MoveVelocity = (this.MoveSideway + this.MoveForward).normalized * this.Speed;

        if (this.MoveVelocity != Vector3.zero)
        {
            this.PlayerRB.MovePosition(this.PlayerRB.position + this.MoveVelocity * Time.fixedDeltaTime);
        }
    }

    #endregion Walking

    #region Look Rotation

    [SerializeField] private float LookSpeed = 5.0f;
    private Vector3 LookDirection;
    private float HorizontalMouseMovement;

    private void HandleRotation()
    {        
        this.HorizontalMouseMovement = Input.GetAxisRaw("Mouse X");

        this.LookDirection = new Vector3(0, Mathf.Clamp(this.HorizontalMouseMovement, -1.0f, 1.0f), 0) * this.LookSpeed;

        this.PlayerRB.MoveRotation(this.PlayerRB.rotation * Quaternion.Euler(this.LookDirection));
    }

    #endregion Look Rotation

    #region Jumping

    [Header("Jumping")] [SerializeField] private float JumpForce = 15;
    [SerializeField] private float FallForceMultiplier = 7;
    [SerializeField] private float MaxJumpVelocity = 8;
    [SerializeField] private ParticleSystem JumpParticleFX;
    [SerializeField] private TrailRenderer TrailFX;
    [SerializeField] private Transform _jumpLaunchPoof;
    [SerializeField] private float _wallJumpLock = 0.25f;
    [SerializeField] private float _wallJumpMovementLerp = 20;
    [SerializeField] private float _coyoteTime = 0.3f;
    [SerializeField] private bool CanDoubleJump = true;
    private float _timeLeftGrounded = -10;
    private float _timeLastWallJumped;
    private bool HasJumped;
    private bool HasDoubleJumped;
    private bool PlayJumpFX = false;
    private GameObject fx;

    private void HandleJumping()
    {
        if (this.PlayJumpFX) 
        {
            this.JumpParticleFX.Play();
            this.PlayJumpFX = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (!this.IsGrounded)
            {
                this._timeLastWallJumped = Time.time;
            }
            else if (this.IsGrounded 
                || Time.time < this._timeLeftGrounded + this._coyoteTime 
                || this.CanDoubleJump && !this.HasDoubleJumped)
            {
                if (!this.HasJumped
                    || (this.HasJumped
                        && !this.HasDoubleJumped))
                {
                    this.ExecuteJump();
                }
            }
        }

        if (this.PlayerRB.velocity.y < this.MaxJumpVelocity
            || (this.PlayerRB.velocity.y > 0
                && !Input.GetButton("Jump")))
        {
            this.PlayerRB.velocity += this.FallForceMultiplier * Physics.gravity.y * Vector3.up * Time.deltaTime;
        }
    }

    private void ExecuteJump()
    {
        this.PlayJumpFX = true;

        this.PlayerRB.velocity = new Vector2(this.PlayerRB.velocity.x, this.JumpForce);
        this.HasDoubleJumped = this.HasJumped;
        this.HasJumped = true;
        this.IsGrounded = false;
    }

    #endregion Jumping


}
