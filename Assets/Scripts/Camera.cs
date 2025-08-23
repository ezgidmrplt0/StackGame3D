using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float dampTime;        // Kameranýn yumuþak takip süresi
    public Vector3 offset;               // Hedeften uzaklýk
    private Vector3 velocity = Vector3.zero;
    public Transform target;             // Takip edilecek obje

    private void FixedUpdate()
    {
        if (target == null)
            return;

        // Hedef pozisyon + offset
        Vector3 targetPosition = target.position + offset;

        // Kamerayý SmoothDamp ile yumuþak hareket ettir
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dampTime);
    }
}

