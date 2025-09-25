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
    public int kuyruktakiSirasi;
    public int istenenUrunSayisi = 1;
    public int alinanUrunSayisi = 0;
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

    [Header("UI")]
    public TextMeshProUGUI urunText;

    [Header("Noktalar")]
    private Transform musteriNoktasi;
    private Transform dondurmaNoktasi;
    private Transform spawnPoint;
    private Transform musteriFinal;
    private Transform dondurmaSilinmeNoktasi;

    private Animator animator;
    private Collider musteriCollider;

    private static bool satisAlaniDolu = false;
    private static bool dondurmaAlaniDolu = false;
    public static bool sodaAcik = false;
    public static bool dondurmaAcik = false;

    [Header("UI Baloncuk")]
    public GameObject baloncukPanel;
    public TextMeshProUGUI productText;

    // Her ürün için ayrı RawImage
    public RawImage cayImage;
    public RawImage sodaImage;
    public RawImage dondurmaImage;

    // Otomatik dondurma alma için
    private float urunAlmaAraligi = 0.5f;
    private float dondurmaAlmaTimer = 0f;

    [Header("Sinir Ayarları")]
    public GameObject angryBubble;
    private float angryTimer = 0f;
    public float maxWaitTime = 10f; // Müşterinin sinirlenmesi için bekleyeceği süre
    private bool isAngry = false;

    void Start()
    {
        // Ürün tipi belirleme
        if (musteriTipi == MusteriTipi.Dondurma)
        {
            requestedProductType = 2;
        }
        else if (!sodaAcik)
        {
            requestedProductType = 0;
        }
        else
        {
            requestedProductType = Random.Range(0, 2);
        }

        istenenUrunSayisi = Random.Range(minUrunSayisi, maxUrunSayisi + 1);

        // Noktaları bulma
        musteriNoktasi = GameObject.FindGameObjectWithTag("MusteriNoktasi")?.transform;
        dondurmaNoktasi = GameObject.FindGameObjectWithTag("DondurmaSatisAlani")?.transform;
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi")?.transform;
        musteriFinal = GameObject.FindGameObjectWithTag("MusteriFinal")?.transform;
        dondurmaSilinmeNoktasi = GameObject.FindGameObjectWithTag("DondurmaMusteriSilinmeNoktasi")?.transform;

        animator = GetComponent<Animator>();
        musteriCollider = GetComponent<Collider>();

        if (musteriCollider != null)
            musteriCollider.enabled = true;

        UpdateUI();

        // Y pozisyonunu sabitle
        transform.position = new Vector3(transform.position.x, musteriYukseklik, transform.position.z);
    }

    public bool IsRequestingSoda() => requestedProductType == 1;
    public bool IsRequestingTea() => requestedProductType == 0;
    public bool IsRequestingIceCream() => requestedProductType == 2;

    void Update()
    {
        if (isLeaving) return;

        // Müşteri tezgâhta beklerken sinir zamanlayıcısını artır
        if (IsAtCounter() && !isAngry && alinanUrunSayisi < istenenUrunSayisi)
        {
            angryTimer += Time.deltaTime;

            if (angryTimer >= maxWaitTime)
            {
                SetAngryState();
            }
        }
        else
        {
            // Müşteri servis edildiyse veya tezgâhtan ayrıldıysa zamanlayıcıyı sıfırla
            angryTimer = 0f;
        }

        if (waitingAfterProduct)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= 0.5f)
            {
                waitingAfterProduct = false;
                waitTimer = 0f;
                hasBeenServed = true;

                if (!kuyruktanCikarildi)
                {
                    if (musteriTipi == MusteriTipi.Normal &&
                        MusteriSpawner.musteriKuyrugu.Count > 0 &&
                        MusteriSpawner.musteriKuyrugu.Peek() == this)
                    {
                        MusteriSpawner.musteriKuyrugu.Dequeue();
                        MusteriSpawner.UpdateQueuePositions();
                    }
                    else if (musteriTipi == MusteriTipi.Dondurma &&
                             MusteriSpawner.dondurmaMusteriKuyrugu.Count > 0 &&
                             MusteriSpawner.dondurmaMusteriKuyrugu.Peek() == this)
                    {
                        MusteriSpawner.dondurmaMusteriKuyrugu.Dequeue();
                        MusteriSpawner.UpdateDondurmaQueuePositions();
                    }
                    kuyruktanCikarildi = true;
                }
            }
            return;
        }

        // Dondurma müşterisi tezgâhtaysa, daha fazla ürüne ihtiyacı varsa VE KÜLAH VARSA
        if (musteriTipi == MusteriTipi.Dondurma && IsAtCounter() && NeedsMoreProducts())
        {
            // Külah kalmadıysa bekle
            if (KulahYenileme.Instance.mevcutKulahSayisi <= 0)
            {
                // Müşteri külah beklerken yürümeyi durdursun
                if (animator != null)
                    animator.SetBool("isWalking", false);
                return; // İşlemi durdur
            }

            dondurmaAlmaTimer += Time.deltaTime;
            if (dondurmaAlmaTimer >= urunAlmaAraligi)
            {
                ReceiveProduct();
                dondurmaAlmaTimer = 0f;
            }
        }

        Transform hedefNokta = musteriTipi == MusteriTipi.Dondurma ? dondurmaNoktasi : musteriNoktasi;
        if (hedefNokta == null) return;

        Vector3 hedefPozisyon = transform.position;

        if (kuyruktakiSirasi == 0 && !hasBeenServed)
        {
            hedefPozisyon = new Vector3(hedefNokta.position.x, musteriYukseklik, hedefNokta.position.z);

            float distanceToCounter = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(hedefNokta.position.x, 0, hedefNokta.position.z)
            );

            if (distanceToCounter < 1.5f && !isAtCounter)
            {
                isAtCounter = true;
            }
        }
        else if (hasBeenServed || isAngry)
        {
            if (musteriTipi == MusteriTipi.Dondurma && dondurmaSilinmeNoktasi != null)
            {
                hedefPozisyon = new Vector3(dondurmaSilinmeNoktasi.position.x, musteriYukseklik, dondurmaSilinmeNoktasi.position.z);

                Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
                Vector2 targetXZ = new Vector2(dondurmaSilinmeNoktasi.position.x, dondurmaSilinmeNoktasi.position.z);

                if (Vector2.Distance(currentXZ, targetXZ) < 0.1f)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else if (musteriFinal != null)
            {
                hedefPozisyon = new Vector3(musteriFinal.position.x, musteriYukseklik, musteriFinal.position.z);

                Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
                Vector2 targetXZ = new Vector2(musteriFinal.position.x, musteriFinal.position.z);

                if (Vector2.Distance(currentXZ, targetXZ) < 0.1f)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            Queue<MusteriHareket> kuyruk = musteriTipi == MusteriTipi.Normal ?
                MusteriSpawner.musteriKuyrugu : MusteriSpawner.dondurmaMusteriKuyrugu;

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

        HareketEt(hedefPozisyon);
    }

    private void SetAngryState()
    {
        isAngry = true;
        UpdateUI(); // UI'ı sinirli duruma göre güncellemek için çağır
        hasBeenServed = true; // Servis edilme durumunu "tamamlandı" olarak işaretle

        // Müşteriyi kuyruktan çıkar
        if (!kuyruktanCikarildi)
        {
            if (musteriTipi == MusteriTipi.Normal)
            {
                MusteriSpawner.MusteriAyrildi(this);
            }
            else
            {
                MusteriSpawner.MusteriAyrildi(this);
            }
            kuyruktanCikarildi = true;
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

            if (animator != null)
                animator.SetBool("isWalking", true);
        }
        else
        {
            if (animator != null)
                animator.SetBool("isWalking", false);
        }
    }

    #region Trigger Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MusteriSatis") && musteriTipi == MusteriTipi.Normal)
        {
            if (!satisAlaniDolu || isAtCounter)
            {
                if (musteriCollider != null)
                {
                    musteriCollider.enabled = false;
                    satisAlaniDolu = true;
                }
            }
        }
        else if (other.CompareTag("DondurmaSatisAlani") && musteriTipi == MusteriTipi.Dondurma)
        {
            if (!dondurmaAlaniDolu || isAtCounter)
            {
                if (musteriCollider != null)
                {
                    musteriCollider.enabled = false;
                    dondurmaAlaniDolu = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MusteriSatis") && musteriTipi == MusteriTipi.Normal)
        {
            if (musteriCollider != null && !musteriCollider.enabled)
            {
                musteriCollider.enabled = true;
                satisAlaniDolu = false;
            }
        }
        else if (other.CompareTag("DondurmaSatisAlani") && musteriTipi == MusteriTipi.Dondurma)
        {
            if (musteriCollider != null && !musteriCollider.enabled)
            {
                musteriCollider.enabled = true;
                dondurmaAlaniDolu = false;
            }
        }
    }
    #endregion

    #region Ürün Alma
    public bool IsAtCounter() =>
        isAtCounter && !hasBeenServed && !isLeaving && !waitingAfterProduct;

    public bool NeedsMoreProducts() =>
        alinanUrunSayisi < istenenUrunSayisi;

    public bool CanReceiveProduct() =>
        !hasBeenServed && alinanUrunSayisi < istenenUrunSayisi && IsAtCounter();

    public void ReceiveProduct()
    {
        if (!CanReceiveProduct()) return;

        // Dondurma müşterisi sadece külah varsa dondurma alsın
        if (musteriTipi == MusteriTipi.Dondurma)
        {
            // Külah kalmadıysa dondurma alma işlemini durdur
            if (KulahYenileme.Instance.mevcutKulahSayisi <= 0)
            {
                return;
            }

            // Külah kullan ve para kazan
            KulahYenileme.Instance.KulahKullan();
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(10);
            }
        }

        // Alınan ürün sayısını artır
        alinanUrunSayisi++;

        // UI'ı güncelle
        UpdateUI();

        // Tüm ürünleri aldıysa bekleme durumuna geç
        if (alinanUrunSayisi >= istenenUrunSayisi)
        {
            waitingAfterProduct = true;
            waitTimer = 0f;
        }
    }
    #endregion

    private void UpdateUI()
    {
        // Önce hepsini kapat
        if (cayImage != null) cayImage.gameObject.SetActive(false);
        if (sodaImage != null) sodaImage.gameObject.SetActive(false);
        if (dondurmaImage != null) dondurmaImage.gameObject.SetActive(false);
        if (angryBubble != null) angryBubble.gameObject.SetActive(false);

        // Müşteri sinirliyse sadece sinirli balonu göster
        if (isAngry)
        {
            baloncukPanel.SetActive(true);
            if (angryBubble != null) angryBubble.gameObject.SetActive(true);
            productText.text = ""; // Metni boş bırak
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

        // Sadece istenen ürünü aç
        switch (requestedProductType)
        {
            case 0: // Çay
                if (cayImage != null) cayImage.gameObject.SetActive(true);
                productText.text = "" + kalan;
                break;
            case 1: // Soda
                if (sodaImage != null) sodaImage.gameObject.SetActive(true);
                productText.text = "" + kalan;
                break;
            case 2: // Dondurma
                if (dondurmaImage != null) dondurmaImage.gameObject.SetActive(true);
                productText.text = "" + kalan;
                break;
        }
    }


    private void OnDestroy()
    {
        MusteriSpawner.MusteriAyrildi(this);

        if (musteriCollider != null && !musteriCollider.enabled)
        {
            if (musteriTipi == MusteriTipi.Normal)
                satisAlaniDolu = false;
            else
                dondurmaAlaniDolu = false;
        }
    }
}