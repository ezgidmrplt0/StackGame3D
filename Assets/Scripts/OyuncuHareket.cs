using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyuncuVeKamera : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    private Rigidbody rb;
    private float verticalVelocity;

    [Header("Kamera Ayarları")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(10f, 10f, -10f);
    public float cameraFollowSpeed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Kamera ortographic
        if (cameraTransform != null)
        {
            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 7f;
            }
        }
    }

    void Update()
    {
        // Karakter hareketi
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = (camRight * moveX + camForward * moveZ).normalized;

        if (!IsGrounded())
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        Vector3 finalVelocity = move * moveSpeed;
        finalVelocity.y = verticalVelocity;
        rb.velocity = finalVelocity;

        // Karakter dönüşü
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Kamera takibi
        if (cameraTransform != null)
        {
            Vector3 targetPos = new Vector3(
                transform.position.x + cameraOffset.x,
                cameraOffset.y,
                transform.position.z + cameraOffset.z
            );
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, cameraFollowSpeed * Time.deltaTime);
            cameraTransform.LookAt(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.2f);
    }
}
