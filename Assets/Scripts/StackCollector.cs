using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StackCollector : MonoBehaviour
{
    [Header("Stack Ayarları")]
    public GameObject cubePrefab;
    public Transform stackRoot;
    public float cubeHeight = 0.5f; // Küp yüksekliği + boşluk (0.5f)
    public float spawnInterval = 0.2f;
    public float tweenDuration = 0.4f;
    public Ease tweenEase = Ease.OutBack;

    [Header("Küp Boyutu")]
    public Vector3 cubeTargetScale = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("Stack Bırakma Ayarları")]
    public Transform stackAreaTarget;

    private readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;
    private int placedCount = 0;
    private bool isInDropArea = false;

    void Update()
    {
        // Stack pozisyonlarını güncelle
        UpdateStackPositions();
    }

    void UpdateStackPositions()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            Transform cube = stack[i];

            // Küpün hedef pozisyonunu belirle (0.5f mesafe ile)
            // Küpün kendi yüksekliğini de hesaba katarak
            float yOffset = cubeTargetScale.y * 0.5f; // Küpün yarı yüksekliği
            Vector3 targetPos = stackRoot.position + Vector3.up * (cubeHeight * i + yOffset);

            // Doğrudan pozisyon ataması
            cube.position = targetPos;

            // Rotasyonu sabit tut
            cube.rotation = Quaternion.identity;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop == null)
                stackingLoop = StartCoroutine(SpawnLoop());
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = true;
            if (stack.Count > 0)
            {
                StartCoroutine(DropSequence());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop != null)
            {
                StopCoroutine(stackingLoop);
                stackingLoop = null;
            }
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = false;
        }
    }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            AddOneCube();
            yield return wait;
        }
    }

    void AddOneCube()
    {
        // Yeni küpü doğrudan stack'in üstüne yerleştir
        float yOffset = cubeTargetScale.y * 0.5f; // Küpün yarı yüksekliği
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (cubeHeight * stack.Count + yOffset);

        GameObject newCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

        // Küpün rigidbody'sini devre dışı bırak
        Rigidbody cubeRb = newCube.GetComponent<Rigidbody>();
        if (cubeRb != null)
        {
            cubeRb.isKinematic = true;
            cubeRb.useGravity = false;
            cubeRb.drag = 10f; // Daha yüksek drag değeri
        }

        // Collider'ı küçült (iç içe geçmeyi önlemek için)
        Collider cubeCollider = newCube.GetComponent<Collider>();
        if (cubeCollider != null && cubeCollider is BoxCollider)
        {
            BoxCollider boxCollider = cubeCollider as BoxCollider;
            boxCollider.size = cubeTargetScale * 0.9f; // Collider'ı %90 küçült
        }

        // Scale animasyonu
        newCube.transform.localScale = Vector3.zero;
        newCube.transform.DOScale(cubeTargetScale, tweenDuration).SetEase(tweenEase);

        // Stack'e ekle
        stack.Add(newCube.transform);
    }

    IEnumerator DropSequence()
    {
        float dropSpacing = 1.3f;

        while (stack.Count > 0 && isInDropArea)
        {
            Transform cube = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            // Küpün rigidbody'sini tekrar etkinleştir
            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null)
            {
                cubeRb.isKinematic = false;
                cubeRb.useGravity = true;
                cubeRb.drag = 1f; // Normal drag değeri
            }

            // Collider'ı orijinal boyutuna getir
            Collider cubeCollider = cube.GetComponent<Collider>();
            if (cubeCollider != null && cubeCollider is BoxCollider)
            {
                BoxCollider boxCollider = cubeCollider as BoxCollider;
                boxCollider.size = Vector3.one; // Orijinal boyut
            }

            cube.SetParent(null);

            int targetIndex = placedCount;
            Vector3 targetPos = stackAreaTarget.position + new Vector3(0f, dropSpacing * targetIndex, 0f);

            // Küpü hedefe doğru hareket ettir
            cube.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);

            placedCount++;
            yield return new WaitForSeconds(0.1f); // Küpler arası bekleme süresi
        }
    }

    public int Count => stack.Count;
}