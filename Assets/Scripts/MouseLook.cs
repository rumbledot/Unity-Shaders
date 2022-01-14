using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float LookSpeed = 180f;
    private float MouseX = 0f;
    private float MouseY = 0f;

    private float XRotation = 0;

    public Transform PlayerBody;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        transform.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        this.MouseX = Input.GetAxis("Mouse X") * LookSpeed * Time.deltaTime;
        this.MouseY = Input.GetAxis("Mouse Y") * LookSpeed * Time.deltaTime;

        this.XRotation -= MouseY;
        this.XRotation = Mathf.Clamp(XRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(XRotation, 0f, 0f);

        this.PlayerBody.Rotate(Vector3.up * MouseX);
    }
}
