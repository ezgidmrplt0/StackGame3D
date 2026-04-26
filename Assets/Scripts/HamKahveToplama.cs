using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

// NOT: Bu script'in Oyuncu/Depocu objesine ba�l� olmas� GEREK�R.
public class HamKahveToplama : MonoBehaviour
{
    // --- STACK AYARLARI ---
    [Header("STACK Y�NET�M�")]
    [Tooltip("Stack'in ba�layaca�� pivot (Oyuncu objesinde bir �ocuk olmal�).")]
    public Transform stackRoot;
    [Tooltip("Stack'e eklenecek kahve �EK�RDE�� prefab�.")]
    public GameObject kahveCekirdegiPrefab;
    public int stackLimit = 10;

    [Header("Stack G�r�n�m Ayarlar�")]
    public Vector3 kahveTargetScale = new Vector3(1f, 1f, 1f);
    public float stackHeight = 0.5f;
    public Ease tweenEase = Ease.OutBack;

    // --- BIRAKMA AYARLARI ---
    [Header("BIRAKMA/SATI� AYARLARI")]
    [Tooltip("Stackteki �r�nlerin yok olma (b�rakma) h�z� (saniye aral���).")]
    public float dropInterval = 0.05f;

    [HideInInspector] public List<Transform> stack = new List<Transform>();

    // --- TOPLAMA AYARLARI (F�rlatma parametreleri kald�r�ld�) ---
    [Header("Yeniden Canlanma (Respawn)")]
    public float respawnDelay = 5f;

    [Header("A�A� SALLANMA AN�MASYONU")]
    public float shakeDuration = 0.2f;
    [Tooltip("Eksen ba��na maksimum d�n�� a��s� (�rn: 5f, 0f, 5f)")]
    public Vector3 shakeStrength = new Vector3(5f, 0f, 5f);
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;

    // Respawn Manager'a iletmek i�in Statik Ayarlar (G�r�nmez Kals�nlar)
    public static float StaticShakeDuration;
    public static Vector3 StaticShakeStrength;
    public static int StaticShakeVibrato;
    public static float StaticShakeRandomness;

    // --- AKI� KONTROL DE���KENLER� ---
    private Coroutine dropLoop;

    private void Start()
    {
        if (stackRoot == null || kahveCekirdegiPrefab == null)
        {
            Debug.LogError(gameObject.name + " �zerindeki HamKahveToplama: Stack Root veya Prefab atanmam��!");
        }
        if (RespawnManager.Instance == null)
        {
            Debug.LogError("RespawnManager sahnede bulunam�yor! Yeniden canlanma �al��mayacak.");
        }
        StaticShakeDuration = shakeDuration;
        StaticShakeStrength = shakeStrength;
        StaticShakeVibrato = shakeVibrato;
        StaticShakeRandomness = shakeRandomness;
    }


    /// <summary>
    /// Kahve A�ac�n� deaktif eder, respawn i�lemini ba�lat�r ve stack'e direkt ekler.
    /// </summary>
    /// <param name="coffeeTree">Deaktif edilecek Kahve A�ac� objesi.</param>
    private void CollectTreeAndStartRespawn(GameObject coffeeTree)
    {
        // Aynı ağaç üzerinde önceki bir shake tween'i varsa öldür
        coffeeTree.transform.DOKill();

        coffeeTree.transform.DOShakeRotation(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: shakeVibrato,
            randomness: shakeRandomness,
            fadeOut: true
        ).OnComplete(() =>
        {
            // 2. Sallanma bitince: A�ac� toplanm�� kabul et, deaktif et
            coffeeTree.SetActive(false);

            // 3. Respawn Manager'a bu objeyi beklemeye almas� i�in g�rev ver
            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.StartRespawn(coffeeTree, respawnDelay);
            }
            else
            {
                Debug.LogWarning("RespawnManager.Instance NULL! A�a� geri gelmeyecek.");
            }

            // 4. Stack'e hemen bir �r�n ekle
            AddOneKahveToStack();
            Debug.Log("Kahve an�nda Stack'e eklendi.");
        });
    }

    // NOT: LaunchCoffeeToStack metodu tamamen KALDIRILDI.

    // --- STACK Y�NET�M METODU (Ayn� Kal�r) ---
    public void AddOneKahveToStack()
    {
        if (stack.Count >= stackLimit) return;

        float yOffset = kahveTargetScale.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (stackHeight * stack.Count + yOffset);

        GameObject newCube = Instantiate(kahveCekirdegiPrefab, spawnPosition, Quaternion.identity);
        newCube.transform.SetParent(stackRoot);

        // Y rotasyon d�zeltmesi
        newCube.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        // Tüm alt Rigidbody'leri ve Collider'ları devre dışı bırak.
        // PW_espresso_cup gibi prefab'ların alt objelerinde non-kinematic Rigidbody
        // ve non-convex MeshCollider olması sonsuz fizik kuvvetine ve yanlış
        // trigger tetiklenmelerine yol açıyor.
        foreach (var rb in newCube.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        foreach (var col in newCube.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        newCube.transform.localScale = Vector3.zero;
        newCube.transform.DOScale(kahveTargetScale, 0.4f).SetEase(tweenEase);

        stack.Add(newCube.transform);
    }

    // --- BIRAKMA VE TOPLAMA TET�KLEY�C�S� (Ayn� Kal�r) ---
    private void OnTriggerEnter(Collider other)
    {
        // 1. Kahve B�rakma Noktas� Kontrol�
        if (other.CompareTag("KahveBirakmaNoktasi"))
        {
            if (dropLoop == null && stack.Count > 0)
            {
                dropLoop = StartCoroutine(DropSequence());
            }
        }

        // 2. Kahve A�ac� Toplama Kontrol�
        if (other.CompareTag("KahveToplamaNoktasi"))
        {
            if (stack.Count >= stackLimit)
            {
                Debug.Log("Stack Dolu! Kahve toplanam�yor.");
                return;
            }

            CollectTreeAndStartRespawn(other.gameObject);
            CoffeeStackCollector.Instance.hamKahve++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("KahveBirakmaNoktasi"))
        {
            if (dropLoop != null)
            {
                StopCoroutine(dropLoop);
                dropLoop = null;
                Debug.Log("B�rakma noktas�ndan ��k�ld�. B�rakma durduruldu.");
            }
        }
    }

    // --- DROP SEQUENCE (Ayn� Kal�r) ---
    // HamKahveToplama.cs i�inde DropSequence metodunuz

    IEnumerator DropSequence()
    {
        var wait = new WaitForSeconds(dropInterval);
        Debug.Log("B�rakma/Sat�� i�lemi ba�lad�.");

        while (stack.Count > 0)
        {
            Transform cubeToDrop = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            cubeToDrop.DOScale(Vector3.zero, dropInterval * 0.8f)
                      .SetEase(Ease.InQuad)
                      .OnComplete(() =>
                      {
                          Destroy(cubeToDrop.gameObject);
                      });

            if (CoffeeStackCollector.Instance != null)
            {
                CoffeeStackCollector.Instance.kahveStogu++;
            }
            CoffeeStackCollector.Instance.hamKahve--;

            yield return wait;
        }

        dropLoop = null;
        Debug.Log("Stack bo�ald�. B�rakma i�lemi tamamland�.");
    }
}