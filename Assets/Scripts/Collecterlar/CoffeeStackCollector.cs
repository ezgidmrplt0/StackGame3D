using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CoffeeStackCollector : MonoBehaviour
{
    public static CoffeeStackCollector Instance;

    // --- Stack Ayarlarý ---

    [Header("Stack Ayarlarý")]
    public GameObject kahvePrefab; // Stacklenecek ürün prefab'i
    public Transform stackRoot; // Oyuncunun üzerindeki stack'in baþlangýç noktasý (Inspector'dan atanacak)
    public float stackHeight = 0.5f;
    public float stackSpawnInterval = 0.2f;
    public int stackLimit = 10;
    public Vector3 kahveTargetScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Ease tweenEase = Ease.OutBack;

    [Header("Stok ve UI")]
    public TextMeshPro stokText;
    public TextMeshPro hamKahveText;
    public int kahveStogu = 100;
    public int hamKahve = 10;
    public GameObject alan;
    public GameObject buton;
    public GameObject butonKahve;

    // Stack Listesi
    public readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;

    // --- Býrakma (Drop) Ayarlarý ---

    [Header("Býrakma (Drop) Ayarlarý")]
    public string dropAreaTag = "KahveBirakmaNoktasi"; // Býrakma alaný için yeni tag
    public List<Transform> dropList = new List<Transform>(); // Býrakýlan ürünlerin yýðýlacaðý liste
    public Transform dropAreaTarget; // Býrakýlan ürünlerin hedef konumu (Inspector'dan atanacak)
    public float dropSpacing = 2f; // Býrakýlan ürünler arasý boþluk

    private Coroutine dropLoop;
    private bool isInDropArea = false; // Býrakma alanýnda mýyýz kontrolü
    private int currentPrice = 100;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        UpdateStackPositions();
        GuncelleUI();
    }

    // --- Stackleme Mantýðý ---

    void UpdateStackPositions()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            Transform cube = stack[i];
            float yOffset = kahveTargetScale.y * 0.5f;
            Vector3 targetPos = stackRoot.position + Vector3.up * (stackHeight * i + yOffset);

            cube.position = targetPos;
            cube.rotation = Quaternion.identity;
        }
    }

    void AddOneKahveToStack()
    {
        if (stack.Count >= stackLimit) return;
        if (kahveStogu <= 0) return;

        kahveStogu--;

        float yOffset = kahveTargetScale.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (stackHeight * stack.Count + yOffset);

        GameObject newCube = Instantiate(kahvePrefab, spawnPosition, Quaternion.identity);
        newCube.transform.SetParent(stackRoot);

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

    IEnumerator StackSpawnLoop()
    {
        var wait = new WaitForSeconds(stackSpawnInterval);

        while (true)
        {
            if (kahveStogu > 0 && stack.Count < stackLimit)
            {
                AddOneKahveToStack();
            }

            yield return wait;
        }
    }

    // --- Býrakma (Drop) Mantýðý ---

    IEnumerator DropSequence()
    {
        // Stack boþ deðilse VE hala býrakma alanýndaysak çalýþmaya devam et
        while (stack.Count > 0 && isInDropArea)
        {
            if (dropAreaTarget == null) yield break;

            // Stack'in en üstündeki ürünü al
            Transform cube = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            cube.SetParent(null); // Parent'i kaldýr

            // Fiziksel simülasyonu aç
            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null)
            {
                cubeRb.isKinematic = false;
                cubeRb.useGravity = true;
                cubeRb.drag = 1f;
            }

            // Hedef pozisyonu hesapla (Drop Listesine göre)
            int targetIndex = dropList.Count;
            Vector3 targetPos = dropAreaTarget.position + Vector3.up * (dropSpacing * targetIndex);

            // Animasyon ve Drop Listesine Ekleme
            cube.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);
            dropList.Add(cube);

            yield return new WaitForSeconds(0.1f); // Býrakma hýzý
        }

        dropLoop = null; // Ýþlem bittiðinde Coroutine'i null'a çek
    }

    // --- Trigger Metotlarý ---

    void OnTriggerEnter(Collider other)
    {
        // Kahve Stackleme Baþlangýcý (KahveAl Tag'i)
        if (other.CompareTag("KahveAl"))
        {
            if (stackingLoop == null)
                stackingLoop = StartCoroutine(StackSpawnLoop());
        }

        // Kahve Býrakma Baþlangýcý (KahveBirakmaNoktasi Tag'i)
        if (other.CompareTag(dropAreaTag))
        {
            isInDropArea = true;
            if (dropLoop == null)
                dropLoop = StartCoroutine(DropSequence());
        }
    }

    public void Aktifet()
    {
        // 1. Kahve Satýþýný Kapatan Butonu Animasyonlu Küçültme
        // Butonun boyutunu 0'a düþür ve yok etme iþlemini planla
        if (butonKahve != null)
        {
            // Butonu hemen devre dýþý býrak, böylece tekrar basýlamaz
            if (butonKahve.TryGetComponent<UnityEngine.UI.Button>(out var buttonComponent))
            {
                buttonComponent.interactable = false;
            }

            // Butonu yavaþça küçült ve animasyon bitince GameObject'i kapat
            butonKahve.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => butonKahve.SetActive(false));

            if (MoneyManager.Instance.money >= currentPrice)
            {
                MoneyManager.Instance.AddMoney(-currentPrice);
            }
            else
            {
                Debug.Log("Yeterli paran yok!");
            }
        }

        // 2. Kahve Alanýný Animasyonlu Açma
        // Alaný hemen aktif et ve ardýndan yavaþça yukarý kaydýr veya boyutunu artýr.
        if (alan != null)
        {
            alan.SetActive(true);

            // Örnek Animasyon: Alaný hafifçe yukarý kaldýr
            Vector3 originalPosition = alan.transform.localPosition;
            alan.transform.localPosition = originalPosition + Vector3.up * 1f; // Baþlangýç pozisyonunu biraz aþaðý çek

            // Yavaþça orijinal yerine kaydýr
            alan.transform.DOLocalMove(originalPosition, 0.6f).SetEase(Ease.OutBack);
        }

        // 3. Diðer Butonu Animasyonlu Getirme
        if (buton != null)
        {
            buton.SetActive(true);

            // Butonu sýfýr boyuttan orijinal boyutuna getir (Fade-in gibi)
            buton.transform.localScale = Vector3.zero;
            buton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

        // 4. Oyun Mantýðýný Aktif Etme
        MusteriHareket.kahveAcik = true;
        StackCollector.Instance.KahveSatisiniAc();
    }

    void OnTriggerExit(Collider other)
    {
        // Kahve Stackleme Sonu
        if (other.CompareTag("KahveAl"))
        {
            if (stackingLoop != null)
            {
                StopCoroutine(stackingLoop);
                stackingLoop = null;
            }
        }

        // Kahve Býrakma Sonu
        if (other.CompareTag(dropAreaTag))
        {
            isInDropArea = false;
            if (dropLoop != null)
            {
                // Coroutine'in hemen durmasý yerine, DropSequence içinde kendini bitirmesi daha iyidir,
                // ancak ani çýkýþ için durdurabiliriz.
                StopCoroutine(dropLoop);
                dropLoop = null;
            }
        }
    }

    // --- UI Güncelleme ---

    void GuncelleUI()
    {
        if (stokText != null)
            stokText.text = $"{kahveStogu}";
        if (hamKahveText != null)
            hamKahveText.text = $"{hamKahve}/10";
        

    }

    public int StackCount => stack.Count;
    public int DropCount => dropList.Count;
}