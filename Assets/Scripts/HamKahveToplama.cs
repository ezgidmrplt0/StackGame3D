using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

// NOT: Bu script'in Oyuncu/Depocu objesine baðlý olmasý GEREKÝR.
public class HamKahveToplama : MonoBehaviour
{
    // --- STACK AYARLARI ---
    [Header("STACK YÖNETÝMÝ")]
    [Tooltip("Stack'in baþlayacaðý pivot (Oyuncu objesinde bir çocuk olmalý).")]
    public Transform stackRoot;
    [Tooltip("Stack'e eklenecek kahve ÇEKÝRDEÐÝ prefabý.")]
    public GameObject kahveCekirdegiPrefab;
    public int stackLimit = 10;

    [Header("Stack Görünüm Ayarlarý")]
    public Vector3 kahveTargetScale = new Vector3(1f, 1f, 1f);
    public float stackHeight = 0.5f;
    public Ease tweenEase = Ease.OutBack;

    // --- BIRAKMA AYARLARI ---
    [Header("BIRAKMA/SATIÞ AYARLARI")]
    [Tooltip("Stackteki ürünlerin yok olma (býrakma) hýzý (saniye aralýðý).")]
    public float dropInterval = 0.05f;

    [HideInInspector] public List<Transform> stack = new List<Transform>();

    // --- TOPLAMA AYARLARI (Fýrlatma parametreleri kaldýrýldý) ---
    [Header("Yeniden Canlanma (Respawn)")]
    public float respawnDelay = 5f;

    [Header("AÐAÇ SALLANMA ANÝMASYONU")]
    public float shakeDuration = 0.2f;
    [Tooltip("Eksen baþýna maksimum dönüþ açýsý (Örn: 5f, 0f, 5f)")]
    public Vector3 shakeStrength = new Vector3(5f, 0f, 5f);
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;

    // Respawn Manager'a iletmek için Statik Ayarlar (Görünmez Kalsýnlar)
    public static float StaticShakeDuration;
    public static Vector3 StaticShakeStrength;
    public static int StaticShakeVibrato;
    public static float StaticShakeRandomness;

    // --- AKIÞ KONTROL DEÐÝÞKENLERÝ ---
    private Coroutine dropLoop;

    private void Start()
    {
        if (stackRoot == null || kahveCekirdegiPrefab == null)
        {
            Debug.LogError(gameObject.name + " üzerindeki HamKahveToplama: Stack Root veya Prefab atanmamýþ!");
        }
        if (RespawnManager.Instance == null)
        {
            Debug.LogError("RespawnManager sahnede bulunamýyor! Yeniden canlanma çalýþmayacak.");
        }
        StaticShakeDuration = shakeDuration;
        StaticShakeStrength = shakeStrength;
        StaticShakeVibrato = shakeVibrato;
        StaticShakeRandomness = shakeRandomness;
    }


    /// <summary>
    /// Kahve Aðacýný deaktif eder, respawn iþlemini baþlatýr ve stack'e direkt ekler.
    /// </summary>
    /// <param name="coffeeTree">Deaktif edilecek Kahve Aðacý objesi.</param>
    private void CollectTreeAndStartRespawn(GameObject coffeeTree)
    {
        // 1. Aðacý sallama animasyonunu baþlat
        // Animasyon bitince, OnComplete içinde deaktif etme ve stackleme iþlemlerini yapýyoruz.
        coffeeTree.transform.DOShakeRotation(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: shakeVibrato,
            randomness: shakeRandomness,
            fadeOut: true // Sallanma biterken yumuþakça durmasýný saðlar
        ).OnComplete(() =>
        {
            // 2. Sallanma bitince: Aðacý toplanmýþ kabul et, deaktif et
            coffeeTree.SetActive(false);

            // 3. Respawn Manager'a bu objeyi beklemeye almasý için görev ver
            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.StartRespawn(coffeeTree, respawnDelay);
            }
            else
            {
                Debug.LogWarning("RespawnManager.Instance NULL! Aðaç geri gelmeyecek.");
            }

            // 4. Stack'e hemen bir ürün ekle
            AddOneKahveToStack();
            Debug.Log("Kahve anýnda Stack'e eklendi.");
        });
    }

    // NOT: LaunchCoffeeToStack metodu tamamen KALDIRILDI.

    // --- STACK YÖNETÝM METODU (Ayný Kalýr) ---
    public void AddOneKahveToStack()
    {
        if (stack.Count >= stackLimit) return;

        float yOffset = kahveTargetScale.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (stackHeight * stack.Count + yOffset);

        GameObject newCube = Instantiate(kahveCekirdegiPrefab, spawnPosition, Quaternion.identity);
        newCube.transform.SetParent(stackRoot);

        // Y rotasyon düzeltmesi
        newCube.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        Rigidbody cubeRb = newCube.GetComponent<Rigidbody>();
        if (cubeRb != null)
        {
            cubeRb.isKinematic = true;
            cubeRb.useGravity = false;
        }

        newCube.transform.localScale = Vector3.zero;
        newCube.transform.DOScale(kahveTargetScale, 0.4f).SetEase(tweenEase);

        stack.Add(newCube.transform);
    }

    // --- BIRAKMA VE TOPLAMA TETÝKLEYÝCÝSÝ (Ayný Kalýr) ---
    private void OnTriggerEnter(Collider other)
    {
        // 1. Kahve Býrakma Noktasý Kontrolü
        if (other.CompareTag("KahveBirakmaNoktasi"))
        {
            if (dropLoop == null && stack.Count > 0)
            {
                dropLoop = StartCoroutine(DropSequence());
            }
        }

        // 2. Kahve Aðacý Toplama Kontrolü
        if (other.CompareTag("KahveToplamaNoktasi"))
        {
            if (stack.Count >= stackLimit)
            {
                Debug.Log("Stack Dolu! Kahve toplanamýyor.");
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
                Debug.Log("Býrakma noktasýndan çýkýldý. Býrakma durduruldu.");
            }
        }
    }

    // --- DROP SEQUENCE (Ayný Kalýr) ---
    // HamKahveToplama.cs içinde DropSequence metodunuz

    IEnumerator DropSequence()
    {
        var wait = new WaitForSeconds(dropInterval);
        Debug.Log("Býrakma/Satýþ iþlemi baþladý.");

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
        Debug.Log("Stack boþaldý. Býrakma iþlemi tamamlandý.");
    }
}