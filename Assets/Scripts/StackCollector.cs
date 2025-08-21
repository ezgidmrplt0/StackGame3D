using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini kullanmak için (animasyon ve tween işlemleri)

public class StackCollector : MonoBehaviour
{
    [Header("Stack Ayarları")]
    public GameObject cubePrefab; // Spawn edilecek küp prefabı
    public Transform stackRoot; // Küplerin karakterin arkasında birikmeye başlayacağı nokta
    public float cubeHeight = 0.3f; // Her küpün yüksekliği, üst üste dizilirken kullanılır
    public float spawnInterval = 0.2f; // Küplerin arka arkaya spawnlanma süresi
    public float tweenDuration = 0.4f; // Küplerin hareket animasyonunun süresi
    public Ease tweenEase = Ease.OutBack; // Animasyonun easing türü (yumuşak hareket)

    [Header("Küp Boyutu")]
    public Vector3 cubeTargetScale = new Vector3(0.3f, 0.3f, 0.3f); // Küpler spawnlandığında ulaşacağı boyut

    [Header("Stack Bırakma Ayarları")]
    public Transform stackAreaTarget; // Küplerin bırakılacağı hedef alan (3. nokta / StackAlanı1)

    private readonly List<Transform> stack = new List<Transform>(); // Mevcut stackteki küplerin listesi
    private Coroutine stackingLoop; // Küplerin otomatik spawnlanmasını kontrol eden coroutine

    private int placedCount = 0; // 3. noktaya bırakılmış küp sayısı, küpler üst üste bırakılırken kullanılır

    void OnTriggerEnter(Collider other)
    {
        // 1. Nokta: Stack toplamaya başla
        if (other.CompareTag("StackNoktasi0"))
        {
            // Eğer coroutine çalışmıyorsa başlat
            if (stackingLoop == null)
                stackingLoop = StartCoroutine(SpawnLoop());
        }

        // 2. Nokta: Stack bırak
        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            // Eğer elimizde küp varsa, sırasıyla bırak
            if (stack.Count > 0)
            {
                StartCoroutine(DropSequence()); // Küpleri bırakma animasyonu
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 1. noktadan çıkınca stack durdur
        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop != null)
            {
                StopCoroutine(stackingLoop); // Coroutine durdurulur
                stackingLoop = null;
            }
        }
    }

    // Küpleri belirli aralıklarla spawnlayan coroutine
    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval); // Her döngü arası bekleme süresi
        while (true)
        {
            AddOneCube(); // Yeni küp ekle
            yield return wait; // Belirtilen süre bekle
        }
    }

    // Tek bir küp spawnlama ve animasyon işlemi
    void AddOneCube()
    {
        // Küpün stackRoot'a göre konumu (üst üste dizilecek)
        Vector3 targetLocalPos = new Vector3(0f, cubeHeight * stack.Count, 0f);
        // Spawnlanacağı başlangıç pozisyonu (stackRoot'un biraz üstü)
        Vector3 spawnWorldPos = stackRoot.position + Vector3.up * 1.5f;

        GameObject go = Instantiate(cubePrefab, spawnWorldPos, Quaternion.identity); // Küpü oluştur
        go.transform.SetParent(stackRoot, true); // StackRoot altına yerleştir
        go.transform.localScale = Vector3.zero; // Animasyon için başlangıç boyutu 0

        // DOTween ile hareket ve ölçek animasyonu
        Sequence seq = DOTween.Sequence();
        seq.Join(go.transform.DOLocalMove(targetLocalPos, tweenDuration).SetEase(tweenEase)); // Konuma taşı
        seq.Join(go.transform.DOScale(cubeTargetScale, tweenDuration).SetEase(tweenEase)); // Boyut animasyonu

        stack.Add(go.transform); // Küpü stack listesine ekle
    }

    // Küpleri hedef alana bırakma animasyonu
    // Küpleri hedef alana bırakma animasyonu
    IEnumerator DropSequence()
    {
        float currentY = stackAreaTarget.position.y; // taban pivot

        // Hedef alanda zaten küpler varsa en üstteki pozisyonu al
        if (stackAreaTarget.childCount > 0)
        {
            float maxY = currentY;
            foreach (Transform child in stackAreaTarget)
            {
                float top = child.position.y + child.GetComponent<Renderer>().bounds.size.y / 2f;
                if (top > maxY) maxY = top;
            }
            currentY = maxY; // en üstten başla
        }

        // Küpleri sırayla yerleştir
        for (int i = 0; i < stack.Count; i++)
        {
            Transform cube = stack[i];
            float cubeHeight = cube.GetComponent<Renderer>().bounds.size.y;

            Vector3 targetPos = new Vector3(
                stackAreaTarget.position.x,
                currentY + cubeHeight / 2f,
                stackAreaTarget.position.z
            );

            cube.SetParent(stackAreaTarget);
            cube.position = targetPos; // ANİ olarak yerleştir

            // 🎉 Küçük scale animasyonu (hamburger gibi puf efekti)
            cube.DOScale(cubeTargetScale * 1.2f, 0.15f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    cube.DOScale(cubeTargetScale, 0.15f).SetEase(Ease.InOutSine);
                });

            currentY += cubeHeight; // bir sonraki küpün y pozisyonu
            yield return new WaitForSeconds(0.05f); // küçük gecikme (daha hoş görünür)
        }

        stack.Clear();
        yield break;
    }





}
