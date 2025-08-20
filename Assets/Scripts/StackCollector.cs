using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini kullanmak için (animasyon ve tween iþlemleri)

public class StackCollector : MonoBehaviour
{
    [Header("Stack Ayarlarý")]
    public GameObject cubePrefab; // Spawn edilecek küp prefabý
    public Transform stackRoot; // Küplerin karakterin arkasýnda birikmeye baþlayacaðý nokta
    public float cubeHeight = 0.3f; // Her küpün yüksekliði, üst üste dizilirken kullanýlýr
    public float spawnInterval = 0.2f; // Küplerin arka arkaya spawnlanma süresi
    public float tweenDuration = 0.4f; // Küplerin hareket animasyonunun süresi
    public Ease tweenEase = Ease.OutBack; // Animasyonun easing türü (yumuþak hareket)

    [Header("Küp Boyutu")]
    public Vector3 cubeTargetScale = new Vector3(0.3f, 0.3f, 0.3f); // Küpler spawnlandýðýnda ulaþacaðý boyut

    [Header("Stack Býrakma Ayarlarý")]
    public Transform stackAreaTarget; // Küplerin býrakýlacaðý hedef alan (3. nokta / StackAlaný1)

    private readonly List<Transform> stack = new List<Transform>(); // Mevcut stackteki küplerin listesi
    private Coroutine stackingLoop; // Küplerin otomatik spawnlanmasýný kontrol eden coroutine

    private int placedCount = 0; // 3. noktaya býrakýlmýþ küp sayýsý, küpler üst üste býrakýlýrken kullanýlýr

    void OnTriggerEnter(Collider other)
    {
        // 1. Nokta: Stack toplamaya baþla
        if (other.CompareTag("StackNoktasi0"))
        {
            // Eðer coroutine çalýþmýyorsa baþlat
            if (stackingLoop == null)
                stackingLoop = StartCoroutine(SpawnLoop());
        }

        // 2. Nokta: Stack býrak
        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            // Eðer elimizde küp varsa, sýrasýyla býrak
            if (stack.Count > 0)
            {
                StartCoroutine(DropSequence()); // Küpleri býrakma animasyonu
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 1. noktadan çýkýnca stack durdur
        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop != null)
            {
                StopCoroutine(stackingLoop); // Coroutine durdurulur
                stackingLoop = null;
            }
        }
    }

    // Küpleri belirli aralýklarla spawnlayan coroutine
    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval); // Her döngü arasý bekleme süresi
        while (true)
        {
            AddOneCube(); // Yeni küp ekle
            yield return wait; // Belirtilen süre bekle
        }
    }

    // Tek bir küp spawnlama ve animasyon iþlemi
    void AddOneCube()
    {
        // Küpün stackRoot'a göre konumu (üst üste dizilecek)
        Vector3 targetLocalPos = new Vector3(0f, cubeHeight * stack.Count, 0f);
        // Spawnlanacaðý baþlangýç pozisyonu (stackRoot'un biraz üstü)
        Vector3 spawnWorldPos = stackRoot.position + Vector3.up * 1.5f;

        GameObject go = Instantiate(cubePrefab, spawnWorldPos, Quaternion.identity); // Küpü oluþtur
        go.transform.SetParent(stackRoot, true); // StackRoot altýna yerleþtir
        go.transform.localScale = Vector3.zero; // Animasyon için baþlangýç boyutu 0

        // DOTween ile hareket ve ölçek animasyonu
        Sequence seq = DOTween.Sequence();
        seq.Join(go.transform.DOLocalMove(targetLocalPos, tweenDuration).SetEase(tweenEase)); // Konuma taþý
        seq.Join(go.transform.DOScale(cubeTargetScale, tweenDuration).SetEase(tweenEase)); // Boyut animasyonu

        stack.Add(go.transform); // Küpü stack listesine ekle
    }

    // Küpleri hedef alana býrakma animasyonu
    IEnumerator DropSequence()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            Transform cube = stack[i];

            // Önceki býrakýlanlarýn üstünden devam et
            int targetIndex = placedCount;
            Vector3 targetPos = stackAreaTarget.position + new Vector3(0f, cubeHeight * targetIndex, 0f);

            cube.SetParent(null); // Küpü sahneden baðýmsýzlaþtýr

            // Küpü zýplatarak veya yumuþak þekilde hedefe götür
            yield return cube.DOJump(targetPos, 0.5f, 1, 0.4f)
                             .SetEase(Ease.OutQuad)
                             .WaitForCompletion();

            placedCount++; // Bir sonraki küp bir öncekinin üstüne gelecek
        }

        // stack temizle
        stack.Clear();
    }

    public int Count => stack.Count; // Stackteki mevcut küp sayýsýný döndür
}
