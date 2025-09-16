using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("Hız Ayarı")]
    public float speed = 20f; // Arabaların hızını ayarla

    [Header("Dönüş Ayarı")]
    public float turnDuration = 1f; // Dönüş süresi (saniye)

    private Vector3 moveDirection = Vector3.left; // Başlangıç yönü
    private Quaternion targetRotation;            // Hedef rotasyon
    private bool isTurning = false;               // Şu anda dönüyor mu?
    private float turnTimer = 0f;

    private void Start()
    {
        // Başlangıç rotasyonu baz al
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        // Belirlenen yönde ilerle
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Eğer dönüş yapıyorsa animasyonlu döndür
        if (isTurning)
        {
            turnTimer += Time.deltaTime;
            float t = turnTimer / turnDuration;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.SmoothStep(0f, 1f, t));

            if (t >= 1f)
            {
                isTurning = false; // Dönüş bitti
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ArabaDestroy"))
        {
            Destroy(gameObject);
        }
        else if (other.CompareTag("ArabaTurn"))
        {
            // %50 ihtimalle dön
            if (Random.value < 0.5f)
            {
                // Yeni yönü ayarla (+Z hareket edecek şekilde)
                moveDirection = Vector3.forward;

                if (gameObject.name.Contains("Bus"))
                {
                    // Otobüs için hedef rotasyonu ayarla
                    targetRotation = Quaternion.Euler(0, 180f, 0);

                    // Otobüsü X ekseninde biraz kaydır (sağa doğru)
                    transform.position += new Vector3(10f, 0f, 0f);
                    // Buradaki 2f değerini sahnene göre ayarlayabilirsin (ör: 1.5f ya da 2.5f)
                }
                else
                {
                    // Normal araçlar için relative dönüş
                    targetRotation = transform.rotation * Quaternion.Euler(0, 90f, 0);
                }

                // Dönüş animasyonu başlasın
                isTurning = true;
                turnTimer = 0f;
            }
        }
    }

}
