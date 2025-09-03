using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class StackCollector : MonoBehaviour
{
    public static StackCollector Instance; // Singleton

    [Header("Stack Ayarları")]
    public GameObject cubePrefab;
    public Transform stackRoot;
    public float cubeHeight = 0.5f;
    public float spawnInterval = 0.2f;
    public float tweenDuration = 0.4f;
    public Ease tweenEase = Ease.OutBack;

    [Header("Küp Boyutu")]
    public Vector3 cubeTargetScale = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("Drop Alanı")]
    public Transform dropArea;
    public float dropRadius = 1.5f;
    public List<Transform> dropList = new List<Transform>();

    [Header("Stack Bırakma Ayarları")]
    public Transform stackAreaTarget;

    [Header("Kasiyer Ayarları")]
    public GameObject kasiyerPrefab;
    public Transform kasiyerSpawnPoint;
    public Transform kasiyerSalesPoint;

    // ─────────────────────────────────────────────────────────────
    // ÇAY SİSTEMİ (Hammadde → Depo → Üretim)
    [Header("Çay Sistemi")]
    [Tooltip("Ot toplama alanı tag'i")]
    public string cayToplamaTag = "CayToplamaNoktasi";
    [Tooltip("Toplanan otların bırakıldığı alan tag'i")]
    public string cayBirakmaTag = "CayBirakmaNoktasi";
    [Tooltip("Üretim için bekleme (StackNoktasi0 zaten var)")]
    public int hamCayTasimaLimiti = 30;
    public float toplamaAraligi = 0.15f;
    public int toplamaAdedi = 1;
    public TextMeshProUGUI hamCayText;
    public TextMeshProUGUI uretimStoguText;

    private int uzerimdeHamCay = 0;
    private int uretimStogu = 0;
    private int toplamaTriggerSayaci = 0;
    private bool birakmaAlaninda = false;
    private Coroutine toplamaLoop;
    private Coroutine birakmaLoop;

    // ─────────────────────────────────────────────────────────────
    // Ham Yaprak Kuyruk Stack
    [Header("Ham Yaprak Stack (Arkada Kuyruk)")]
    public GameObject hamCayPrefab;
    public Transform hamCayStackRoot;
    public float hamCaySpacing = 0.5f;
    private List<Transform> hamCayStack = new List<Transform>();
    // ─────────────────────────────────────────────────────────────

    private readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;
    private Coroutine dropLoop;
    private bool isInDropArea = false;
    private bool isInSalesArea = false;
    private bool isSelling = false;
    private float lastSellTime = 0f;
    public float sellCooldown = 0.1f;

    private List<KasiyerHareket> activeKasiyers = new List<KasiyerHareket>();

    private int stackLimit = 5;
    public void SetStackLimit(int newLimit) => stackLimit = newLimit;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        UpdateStackPositions();        // Çay küpleri (dikey)
        UpdateHamCayStackPositions();  // Yaprak kuyruğu

        if (isInSalesArea && dropList.Count > 0 && Time.time - lastSellTime > sellCooldown)
            TrySellToCustomer();

        UpdateKasiyerSales();
        GuncelleUI();
    }

    void UpdateKasiyerSales()
    {
        foreach (var kasiyer in activeKasiyers)
            if (kasiyer != null && kasiyer.IsAtSalesPoint() && dropList.Count > 0)
                kasiyer.TrySellProducts();
    }

    void UpdateStackPositions()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            Transform cube = stack[i];
            float yOffset = cubeTargetScale.y * 0.5f;
            Vector3 targetPos = stackRoot.position + Vector3.up * (cubeHeight * i + yOffset);
            cube.position = targetPos;
            cube.rotation = Quaternion.identity;
        }
    }

    void UpdateHamCayStackPositions()
    {
        for (int i = 0; i < hamCayStack.Count; i++)
        {
            Transform leaf = hamCayStack[i];
            Vector3 targetPos = hamCayStackRoot.position - hamCayStackRoot.forward * hamCaySpacing * i;
            leaf.position = Vector3.Lerp(leaf.position, targetPos, Time.deltaTime * 10f);
            leaf.rotation = Quaternion.Lerp(leaf.rotation, hamCayStackRoot.rotation, Time.deltaTime * 10f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(cayToplamaTag))
        {
            toplamaTriggerSayaci++;
            if (toplamaLoop == null)
                toplamaLoop = StartCoroutine(ToplamaLoop());
        }

        if (other.CompareTag(cayBirakmaTag))
        {
            birakmaAlaninda = true;
            if (birakmaLoop == null)
                birakmaLoop = StartCoroutine(BirakmaLoop());
        }

        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop == null)
                stackingLoop = StartCoroutine(SpawnLoop());
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = true;
            if (dropLoop == null)
                dropLoop = StartCoroutine(DropSequence());
        }

        if (other.CompareTag("SatisNoktasi"))
        {
            isInSalesArea = true;
            Debug.Log("Satış alanına girdi");
        }

        if (other.CompareTag("YukseltmeNoktasi"))
            TryBuyKasiyer(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(cayToplamaTag))
        {
            toplamaTriggerSayaci = Mathf.Max(0, toplamaTriggerSayaci - 1);
            if (toplamaTriggerSayaci == 0 && toplamaLoop != null)
            {
                StopCoroutine(toplamaLoop);
                toplamaLoop = null;
            }
        }

        if (other.CompareTag(cayBirakmaTag))
        {
            birakmaAlaninda = false;
            if (birakmaLoop != null)
            {
                StopCoroutine(birakmaLoop);
                birakmaLoop = null;
            }
        }

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
            if (dropLoop != null)
            {
                StopCoroutine(dropLoop);
                dropLoop = null;
            }
        }

        if (other.CompareTag("SatisNoktasi"))
        {
            isInSalesArea = false;
            Debug.Log("Satış alanından çıktı");
        }
    }

    // ─────────────────────────────────────────────────────────────
    IEnumerator ToplamaLoop()
    {
        var wait = new WaitForSeconds(toplamaAraligi);
        while (true)
        {
            if (toplamaTriggerSayaci <= 0) break;

            if (uzerimdeHamCay < hamCayTasimaLimiti)
            {
                uzerimdeHamCay += toplamaAdedi;
                uzerimdeHamCay = Mathf.Min(uzerimdeHamCay, hamCayTasimaLimiti);

                AddHamCayCube(); // 🌿 yaprak kuyruk ekle
            }
            yield return wait;
        }
        toplamaLoop = null;
    }

    IEnumerator BirakmaLoop()
    {
        var wait = new WaitForSeconds(0.05f);
        while (birakmaAlaninda)
        {
            if (uzerimdeHamCay > 0)
            {
                uzerimdeHamCay--;
                uretimStogu++;

                RemoveHamCayCube(); // 🌿 yaprak kuyruktan çıkar
            }
            yield return wait;
        }
        birakmaLoop = null;
    }
    // ─────────────────────────────────────────────────────────────

    void AddHamCayCube()
    {
        if (hamCayPrefab == null || hamCayStackRoot == null) return;

        Vector3 offset = -hamCayStackRoot.forward * hamCaySpacing * hamCayStack.Count;
        Vector3 spawnPos = hamCayStackRoot.position + offset;

        GameObject newLeaf = Instantiate(hamCayPrefab, spawnPos, Quaternion.identity);
        newLeaf.transform.localScale = Vector3.zero;
        newLeaf.transform.DOScale(Vector3.one * 0.3f, 0.3f).SetEase(Ease.OutBack);
        newLeaf.transform.SetParent(hamCayStackRoot);

        hamCayStack.Add(newLeaf.transform);
    }

    void RemoveHamCayCube()
    {
        if (hamCayStack.Count == 0) return;

        Transform lastLeaf = hamCayStack[hamCayStack.Count - 1];
        hamCayStack.RemoveAt(hamCayStack.Count - 1);

        lastLeaf.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(lastLeaf.gameObject));
    }

    // ─────────────────────────────────────────────────────────────
    // ÇAY ÜRETİM ve SATIŞ SİSTEMİ (senin kodun aynı kaldı)
    // ─────────────────────────────────────────────────────────────

    void TryBuyKasiyer(GameObject yukseltmeNoktasi)
    {
        int kasiyerFiyati = 0;
        if (MoneyManager.Instance.money >= kasiyerFiyati)
            StartCoroutine(SlowlyPayForKasiyer(kasiyerFiyati, yukseltmeNoktasi));
        else
            Debug.Log("Yeterli para yok!");
    }

    IEnumerator SlowlyPayForKasiyer(int fiyat, GameObject yukseltmeNoktasi)
    {
        int kalan = fiyat;
        while (kalan > 0)
        {
            MoneyManager.Instance.AddMoney(-1);
            kalan--;
            yield return new WaitForSeconds(0.01f);
        }

        if (kasiyerPrefab != null && kasiyerSpawnPoint != null)
        {
            GameObject yeniKasiyer = Instantiate(kasiyerPrefab, kasiyerSpawnPoint.position, kasiyerSpawnPoint.rotation);
            Debug.Log("Kasiyer satın alındı!");

            KasiyerHareket kasiyerScript = yeniKasiyer.GetComponent<KasiyerHareket>();
            if (kasiyerScript != null)
            {
                if (kasiyerSalesPoint != null)
                    kasiyerScript.satisNoktasi = kasiyerSalesPoint;
                else
                {
                    GameObject hedefObj = GameObject.FindGameObjectWithTag("SatisNoktasi");
                    if (hedefObj != null) kasiyerScript.satisNoktasi = hedefObj.transform;
                }
                activeKasiyers.Add(kasiyerScript);
            }
        }
        Destroy(yukseltmeNoktasi);
    }

    public bool SellProduct()
    {
        if (dropList.Count == 0) return false;

        Transform product = dropList[dropList.Count - 1];
        dropList.RemoveAt(dropList.Count - 1);

        if (MusteriSpawner.musteriKuyrugu.Count > 0)
        {
            MusteriHareket customer = MusteriSpawner.musteriKuyrugu.Peek();
            if (customer != null && customer.IsAtCounter() && customer.NeedsMoreProducts())
            {
                Vector3 customerPosition = customer.transform.position + Vector3.up * 1.5f;

                product.DOMove(customerPosition, 0.2f)
                      .SetEase(Ease.OutQuad)
                      .OnComplete(() =>
                      {
                          customer.ReceiveProduct();
                          Destroy(product.gameObject);
                          MoneyManager.Instance.AddMoney(1);
                      });
                return true;
            }
        }
        dropList.Add(product);
        return false;
    }

    void TrySellToCustomer()
    {
        if (isSelling) return;

        if (MusteriSpawner.musteriKuyrugu.Count > 0)
        {
            MusteriHareket currentCustomer = MusteriSpawner.musteriKuyrugu.Peek();
            if (currentCustomer != null && currentCustomer.IsAtCounter() && currentCustomer.NeedsMoreProducts() && dropList.Count > 0)
                StartCoroutine(GiveProductsToCustomer(currentCustomer));
        }
    }

    IEnumerator GiveProductsToCustomer(MusteriHareket customer)
    {
        if (dropList.Count == 0 || isSelling) yield break;

        isSelling = true;
        lastSellTime = Time.time;

        int needed = customer.istenenUrunSayisi - customer.alinanUrunSayisi;
        int toGive = Mathf.Min(needed, dropList.Count);

        for (int i = 0; i < toGive; i++)
        {
            Transform product = dropList[dropList.Count - 1];
            dropList.RemoveAt(dropList.Count - 1);

            Vector3 customerPosition = customer.transform.position + Vector3.up * 1.5f;

            product.DOMove(customerPosition, 0.15f)
                   .SetEase(Ease.OutQuad)
                   .OnComplete(() =>
                   {
                       customer.ReceiveProduct();
                       Destroy(product.gameObject);
                       MoneyManager.Instance.AddMoney(1);
                   });

            yield return new WaitForSeconds(0.05f);
        }
        isSelling = false;
    }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            if (uretimStogu > 0 && stack.Count < stackLimit)
            {
                uretimStogu--;
                AddOneCube();
            }
            yield return wait;
        }
    }

    void AddOneCube()
    {
        if (stack.Count >= stackLimit) return;

        float yOffset = cubeTargetScale.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (cubeHeight * stack.Count + yOffset);

        GameObject newCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

        Rigidbody cubeRb = newCube.GetComponent<Rigidbody>();
        if (cubeRb != null)
        {
            cubeRb.isKinematic = true;
            cubeRb.useGravity = false;
            cubeRb.drag = 10f;
        }

        Collider cubeCollider = newCube.GetComponent<Collider>();
        if (cubeCollider != null && cubeCollider is BoxCollider boxCollider)
            boxCollider.size = cubeTargetScale * 0.9f;

        newCube.transform.localScale = Vector3.zero;
        newCube.transform.DOScale(cubeTargetScale, tweenDuration).SetEase(tweenEase);

        stack.Add(newCube.transform);
    }

    IEnumerator DropSequence()
    {
        float dropSpacing = 2f;
        while (stack.Count > 0 && isInDropArea)
        {
            Transform cube = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null)
            {
                cubeRb.isKinematic = false;
                cubeRb.useGravity = true;
                cubeRb.drag = 1f;
            }

            Collider cubeCollider = cube.GetComponent<Collider>();
            if (cubeCollider != null && cubeCollider is BoxCollider boxCollider)
                boxCollider.size = Vector3.one;

            cube.SetParent(null);

            int targetIndex = dropList.Count;
            Vector3 targetPos = stackAreaTarget.position + new Vector3(0f, dropSpacing * targetIndex, 0f);

            cube.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);
            dropList.Add(cube);

            yield return new WaitForSeconds(0.1f);
        }
    }

    public int StackCount => stack.Count;
    public int DropCount => dropList.Count;

    void GuncelleUI()
    {
        if (hamCayText != null) hamCayText.text = $"Yaprak: {uzerimdeHamCay}/{hamCayTasimaLimiti}";
        if (uretimStoguText != null) uretimStoguText.text = $"Üretim Stoku: {uretimStogu}";
    }
}
