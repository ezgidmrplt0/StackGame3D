using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("Hýz Ayarý")]
    public float speed = 20f; // Arabalarýn hýzýný ayarla

    private void Update()
    {
        // Sadece X ekseninde ilerlesin
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ArabaDestroy"))
        {
            Destroy(gameObject);
        }
    }
}
