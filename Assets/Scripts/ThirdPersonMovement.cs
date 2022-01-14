using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [SerializeField] private Transform Camera;

    [SerializeField] private CharacterController Controller;
    [SerializeField] private float Speed = 6f;

    private float HorizontalMovement, VerticalMovement;
    private Vector3 Direction;
    private float LookAngle, LookAngleSmooth, LookSmooth = 0.1f, LookSmoothSpeed;

    // Update is called once per frame
    void Update()
    {
        HorizontalMovement = Input.GetAxisRaw("Horizontal");
        VerticalMovement = Input.GetAxisRaw("Vertical");

        Direction = new Vector3(HorizontalMovement, 0f, VerticalMovement).normalized;

        if (Direction.magnitude >= 0.1f) 
        {
            LookAngle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + Camera.eulerAngles.y;

            LookAngleSmooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, LookAngle, ref LookSmoothSpeed, LookSmooth);

            transform.rotation = Quaternion.Euler(0, LookAngleSmooth, 0);

            Direction = Quaternion.Euler(0, LookAngleSmooth, 0) * Vector3.forward;

            Controller.Move(Direction.normalized * Speed * Time.deltaTime);
        }
    }
}
