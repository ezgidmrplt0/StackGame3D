using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class StackCollector : MonoBehaviour
{
    public static StackCollector Instance; // Singleton pattern

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

    private readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;
    private Coroutine dropLoop;
    private bool isInDropArea = false;
    private bool isInSalesArea = false;
    private bool isSelling = false;
    private float lastSellTime = 0f;
    public float sellCooldown = 0.1f;

    private List<KasiyerHareket> activeKasiyers = new List<KasiyerHareket>();

    // 🔥 Dinamik stack limiti
    private int stackLimit = 5; // Başlangıç limiti
    public void SetStackLimit(int newLimit) => stackLimit = newLimit;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        UpdateStackPositions();

        if (isInSalesArea && dropList.Count > 0 && Time.time - lastSellTime > sellCooldown)
        {
            TrySellToCustomer();
        }

        UpdateKasiyerSales();
    }

    void UpdateKasiyerSales()
    {
        foreach (var kasiyer in activeKasiyers)
        {
            if (kasiyer != null && kasiyer.IsAtSalesPoint() && dropList.Count > 0)
            {
                kasiyer.TrySellProducts();
            }
        }
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
            if (dropLoop == null)
                dropLoop = StartCoroutine(DropSequence());
        }

        if (other.CompareTag("SatisNoktasi"))
        {
            isInSalesArea = true;
            Debug.Log("Satış alanına girdi");
        }

        if (other.CompareTag("YukseltmeNoktasi"))
        {
            TryBuyKasiyer(other.gameObject);
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

    void TryBuyKasiyer(GameObject yukseltmeNoktasi)
    {
        int kasiyerFiyati = 0;

        if (MoneyManager.Instance.money >= kasiyerFiyati)
        {
            StartCoroutine(SlowlyPayForKasiyer(kasiyerFiyati, yukseltmeNoktasi));
        }
        else
        {
            Debug.Log("Yeterli para yok! Gerekli: " + kasiyerFiyati + ", Mevcut: " + MoneyManager.Instance.money);
        }
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
                {
                    kasiyerScript.satisNoktasi = kasiyerSalesPoint;
                }
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
            {
                StartCoroutine(GiveProductsToCustomer(currentCustomer));
            }
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
            AddOneCube();
            yield return wait;
        }
    }

    void AddOneCube()
    {
        if (stack.Count >= stackLimit) // 🔥 burası artık dinamik
            return;

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
        if (cubeCollider != null && cubeCollider is BoxCollider)
        {
            BoxCollider boxCollider = cubeCollider as BoxCollider;
            boxCollider.size = cubeTargetScale * 0.9f;
        }

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
            if (cubeCollider != null && cubeCollider is BoxCollider)
            {
                BoxCollider boxCollider = cubeCollider as BoxCollider;
                boxCollider.size = Vector3.one;
            }

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
}
