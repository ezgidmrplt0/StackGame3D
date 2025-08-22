using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyuncuHareket : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    public float moveSpeed = 5f;
    private Rigidbody rb;

    [Header("Dönüş Ayarları")]
    public float rotationSpeed = 10f; // ne kadar hızlı dönsün

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Oyuncu hareketleri - Kameraya göre relatif hale getirdik
        float moveX = Input.GetAxis("Horizontal");  // A/D için sağ/sol
        float moveZ = Input.GetAxis("Vertical");    // W/S için ileri/geri

        // Kameranın forward ve right vektörlerini al (y=0 için yatay tut)
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Hareket vektörü: kameraya göre hesapla
        Vector3 move = (camRight * moveX + camForward * moveZ) * moveSpeed;

        // Velocity'i uygula, y bileşenini koru
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // Karakterin baktığı yönü hareket vektörüne döndür
        Vector3 lookDir = new Vector3(move.x, 0f, move.z);
        if (lookDir.sqrMagnitude > 0.01f) // hareket ediyorsa
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
