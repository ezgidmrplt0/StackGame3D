// RespawnManager.cs (GÜNCELLENMÝÞ)
using UnityEngine;
using System.Collections;
using DG.Tweening; // DOTween'i burada kullanmalýyýz!

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;
    void Awake() { Instance = this; }

    public void StartRespawn(GameObject targetObject, float delay)
    {
        StartCoroutine(RespawnCoroutine(targetObject, delay));
    }

    private IEnumerator RespawnCoroutine(GameObject targetObject, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetObject != null)
        {
            // 1. Obje aktif ediliyor
            targetObject.SetActive(true);

            // 2. YENÝ: Sallanma Animasyonunu Baþlat (Geri Gelme Efekti)
            // HamKahveToplama.cs'teki statik deðerleri kullanýyoruz.
            targetObject.transform.DOShakeRotation(
                duration: HamKahveToplama.StaticShakeDuration,
                strength: HamKahveToplama.StaticShakeStrength,
                vibrato: HamKahveToplama.StaticShakeVibrato,
                randomness: HamKahveToplama.StaticShakeRandomness,
                fadeOut: true
            );
        }
    }
}