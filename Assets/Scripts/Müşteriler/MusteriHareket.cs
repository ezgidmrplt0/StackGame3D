using System.Collections;
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
    private float musteriYukseklik = 7.5f;

    [Header("Sipariş Bilgisi")]
    public int kuyruktakiSirasi; // Normal müşteriler için kullanılmaya devam edecek
    public int istenenUrunSayisi = 1;
    public int alinanUrunSayisi = 0;
    // Ürün Tipleri: 0: Çay, 1: Soda, 2: Dondurma, 3: Kahve
    public int requestedProductType = 0;

    [Header("Random Sipariş Ayarları")]
    public int minUrunSayisi = 1;
    public int maxUrunSayisi = 3;

    private bool isAtCounter = false;
    private bool hasBeenServed = false;
    private bool isLeaving = false;
    private bool kuyruktanCikarildi = false;
    private bool waitingAfterProduct = false;
    private float waitTimer = 0f;

    // Dondurma Rota Durumları ve Normal Müşteri Kayması
    private bool hasDoneInitialShift = false;
    private Vector3 initialShiftTarget;
    private bool hasReachedIceCreamPoint1 = false;
    private bool hasReachedIceCreamCounter = false;
    private bool hasReachedIceCreamDeletionPoint = false;

    [Header("UI")]
    public TextMeshProUGUI urunText;

    [Header("Noktalar")]
    private Transform musteriNoktasi;
    private Transform dondurmaNoktasi1;
    private Transform dondurmaSatisAlani;
    private Transform dondurmaMusteriSilmeNoktasi;
    private Transform spawnPoint;
    private Transform musteriFinal;

    private Animator animator;
    private Collider musteriCollider;

    private static bool satisAlaniDolu = false;
    private static bool dondurmaAlaniDolu = false;
    public static bool sodaAcik = false;
    public static bool dondurmaAcik = false;
    public static bool kahveAcik = false;

    [Header("UI Baloncuk")]
    public GameObject baloncukPanel;
    public TextMeshProUGUI productText;

    // Her ürün için ayrı RawImage
    public RawImage cayImage;
    public RawImage sodaImage;
    public RawImage dondurmaImage;
    public RawImage kahveImage;

    // Otomatik dondurma alma için
    private float urunAlmaAraligi = 0.5f;
    private float dondurmaAlmaTimer = 0f;

    [Header("Sinir Ayarları")]
    public GameObject angryBubble;
    private float angryTimer = 0f;
    public float maxWaitTime = 10f;
    private bool isAngry = false;

    void Start()
    {
        // Ürün tipi belirleme
        if (musteriTipi == MusteriTipi.Dondurma)
        {
            requestedProductType = 2;
        }
        else // MusteriTipi.Normal
        {
            List<int> availableProducts = new List<int> { 0 }; // Çay her zaman var
            if (sodaAcik) availableProducts.Add(1);
            if (kahveAcik) availableProducts.Add(3);

            if (availableProducts.Count > 0)
            {
                int randomIndex = Random.Range(0, availableProducts.Count);
                requestedProductType = availableProducts[randomIndex];
            }
            else requestedProductType = 0;
        }

        istenenUrunSayisi = Random.Range(minUrunSayisi, maxUrunSayisi + 1);

        // Noktaları bulma
        musteriNoktasi = GameObject.FindGameObjectWithTag("MusteriNoktasi")?.transform;
        dondurmaNoktasi1 = GameObject.FindGameObjectWithTag("dondurma1")?.transform;
        dondurmaSatisAlani = GameObject.FindGameObjectWithTag("DondurmaSatisAlani")?.transform;
        dondurmaMusteriSilmeNoktasi = GameObject.FindGameObjectWithTag("DondurmaMusteriSilinmeNoktasi")?.transform;
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi")?.transform;
        musteriFinal = GameObject.FindGameObjectWithTag("MusteriFinal")?.transform;

        animator = GetComponent<Animator>();
        musteriCollider = GetComponent<Collider>();
        if (musteriCollider != null) musteriCollider.enabled = true;

        UpdateUI();

        // Y pozisyonunu sabitle
        transform.position = new Vector3(transform.position.x, musteriYukseklik, transform.position.z);

        // Normal Müşteri: Başlangıç kayması hedefini hesapla
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

    void Update()
    {
        if (isLeaving) return;

        // Sinir zamanlayıcısı
        if (IsAtCounter() && !isAngry && alinanUrunSayisi < istenenUrunSayisi)
        {
            angryTimer += Time.deltaTime;
            if (angryTimer >= maxWaitTime) SetAngryState();
        }
        else angryTimer = 0f;

        // Servis sonrası kısa bekleme (balon kapanmadan ayrılma)
        if (waitingAfterProduct)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= 0.5f)
            {
                waitingAfterProduct = false;
                waitTimer = 0f;
                hasBeenServed = true;

                // Normal müşteri kuyruktan çıkarma
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

        // Dondurma otomatik ürün alma
        if (musteriTipi == MusteriTipi.Dondurma && hasReachedIceCreamCounter && NeedsMoreProducts())
        {
            if (KulahYenileme.Instance != null && KulahYenileme.Instance.mevcutKulahSayisi <= 0)
            {
                if (animator != null) animator.SetBool("isWalking", false);
                return;
            }

            dondurmaAlmaTimer += Time.deltaTime;
            if (dondurmaAlmaTimer >= urunAlmaAraligi)
            {
                ReceiveProduct();
                dondurmaAlmaTimer = 0f;
            }
        }

        // ---------------- HAREKET MANTIĞI ----------------
        Transform hedefNokta = null;
        Vector3 hedefPozisyon = transform.position;

        // 1) Normal Müşteri - Başlangıç Kayması
        if (musteriTipi == MusteriTipi.Normal && !hasDoneInitialShift)
        {
            hedefPozisyon = initialShiftTarget;
            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(initialShiftTarget.x, initialShiftTarget.z);

            if (Vector2.Distance(currentXZ, targetXZ) < 0.1f)
                hasDoneInitialShift = true;
        }
        // 2) Ayrılma Rotası (sinirlenmiş veya servis bitmiş)
        else if (hasBeenServed || isAngry)
        {
            if (musteriTipi == MusteriTipi.Dondurma)
            {
                if (!hasReachedIceCreamDeletionPoint)
                    hedefNokta = dondurmaMusteriSilmeNoktasi; // Adım 3
                else
                    hedefNokta = musteriFinal; // Adım 4 (Yok olma)
            }
            else // Normal Müşteri
            {
                hedefNokta = musteriFinal;
            }
        }
        // 3) Hizmet Rotası
        else if (kuyruktakiSirasi == 0 || musteriTipi == MusteriTipi.Dondurma)
        {
            if (musteriTipi == MusteriTipi.Dondurma)
            {
                if (!hasReachedIceCreamPoint1)
                    hedefNokta = dondurmaNoktasi1;            // Adım 1
                else if (!hasReachedIceCreamCounter)
                    hedefNokta = dondurmaSatisAlani;          // Adım 2
                else
                {
                    // Tezgahta ürün beklerken dur
                    if (animator != null) animator.SetBool("isWalking", false);
                    return;
                }
            }
            else // Normal
            {
                hedefNokta = musteriNoktasi;
            }

            // Tezgâh/Nokta Yakınlık Kontrolü
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
                        else if (hedefNokta == dondurmaMusteriSilmeNoktasi) hasReachedIceCreamDeletionPoint = true;
                    }
                    else
                    {
                        isAtCounter = true;
                    }
                }
            }
        }
        // 4) Sırada Bekleme (Normal)
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

        // Hedef pozisyonu ayarla
        if (hedefNokta != null)
            hedefPozisyon = new Vector3(hedefNokta.position.x, musteriYukseklik, hedefNokta.position.z);

        // Hareketi Gerçekleştir
        HareketEt(hedefPozisyon);

        // Final yok etme kontrolü
        if (hedefNokta == musteriFinal)
        {
            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(musteriFinal.position.x, musteriFinal.position.z);

            if (Vector2.Distance(currentXZ, targetXZ) < 0.1f)
                Destroy(gameObject);
        }
    }

    // ---- Yardımcı: Slot'u hemen serbest bırak ----
    private void FreeServiceSlot()
    {
        if (musteriCollider != null) musteriCollider.enabled = true;

        if (musteriTipi == MusteriTipi.Dondurma)
            dondurmaAlaniDolu = false;
        else
            satisAlaniDolu = false;
    }

    // ---- Sinirlenme akışı (kuyruktan çıkar + slotu boşalt + ayrılma rotası) ----
    private void SetAngryState()
    {
        if (isAngry) return;

        isAngry = true;
        UpdateUI();

        // Kuyruktan hemen çıkar
        if (!kuyruktanCikarildi)
        {
            MusteriSpawner.MusteriAyrildi(this);
            kuyruktanCikarildi = true;
        }

        // Slotu anında serbest bırak (OnTriggerExit beklemeden)
        FreeServiceSlot();

        // Ayrılma rotasını tetikle
        hasBeenServed = true;
        isAtCounter = false;

        if (musteriTipi == MusteriTipi.Dondurma)
        {
            // Silme Noktası -> Final
            hasReachedIceCreamPoint1 = true;
            hasReachedIceCreamCounter = false;
            hasReachedIceCreamDeletionPoint = false; // önce silme noktasına gidecek
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

            Vector3 direction = (targetPositionXZ - currentPosition).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            if (animator != null) animator.SetBool("isWalking", false);

            // Dondurma Silme Noktası varış işareti
            if (musteriTipi == MusteriTipi.Dondurma && !hasReachedIceCreamDeletionPoint && (hasBeenServed || isAngry))
            {
                if (dondurmaMusteriSilmeNoktasi != null &&
                    Vector3.Distance(transform.position, new Vector3(dondurmaMusteriSilmeNoktasi.position.x, musteriYukseklik, dondurmaMusteriSilmeNoktasi.position.z)) < 0.1f)
                {
                    hasReachedIceCreamDeletionPoint = true;
                }
            }
        }
    }

    #region Trigger Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MusteriSatis") && musteriTipi == MusteriTipi.Normal)
        {
            if (!satisAlaniDolu || isAtCounter)
            {
                // Slotu işaretle; collider'ı devre dışı bırakmıyoruz
                satisAlaniDolu = true;
            }
        }
        else if (other.CompareTag("DondurmaSatisAlani") && musteriTipi == MusteriTipi.Dondurma)
        {
            if (!dondurmaAlaniDolu || hasReachedIceCreamCounter)
            {
                dondurmaAlaniDolu = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Çıkarken slotu boşalt (collider durumundan bağımsız)
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

    public bool NeedsMoreProducts() =>
        alinanUrunSayisi < istenenUrunSayisi;

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
            // Normal ürünler için para kazanma mantığı buraya eklenebilir.
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
        if (cayImage != null) cayImage.gameObject.SetActive(false);
        if (sodaImage != null) sodaImage.gameObject.SetActive(false);
        if (dondurmaImage != null) dondurmaImage.gameObject.SetActive(false);
        if (kahveImage != null) kahveImage.gameObject.SetActive(false);
        if (angryBubble != null) angryBubble.gameObject.SetActive(false);

        if (isAngry)
        {
            if (baloncukPanel != null) baloncukPanel.SetActive(true);
            if (angryBubble != null) angryBubble.gameObject.SetActive(true);
            if (productText != null) productText.text = "";
            return;
        }

        int kalan = istenenUrunSayisi - alinanUrunSayisi;
        if (baloncukPanel == null || productText == null) return;

        if (kalan <= 0)
        {
            baloncukPanel.SetActive(false);
            return;
        }

        baloncukPanel.SetActive(true);

        switch (requestedProductType)
        {
            case 0: if (cayImage != null) cayImage.gameObject.SetActive(true); productText.text = "" + kalan; break;
            case 1: if (sodaImage != null) sodaImage.gameObject.SetActive(true); productText.text = "" + kalan; break;
            case 2: if (dondurmaImage != null) dondurmaImage.gameObject.SetActive(true); productText.text = "" + kalan; break;
            case 3: if (kahveImage != null) kahveImage.gameObject.SetActive(true); productText.text = "" + kalan; break;
        }
    }

    private void OnDestroy()
    {
        // Kuyruktan güvenle çıkar (idempotent)
        MusteriSpawner.MusteriAyrildi(this);

        // Slot açık kalsın
        if (musteriTipi == MusteriTipi.Normal) satisAlaniDolu = false;
        else dondurmaAlaniDolu = false;

        if (musteriCollider != null) musteriCollider.enabled = true;
    }
}
