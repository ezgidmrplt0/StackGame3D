using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyuncuHareket : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    public float moveSpeed = 5f;
    private Rigidbody rb;

    [Header("Dönüş Ayarları")]
    public float rotationSpeed = 10f;

    // Yerçekimi için
    public float gravity = -9.81f;
    private Vector3 velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Rigidbody ayarları
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Hareket girdisi
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Kameranın yön vektörleri
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Hareket vektörü
        Vector3 move = (camRight * moveX + camForward * moveZ).normalized;

        // Yerçekimi
        if (!IsGrounded())
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Hareketi uygula
        Vector3 moveVelocity = move * moveSpeed;
        rb.velocity = new Vector3(moveVelocity.x, velocity.y, moveVelocity.z);

        // Dönüş
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.1f + 0.1f);
    }
}