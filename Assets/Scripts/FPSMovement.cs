using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMovement : MonoBehaviour
{
    private float X, Z;
    private bool IsGrounded = true;
    private Vector3 Move;
    private Vector3 Velocity;
    public float Speed = 12f;
    private float Gravity = -19.81f;
    public float MaxJumpHeight = 3f;

    public Transform GroundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask GroundLayer;

    public CharacterController Controller;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(this.GroundCheck.position, GroundDistance);
    }

    void Update()
    {
        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundLayer);

        if (IsGrounded 
            && Velocity.y < 2f) 
        {
            Velocity.y = 0f;
        }

        X = Input.GetAxis("Horizontal");
        Z = Input.GetAxis("Vertical");

        Move = transform.right * X + transform.forward * Z;

        Controller.Move(Move * Speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump")
            && IsGrounded) 
        {
            Velocity.y = Mathf.Sqrt(MaxJumpHeight * -2f * Gravity);
        }

        Velocity.y += Gravity * Time.deltaTime;

        Controller.Move(Velocity * Time.deltaTime);
    }
}
