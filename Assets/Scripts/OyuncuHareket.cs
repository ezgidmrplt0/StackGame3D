using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyuncuHareket : MonoBehaviour
{
    public float moveSpeed = 5f;      // Yürüme hýzý
    public float jumpForce = 5f;      // Zýplama kuvveti
    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Klavye giriþlerini al
        float moveX = Input.GetAxis("Horizontal"); // A-D veya Sol-Sað ok
        float moveZ = Input.GetAxis("Vertical");   // W-S veya Yukarý-Aþaðý ok

        // Hareket yönü oluþtur
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;

        // Rigidbody'nin hýzýný ayarla (x,z kontrol bizde, y yerçekimi)
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // Zýplama (space basýlýrsa ve yerdeyse)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // Yere deðip deðmediðini kontrol et
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
