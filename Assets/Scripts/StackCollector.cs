using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class StackCollector : MonoBehaviour
{
    public static StackCollector Instance;

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
    private List<KasiyerHareket> activeKasiyers = new List<KasiyerHareket>();

    [Header("Kasiyer Ekonomi")]
    public int kasiyerCost = 100;
    public float costIncreaseRate = 0.2f;
    public float kasiyerDuration = 20f;
    public TextMeshProUGUI kasiyerFiyatText;

    [Header("Çay Sistemi")]
    public string cayToplamaTag = "CayToplamaNoktasi";
    public string cayBirakmaTag = "CayBirakmaNoktasi";
    public int hamCayTasimaLimiti = 20;
    public float toplamaAraligi = 0.5f;
    public int toplamaAdedi = 1;
    public TextMeshPro hamCayText;
    public TextMeshPro uretimStoguText;
    private int uzerimdeHamCay = 0;
    public int uretimStogu = 0;
    private int toplamaTriggerSayaci = 0;
    private bool birakmaAlaninda = false;
    private Coroutine toplamaLoop;
    private Coroutine birakmaLoop;

    [Header("Ham Yaprak Stack")]
    public GameObject hamCayPrefab;
    public Transform hamCayStackRoot;
    public float hamCaySpacing = 0.5f;
    public Vector3 hamCayTargetScale = new Vector3(0.5f, 0.5f, 0.5f);
    private List<Transform> hamCayStack = new List<Transform>();

    public readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;
    private Coroutine dropLoop;
    private bool isInDropArea = false;
    private bool isInSalesArea = false;
    private bool isSelling = false;
    private float lastSellTime = 0f;
    public float sellCooldown = 0.1f;

    [Header("Depocu Ayarları")]
    public GameObject depocuPrefab;
    public Transform depocuSpawnPoint;
    public Transform cayToplamaNoktasi;
    public Transform cayBirakmaNoktasi;
    private List<DepocuCalisan> activeDepocular = new List<DepocuCalisan>();

    [Header("Depocu Ekonomi")]
    public int depocuCost = 100;
    public float depocuCostIncreaseRate = 0.2f;
    public float depocuDuration = 20f;
    public TextMeshProUGUI depocuFiyatText;

    [Header("Satış Ayarları")]
    public int cayFiyati = 2;
    public int sodaFiyati = 5;
    private Dictionary<MusteriHareket, float> sonSatisZamanlari = new Dictionary<MusteriHareket, float>();
    private float satisCoolDown = 0.5f;

    [Header("Soda Satış Kontrol")]
    public bool sodaSatisAktif = true;
    private Dictionary<MusteriHareket, float> sonSodaSatisZamanlari = new Dictionary<MusteriHareket, float>();
    private int stackLimit = 5;

    [Header("Urun Tasiyici Ayarları")]
    public GameObject urunTasiyiciPrefab;
    public Transform urunTasiyiciSpawnPoint;
    public Transform urunTasiyiciAlmaNoktasi;
    public Transform urunTasiyiciBirakmaNoktasi;
    public Transform urunTasiyiciStackTarget;
    private List<UrunTasiyici> activeTasiyicilar = new List<UrunTasiyici>();

    [Header("Urun Tasiyici Ekonomi")]
    public int urunTasiyiciCost = 100;
    public float urunTasiyiciCostIncreaseRate = 0.2f;
    public float urunTasiyiciDuration = 20f;
    public TextMeshProUGUI urunTasiyiciFiyatText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateKasiyerUI();
    }

    void Update()
    {
        UpdateStackPositions();
        UpdateHamCayStackPositions();

        if (isInSalesArea && Time.time - lastSellTime > sellCooldown)
        {
            if (dropList.Count > 0)
                TrySellToCustomer();

            if (SodaStack.Instance != null && SodaStack.Instance.sodaDropList.Count > 0)
                TrySellSodaToCustomer();
        }

        UpdateKasiyerSales();
        GuncelleUI();
    }

    void TrySellSodaToCustomer()
    {
        if (!sodaSatisAktif) return;
        if (isSelling) return;
        if (MusteriSpawner.musteriKuyrugu.Count == 0) return;

        MusteriHareket customer = MusteriSpawner.musteriKuyrugu.Peek();
        if (customer == null || !customer.CanReceiveProduct() || !customer.IsRequestingSoda()) return;

        if (sonSodaSatisZamanlari.ContainsKey(customer) &&
            Time.time - sonSodaSatisZamanlari[customer] < satisCoolDown) return;

        StartCoroutine(GiveSingleSodaToCustomer(customer));
    }

    IEnumerator GiveSingleSodaToCustomer(MusteriHareket customer)
    {
        if (SodaStack.Instance == null || SodaStack.Instance.sodaDropList.Count == 0 || isSelling)
            yield break;

        isSelling = true;

        if (SellSodaToCustomer(customer))
        {
            sonSodaSatisZamanlari[customer] = Time.time;
            lastSellTime = Time.time;
        }

        yield return new WaitForSeconds(0.1f);
        isSelling = false;
    }

    public bool SellSodaToCustomer(MusteriHareket customer)
    {
        if (customer == null || !customer.CanReceiveProduct() || !customer.IsRequestingSoda() ||
            SodaStack.Instance == null || SodaStack.Instance.sodaDropList.Count == 0)
            return false;

        Transform soda = SodaStack.Instance.sodaDropList[SodaStack.Instance.sodaDropList.Count - 1];
        SodaStack.Instance.sodaDropList.RemoveAt(SodaStack.Instance.sodaDropList.Count - 1);

        Vector3 customerPosition = customer.transform.position + Vector3.up * 1.5f;

        soda.DOMove(customerPosition, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                customer.ReceiveProduct();
                Destroy(soda.gameObject);
                MoneyManager.Instance.AddMoney(sodaFiyati);
                Debug.Log($"{customer.name} için soda satıldı! {sodaFiyati}$ kazanıldı!");
            });

        return true;
    }

    void UpdateKasiyerSales()
    {
        foreach (var kasiyer in activeKasiyers)
        {
            if (kasiyer != null && kasiyer.IsAtSalesPoint())
            {
                if (dropList.Count > 0)
                    TrySellWithKasiyer(kasiyer);

                if (SodaStack.Instance != null && SodaStack.Instance.sodaDropList.Count > 0)
                    TrySellSodaWithKasiyer(kasiyer);
            }
        }
    }

    void TrySellSodaWithKasiyer(KasiyerHareket kasiyer)
    {
        if (!sodaSatisAktif) return;
        if (MusteriSpawner.musteriKuyrugu.Count == 0) return;

        MusteriHareket customer = MusteriSpawner.musteriKuyrugu.Peek();
        if (customer == null || !customer.CanReceiveProduct() || !customer.IsRequestingSoda()) return;

        if (sonSodaSatisZamanlari.ContainsKey(customer) &&
            Time.time - sonSodaSatisZamanlari[customer] < satisCoolDown) return;

        if (SellSodaToCustomer(customer))
        {
            sonSodaSatisZamanlari[customer] = Time.time;
            lastSellTime = Time.time;
        }
    }

    void TrySellWithKasiyer(KasiyerHareket kasiyer)
    {
        if (MusteriSpawner.musteriKuyrugu.Count == 0) return;

        MusteriHareket customer = MusteriSpawner.musteriKuyrugu.Peek();
        if (customer == null || !customer.CanReceiveProduct() || !customer.IsRequestingTea()) return;

        if (sonSatisZamanlari.ContainsKey(customer) &&
            Time.time - sonSatisZamanlari[customer] < satisCoolDown) return;

        if (SellProductToCustomer(customer))
        {
            sonSatisZamanlari[customer] = Time.time;
            lastSellTime = Time.time;
        }
    }

    void TrySellToCustomer()
    {
        if (isSelling) return;
        if (MusteriSpawner.musteriKuyrugu.Count == 0) return;

        MusteriHareket customer = MusteriSpawner.musteriKuyrugu.Peek();
        if (customer == null || !customer.CanReceiveProduct() || !customer.IsRequestingTea()) return;

        if (sonSatisZamanlari.ContainsKey(customer) &&
            Time.time - sonSatisZamanlari[customer] < satisCoolDown) return;

        StartCoroutine(GiveSingleProductToCustomer(customer));
    }

    void UpdateKasiyerUI()
    {
        if (kasiyerFiyatText != null)
            kasiyerFiyatText.text = $"Kasiyer Al ({kasiyerCost}$)";
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
            Vector3 targetPos = hamCayStackRoot.position + Vector3.up * hamCaySpacing * i;
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
        }
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
        }
    }

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
                AddHamCayCube();
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
                RemoveHamCayCube();
            }

            yield return wait;
        }

        birakmaLoop = null;
    }

    void AddHamCayCube()
    {
        if (hamCayPrefab == null || hamCayStackRoot == null) return;

        Vector3 offset = Vector3.up * hamCaySpacing * hamCayStack.Count;
        Vector3 spawnPos = hamCayStackRoot.position + offset;

        GameObject newLeaf = Instantiate(hamCayPrefab, spawnPos, Quaternion.identity);
        newLeaf.transform.localScale = Vector3.zero;
        newLeaf.transform.DOScale(hamCayTargetScale, 0.3f).SetEase(Ease.OutBack);
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

    public void KasiyerAktifEt()
    {
        if (MoneyManager.Instance.money < kasiyerCost)
        {
            Debug.Log("Yetersiz para! Gerekli: " + kasiyerCost);
            return;
        }

        MoneyManager.Instance.AddMoney(-kasiyerCost);
        kasiyerCost = Mathf.CeilToInt(kasiyerCost * (1 + costIncreaseRate));
        UpdateKasiyerUI();

        if (kasiyerPrefab != null && kasiyerSpawnPoint != null)
        {
            GameObject yeniKasiyer = Instantiate(kasiyerPrefab, kasiyerSpawnPoint.position, kasiyerSpawnPoint.rotation);
            KasiyerHareket kasiyerScript = yeniKasiyer.GetComponent<KasiyerHareket>();

            if (kasiyerScript != null)
            {
                if (kasiyerSalesPoint != null)
                    kasiyerScript.satisNoktasi = kasiyerSalesPoint;
                else
                {
                    GameObject hedefObj = GameObject.FindGameObjectWithTag("SatisNoktasi");
                    if (hedefObj != null)
                        kasiyerScript.satisNoktasi = hedefObj.transform;
                }

                activeKasiyers.Add(kasiyerScript);
                StartCoroutine(KasiyerSuresiBitinceYokEt(yeniKasiyer, kasiyerScript));
            }
        }
    }

    IEnumerator KasiyerSuresiBitinceYokEt(GameObject kasiyerObj, KasiyerHareket kasiyerScript)
    {
        yield return new WaitForSeconds(kasiyerDuration);

        if (activeKasiyers.Contains(kasiyerScript))
            activeKasiyers.Remove(kasiyerScript);

        Destroy(kasiyerObj);
        Debug.Log("Kasiyer süresi doldu!");
    }

    IEnumerator GiveSingleProductToCustomer(MusteriHareket customer)
    {
        if (dropList.Count == 0 || isSelling) yield break;

        isSelling = true;

        if (SellProductToCustomer(customer))
        {
            sonSatisZamanlari[customer] = Time.time;
            lastSellTime = Time.time;
        }

        yield return new WaitForSeconds(0.1f);
        isSelling = false;
    }

    public bool SellProductToCustomer(MusteriHareket customer)
    {
        if (customer == null || !customer.CanReceiveProduct() || dropList.Count == 0)
            return false;

        Transform product = dropList[dropList.Count - 1];
        dropList.RemoveAt(dropList.Count - 1);

        Vector3 customerPosition = customer.transform.position + Vector3.up * 1.5f;

        product.DOMove(customerPosition, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                customer.ReceiveProduct();
                Destroy(product.gameObject);
                MoneyManager.Instance.AddMoney(cayFiyati);
                Debug.Log($"{customer.name} için çay satıldı! {cayFiyati}$ kazanıldı!");
            });

        return true;
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

    public void DepocuAktifEt()
    {
        if (MoneyManager.Instance.money < depocuCost)
        {
            Debug.Log("Yetersiz para! Gerekli: " + depocuCost);
            return;
        }

        MoneyManager.Instance.AddMoney(-depocuCost);
        depocuCost = Mathf.CeilToInt(depocuCost * (1 + depocuCostIncreaseRate));

        if (depocuFiyatText != null)
            depocuFiyatText.text = $"Depocu Al ({depocuCost}$)";

        if (depocuPrefab != null && depocuSpawnPoint != null)
        {
            Quaternion spawnRotation = depocuSpawnPoint.rotation * Quaternion.Euler(0, 90, 0);
            GameObject yeniDepocu = Instantiate(depocuPrefab, depocuSpawnPoint.position, spawnRotation);

            DepocuCalisan depocuScript = yeniDepocu.GetComponent<DepocuCalisan>();
            if (depocuScript != null)
            {
                depocuScript.toplamaNoktasi = cayToplamaNoktasi;
                depocuScript.birakmaNoktasi = cayBirakmaNoktasi;
                depocuScript.hamCayPrefab = hamCayPrefab;
                depocuScript.stackRoot = yeniDepocu.transform;

                activeDepocular.Add(depocuScript);
                StartCoroutine(DepocuSuresiBitinceYokEt(depocuScript));
            }
        }
    }

    IEnumerator DepocuSuresiBitinceYokEt(DepocuCalisan depocu)
    {
        yield return new WaitForSeconds(depocuDuration);

        if (activeDepocular.Contains(depocu))
            activeDepocular.Remove(depocu);

        depocu.CalismayiBitir();
        Debug.Log("Depocu süresi doldu!");
    }

    public void UretimStokEkle(int miktar)
    {
        uretimStogu += miktar;
    }

    void GuncelleUI()
    {
        if (hamCayText != null)
            hamCayText.text = $"{uzerimdeHamCay}/{hamCayTasimaLimiti}";

        if (uretimStoguText != null)
            uretimStoguText.text = $"{uretimStogu}";
    }

    public void UrunTasiyiciAktifEt()
    {
        if (MoneyManager.Instance.money < urunTasiyiciCost)
        {
            Debug.Log("Yetersiz para! Gerekli: " + urunTasiyiciCost);
            return;
        }

        MoneyManager.Instance.AddMoney(-urunTasiyiciCost);
        urunTasiyiciCost = Mathf.CeilToInt(urunTasiyiciCost * (1 + urunTasiyiciCostIncreaseRate));

        if (urunTasiyiciFiyatText != null)
            urunTasiyiciFiyatText.text = $"Taşıyıcı Al ({urunTasiyiciCost}$)";

        if (urunTasiyiciPrefab != null && urunTasiyiciSpawnPoint != null)
        {
            GameObject yeniTasiyici = Instantiate(urunTasiyiciPrefab, urunTasiyiciSpawnPoint.position, urunTasiyiciSpawnPoint.rotation);
            UrunTasiyici script = yeniTasiyici.GetComponent<UrunTasiyici>();

            script.stackAlmaNoktasi = urunTasiyiciAlmaNoktasi;
            script.stackBirakmaNoktasi = urunTasiyiciBirakmaNoktasi;
            script.stackAreaTarget = urunTasiyiciStackTarget;
            script.stackCollector = this;

            activeTasiyicilar.Add(script);
            StartCoroutine(UrunTasiyiciSuresiBitinceYokEt(script));
        }
    }

    IEnumerator UrunTasiyiciSuresiBitinceYokEt(UrunTasiyici tasiyici)
    {
        yield return new WaitForSeconds(urunTasiyiciDuration);

        if (activeTasiyicilar.Contains(tasiyici))
            activeTasiyicilar.Remove(tasiyici);

        tasiyici.CalismayiBitir();
        Debug.Log("Ürün taşıyıcı süresi doldu!");
    }

    public void AddToDropList(Transform product)
    {
        if (product == null) return;

        product.SetParent(null);

        Rigidbody rb = product.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 1f;
        }

        dropList.Add(product);
        Debug.Log("Ürün dropList'e eklendi: " + dropList.Count);
    }

    public void SodaSatisiniAc()
    {
        sodaSatisAktif = true;
        Debug.Log("Soda satışı aktifleştirildi!");
    }

    public void SetStackLimit(int newLimit) => stackLimit = newLimit;
}