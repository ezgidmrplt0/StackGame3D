using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OyuncuVeKamera : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    public float moveSpeed = 15f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    private Rigidbody rb;
    private float verticalVelocity;

    private float speedMultiplier = 1f; // Hız çarpanı (default 1)

    [Header("Kamera Ayarları")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(10f, 10f, -10f);
    public float cameraFollowSpeed = 5f;

    [Header("Mobil Kontrol")]
    public bool useMobileInput = false;
    public Joystick joystick; // ✔ Burada artık Floating/Dynamic/Variable/Fixed hepsi olur

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
        Vector3 move = Vector3.zero;

        if (useMobileInput && joystick != null)
        {
            // Mobil joystick input
            move = GetMobileMove();
        }
        else
        {
            // PC input
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            move = (camRight * moveX + camForward * moveZ).normalized;
        }

        // Yerçekimi
        if (!IsGrounded())
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        // Hareket
        Vector3 finalVelocity = move * moveSpeed * speedMultiplier;
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
            cameraTransform.LookAt(transform.position);
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.2f);
    }

    Vector3 GetMobileMove()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Joystick inputunu kullan
        Vector3 moveDir = (camRight * joystick.Horizontal + camForward * joystick.Vertical).normalized;
        return moveDir;
    }

    // 🚀 Hız değiştirici (DOTween ile yumuşak geçiş)
    public void SetSpeedMultiplier(float targetValue)
    {
        DOTween.To(() => speedMultiplier, x => speedMultiplier = x, targetValue, 0.5f);
    }
}
