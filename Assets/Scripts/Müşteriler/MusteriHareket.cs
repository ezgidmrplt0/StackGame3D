using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusteriHareket : MonoBehaviour
{
    public enum MusteriTipi { Normal, Dondurma }

    [Header("Müşteri Tipi")]
    public MusteriTipi musteriTipi = MusteriTipi.Normal;

    [Header("Hareket Ayarları")]
    public float moveSpeed = 10f;
    public float takipMesafesi = 6f;
    [SerializeField] private float musteriYukseklik = 7.5f;

    [Header("Sipariş Bilgisi")]
    public int kuyruktakiSirasi;
    public int istenenUrunSayisi = 1;
    public int alinanUrunSayisi = 0;
    // Ürün Tipleri: 0: Çay, 1: Soda, 2: Dondurma, 3: Kahve
    public int requestedProductType = 0;

    [Header("Random Sipariş Ayarları")]
    public int minUrunSayisi = 1;
    public int maxUrunSayisi = 3;

    // Durum bayrakları
    private bool isAtCounter = false;
    private bool hasBeenServed = false;
    private bool isLeaving = false;
    private bool kuyruktanCikarildi = false;
    private bool waitingAfterProduct = false;
    private float waitTimer = 0f;

    // Dondurma rotası ve normal müşteri kayması
    private bool hasDoneInitialShift = false;
    private Vector3 initialShiftTarget;
    private bool hasReachedIceCreamPoint1 = false;
    private bool hasReachedIceCreamCounter = false;
    private bool hasReachedIceCreamDeletionPoint = false;

    [Header("UI")]
    public TextMeshProUGUI urunText; // (isteğe bağlı – kullanılmıyorsa Inspector’dan boş bırakılabilir)

    [Header("Noktalar (Tag ile bulunur, yoksa log atar)")]
    private Transform musteriNoktasi;
    private Transform dondurmaNoktasi1;
    private Transform dondurmaSatisAlani;
    private Transform dondurmaMusteriSilmeNoktasi;
    private Transform spawnPoint;
    private Transform musteriFinal;

    private Animator animator;
    private Collider musteriCollider;

    // Alan dolulukları ve açılış durumları
    private static bool satisAlaniDolu = false;
    private static bool dondurmaAlaniDolu = false;
    public static bool sodaAcik = false;
    public static bool dondurmaAcik = false;
    public static bool kahveAcik = false;

    [Header("UI Baloncuk")]
    public GameObject baloncukPanel;
    public TextMeshProUGUI productText;

    [Header("Ürün Görselleri")]
    public RawImage cayImage;
    public RawImage sodaImage;
    public RawImage dondurmaImage;
    public RawImage kahveImage;

    // Otomatik dondurma alma
    [SerializeField] private float urunAlmaAraligi = 0.5f;
    private float dondurmaAlmaTimer = 0f;

    [Header("Sinir Ayarları")]
    public GameObject angryBubble;
    private float angryTimer = 0f;
    public float maxWaitTime = 10f;
    private bool isAngry = false;

    private void Start()
    {
        // Ürün tipi seçimi
        if (musteriTipi == MusteriTipi.Dondurma)
        {
            requestedProductType = 2;
        }
        else
        {
            var availableProducts = new List<int> { 0 }; // çay hep var
            if (sodaAcik) availableProducts.Add(1);
            if (kahveAcik) availableProducts.Add(3);

            requestedProductType = availableProducts[Random.Range(0, availableProducts.Count)];
        }

        // Sipariş adedi
        if (maxUrunSayisi < minUrunSayisi) maxUrunSayisi = minUrunSayisi;
        istenenUrunSayisi = Random.Range(minUrunSayisi, maxUrunSayisi + 1);

        // Tag ile noktaları bul
        musteriNoktasi = GameObject.FindGameObjectWithTag("MusteriNoktasi")?.transform;
        dondurmaNoktasi1 = GameObject.FindGameObjectWithTag("dondurma1")?.transform;
        dondurmaSatisAlani = GameObject.FindGameObjectWithTag("DondurmaSatisAlani")?.transform;
        dondurmaMusteriSilmeNoktasi = GameObject.FindGameObjectWithTag("DondurmaMusteriSilinmeNoktasi")?.transform;
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi")?.transform;
        musteriFinal = GameObject.FindGameObjectWithTag("MusteriFinal")?.transform;

        if (musteriNoktasi == null) Debug.LogWarning("[MusteriHareket] 'MusteriNoktasi' tagli obje bulunamadi.");
        if (musteriFinal == null) Debug.LogWarning("[MusteriHareket] 'MusteriFinal' tagli obje bulunamadi.");
        if (musteriTipi == MusteriTipi.Dondurma)
        {
            if (dondurmaNoktasi1 == null) Debug.LogWarning("[MusteriHareket] 'dondurma1' tagli obje bulunamadi.");
            if (dondurmaSatisAlani == null) Debug.LogWarning("[MusteriHareket] 'DondurmaSatisAlani' tagli obje bulunamadi.");
            if (dondurmaMusteriSilmeNoktasi == null) Debug.LogWarning("[MusteriHareket] 'DondurmaMusteriSilinmeNoktasi' tagli obje bulunamadi.");
        }

        animator = GetComponent<Animator>();
        musteriCollider = GetComponent<Collider>();

        UpdateUI();

        // Y sabitle
        var p = transform.position;
        transform.position = new Vector3(p.x, musteriYukseklik, p.z);

        // Normal müşteri – başlangıç kayması
        if (musteriTipi == MusteriTipi.Normal)
        {
            float kaymaMiktari = 1.0f;
            initialShiftTarget = transform.position - transform.right * kaymaMiktari;
            initialShiftTarget.y = musteriYukseklik;
        }
    }

    public bool IsRequestingSoda() => requestedProductType == 1;
    public bool IsRequestingTea() => requestedProductType == 0;
    public bool IsRequestingIceCream() => requestedProductType == 2;
    public bool IsRequestingCoffee() => requestedProductType == 3;

    private void Update()
    {
        if (isLeaving) return;

        // Sinir sayaç yönetimi
        if (IsAtCounter() && !isAngry && alinanUrunSayisi < istenenUrunSayisi)
        {
            angryTimer += Time.deltaTime;
            if (angryTimer >= maxWaitTime) SetAngryState();
        }
        else angryTimer = 0f;

        // Ürün tesliminden sonra kısa bekleme
        if (waitingAfterProduct)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= 0.5f)
            {
                waitingAfterProduct = false;
                waitTimer = 0f;
                hasBeenServed = true;

                // Normal müşteri kuyruktan çıkar
                if (musteriTipi == MusteriTipi.Normal && !kuyruktanCikarildi)
                {
                    if (MusteriSpawner.musteriKuyrugu.Count > 0 &&
                        MusteriSpawner.musteriKuyrugu.Peek() == this)
                    {
                        MusteriSpawner.musteriKuyrugu.Dequeue();
                        MusteriSpawner.UpdateQueuePositions();
                    }
                    kuyruktanCikarildi = true;
                }
            }
            return;
        }

        // Dondurma – tezgahta otomatik ürün alma
        if (musteriTipi == MusteriTipi.Dondurma && hasReachedIceCreamCounter && NeedsMoreProducts())
        {
            if (KulahYenileme.Instance == null || KulahYenileme.Instance.mevcutKulahSayisi <= 0)
            {
                if (animator != null) animator.SetBool("isWalking", false);
            }
            else
            {
                dondurmaAlmaTimer += Time.deltaTime;
                if (dondurmaAlmaTimer >= urunAlmaAraligi)
                {
                    ReceiveProduct();
                    dondurmaAlmaTimer = 0f;
                }
            }
        }

        // ---------------- HAREKET MANTIĞI ----------------
        Transform hedefNokta = null;
        Vector3 hedefPozisyon = transform.position;

        // 1) Normal müşteri – ilk kayma
        if (musteriTipi == MusteriTipi.Normal && !hasDoneInitialShift)
        {
            hedefPozisyon = initialShiftTarget;
            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(initialShiftTarget.x, initialShiftTarget.z);
            if (Vector2.Distance(currentXZ, targetXZ) < 0.1f) hasDoneInitialShift = true;
        }
        // 2) Ayrılma rotası
        else if (hasBeenServed || isAngry)
        {
            if (musteriTipi == MusteriTipi.Dondurma)
            {
                hedefNokta = !hasReachedIceCreamDeletionPoint ? dondurmaMusteriSilmeNoktasi : musteriFinal;
            }
            else
            {
                hedefNokta = musteriFinal;
            }
        }
        // 3) Hizmet rotası
        else if (kuyruktakiSirasi == 0 || musteriTipi == MusteriTipi.Dondurma)
        {
            if (musteriTipi == MusteriTipi.Dondurma)
            {
                if (!hasReachedIceCreamPoint1) hedefNokta = dondurmaNoktasi1;
                else if (!hasReachedIceCreamCounter) hedefNokta = dondurmaSatisAlani;
                else
                {
                    if (animator != null) animator.SetBool("isWalking", false);
                    return;
                }
            }
            else
            {
                hedefNokta = musteriNoktasi;
            }

            if (hedefNokta != null)
            {
                float distanceToTarget = Vector3.Distance(
                    new Vector3(transform.position.x, 0, transform.position.z),
                    new Vector3(hedefNokta.position.x, 0, hedefNokta.position.z)
                );

                if (distanceToTarget < 1.5f && !isAtCounter)
                {
                    if (musteriTipi == MusteriTipi.Dondurma)
                    {
                        if (hedefNokta == dondurmaNoktasi1) hasReachedIceCreamPoint1 = true;
                        else if (hedefNokta == dondurmaSatisAlani)
                        {
                            hasReachedIceCreamCounter = true;
                            isAtCounter = true;
                        }
                        else if (hedefNokta == dondurmaMusteriSilmeNoktasi)
                        {
                            hasReachedIceCreamDeletionPoint = true;
                        }
                    }
                    else
                    {
                        isAtCounter = true;
                    }
                }
            }
        }
        // 4) Sırada bekleme (Normal)
        else
        {
            Queue<MusteriHareket> kuyruk = MusteriSpawner.musteriKuyrugu;
            MusteriHareket[] musteriler = kuyruk.ToArray();

            if (kuyruktakiSirasi > 0 && kuyruktakiSirasi < musteriler.Length)
            {
                MusteriHareket onundeki = musteriler[kuyruktakiSirasi - 1];
                Vector3 onundekiPozisyon = onundeki.transform.position;

                hedefPozisyon = new Vector3(
                    onundekiPozisyon.x - onundeki.transform.forward.x * takipMesafesi,
                    musteriYukseklik,
                    onundekiPozisyon.z - onundeki.transform.forward.z * takipMesafesi
                );
            }
        }

        // Hedef pozisyonu belirle
        if (hedefNokta != null)
        {
            hedefPozisyon = new Vector3(
                hedefNokta.position.x,
                musteriYukseklik,
                hedefNokta.position.z
            );
        }

        // Hareket
        HareketEt(hedefPozisyon);

        // Final yok etme kontrolü
        if (hedefNokta == musteriFinal && musteriFinal != null)
        {
            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(musteriFinal.position.x, musteriFinal.position.z);
            if (Vector2.Distance(currentXZ, targetXZ) < 0.1f) Destroy(gameObject);
        }
    }

    // ---- Slot'u boşalt ----
    private void FreeServiceSlot()
    {
        if (musteriCollider != null) musteriCollider.enabled = true;
        if (musteriTipi == MusteriTipi.Dondurma) dondurmaAlaniDolu = false;
        else satisAlaniDolu = false;
    }

    // ---- Sinirlenme akışı ----
    private void SetAngryState()
    {
        if (isAngry) return;

        isAngry = true;
        UpdateUI();

        if (!kuyruktanCikarildi)
        {
            MusteriSpawner.MusteriAyrildi(this);
            kuyruktanCikarildi = true;
        }

        FreeServiceSlot();

        hasBeenServed = true;
        isAtCounter = false;

        if (musteriTipi == MusteriTipi.Dondurma)
        {
            hasReachedIceCreamPoint1 = true;
            hasReachedIceCreamCounter = false;
            hasReachedIceCreamDeletionPoint = false;
        }
    }

    private void HareketEt(Vector3 hedefPozisyon)
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPositionXZ = new Vector3(hedefPozisyon.x, musteriYukseklik, hedefPozisyon.z);

        float distance = Vector3.Distance(
            new Vector3(currentPosition.x, 0, currentPosition.z),
            new Vector3(targetPositionXZ.x, 0, targetPositionXZ.z)
        );

        if (distance > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                new Vector3(currentPosition.x, musteriYukseklik, currentPosition.z),
                targetPositionXZ,
                moveSpeed * Time.deltaTime
            );

            Vector3 direction = (targetPositionXZ - new Vector3(currentPosition.x, musteriYukseklik, currentPosition.z)).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            if (animator != null) animator.SetBool("isWalking", false);

            // Dondurma – silme noktasına varıldıysa işaretle
            if (musteriTipi == MusteriTipi.Dondurma && !hasReachedIceCreamDeletionPoint && (hasBeenServed || isAngry))
            {
                if (dondurmaMusteriSilmeNoktasi != null)
                {
                    float d = Vector3.Distance(
                        transform.position,
                        new Vector3(dondurmaMusteriSilmeNoktasi.position.x, musteriYukseklik, dondurmaMusteriSilmeNoktasi.position.z)
                    );
                    if (d < 0.1f) hasReachedIceCreamDeletionPoint = true;
                }
            }
        }
    }

    #region Trigger Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MusteriSatis") && musteriTipi == MusteriTipi.Normal)
        {
            if (!satisAlaniDolu || isAtCounter) satisAlaniDolu = true;
        }
        else if (other.CompareTag("DondurmaSatisAlani") && musteriTipi == MusteriTipi.Dondurma)
        {
            if (!dondurmaAlaniDolu || hasReachedIceCreamCounter) dondurmaAlaniDolu = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MusteriSatis") && musteriTipi == MusteriTipi.Normal)
        {
            satisAlaniDolu = false;
            if (musteriCollider != null) musteriCollider.enabled = true;
        }
        else if (other.CompareTag("DondurmaSatisAlani") && musteriTipi == MusteriTipi.Dondurma)
        {
            dondurmaAlaniDolu = false;
            if (musteriCollider != null) musteriCollider.enabled = true;
        }
    }
    #endregion

    #region Ürün Alma
    public bool IsAtCounter() =>
        (musteriTipi == MusteriTipi.Normal && isAtCounter && !hasBeenServed && !isLeaving && !waitingAfterProduct) ||
        (musteriTipi == MusteriTipi.Dondurma && hasReachedIceCreamCounter && !hasBeenServed && !isLeaving && !waitingAfterProduct);

    public bool NeedsMoreProducts() => alinanUrunSayisi < istenenUrunSayisi;

    public bool CanReceiveProduct() =>
        !hasBeenServed && alinanUrunSayisi < istenenUrunSayisi && IsAtCounter();

    public void ReceiveProduct()
    {
        if (!CanReceiveProduct()) return;

        if (musteriTipi == MusteriTipi.Dondurma)
        {
            if (KulahYenileme.Instance == null || KulahYenileme.Instance.mevcutKulahSayisi <= 0) return;

            KulahYenileme.Instance.KulahKullan();
            if (MoneyManager.Instance != null) MoneyManager.Instance.AddMoney(10);
        }
        else
        {
            // Normal ürünler için para/anim vb. buraya eklenebilir.
        }

        alinanUrunSayisi++;
        UpdateUI();

        if (alinanUrunSayisi >= istenenUrunSayisi)
        {
            waitingAfterProduct = true;
            waitTimer = 0f;
        }
    }
    #endregion

    private void UpdateUI()
    {
        // Tüm ikonları kapat
        if (cayImage) cayImage.gameObject.SetActive(false);
        if (sodaImage) sodaImage.gameObject.SetActive(false);
        if (dondurmaImage) dondurmaImage.gameObject.SetActive(false);
        if (kahveImage) kahveImage.gameObject.SetActive(false);
        if (angryBubble) angryBubble.gameObject.SetActive(false);

        if (baloncukPanel == null || productText == null) return;

        if (isAngry)
        {
            baloncukPanel.SetActive(true);
            if (angryBubble) angryBubble.gameObject.SetActive(true);
            productText.text = "";
            return;
        }

        int kalan = Mathf.Max(0, istenenUrunSayisi - alinanUrunSayisi);
        if (kalan <= 0)
        {
            baloncukPanel.SetActive(false);
            return;
        }

        baloncukPanel.SetActive(true);

        switch (requestedProductType)
        {
            case 0: if (cayImage) cayImage.gameObject.SetActive(true); break;
            case 1: if (sodaImage) sodaImage.gameObject.SetActive(true); break;
            case 2: if (dondurmaImage) dondurmaImage.gameObject.SetActive(true); break;
            case 3: if (kahveImage) kahveImage.gameObject.SetActive(true); break;
        }
        productText.text = $"{kalan}";
    }

    private void OnDestroy()
    {
        // Idempotent ayrılma
        MusteriSpawner.MusteriAyrildi(this);

        // Slotu açık bırak
        if (musteriTipi == MusteriTipi.Normal) satisAlaniDolu = false;
        else dondurmaAlaniDolu = false;

        if (musteriCollider != null) musteriCollider.enabled = true;
    }
}
