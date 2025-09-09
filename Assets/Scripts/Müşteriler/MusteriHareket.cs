using UnityEngine;
using System.Collections;
using TMPro;

public class MusteriHareket : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 10f;
    public float takipMesafesi = 6f;
    private float musteriYukseklik = 7.5f;

    [Header("Sipariş Bilgisi")]
    public int kuyruktakiSirasi;
    public int istenenUrunSayisi = 1;
    public int alinanUrunSayisi = 0;

    // Hangi ürünü istediği (0: çay, 1: soda)
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
    private Transform spawnPoint;
    private Transform musteriFinal;

    private Animator animator;
    private Collider musteriCollider;

    private static bool satisAlaniDolu = false;

    void Start()
    {
        // Rastgele ürün sayısı belirle
        istenenUrunSayisi = Random.Range(minUrunSayisi, maxUrunSayisi + 1);


        requestedProductType = Random.Range(0, 2);

        // Noktaları bul
        musteriNoktasi = GameObject.FindGameObjectWithTag("MusteriNoktasi").transform;
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi").transform;
        musteriFinal = GameObject.FindGameObjectWithTag("MusteriFinal").transform;

        animator = GetComponent<Animator>();
        musteriCollider = GetComponent<Collider>();

        // Collider başlangıçta açık olsun
        if (musteriCollider != null)
            musteriCollider.enabled = true;

        // UI güncelle
        UpdateUI();

        // Y pozisyonunu sabitle
        transform.position = new Vector3(transform.position.x, musteriYukseklik, transform.position.z);

        Debug.Log($"{name} {istenenUrunSayisi} ürün istiyor!");
    }

    // Ürün tipine göre kontrol
    public bool IsRequestingSoda() => requestedProductType == 1;
    public bool IsRequestingTea() => requestedProductType == 0;

    void Update()
    {
        if (isLeaving) return;

        // Ürün aldıktan sonra bekleme süresi
        if (waitingAfterProduct)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= 0.5f)
            {
                waitingAfterProduct = false;
                waitTimer = 0f;
                hasBeenServed = true;

                Debug.Log("Bekleme süresi tamamlandı, MusteriFinal noktasına gidiyor...");

                if (!kuyruktanCikarildi &&
                    MusteriSpawner.musteriKuyrugu.Count > 0 &&
                    MusteriSpawner.musteriKuyrugu.Peek() == this)
                {
                    MusteriSpawner.musteriKuyrugu.Dequeue();
                    kuyruktanCikarildi = true;
                    MusteriSpawner.UpdateQueuePositions();
                }
            }
            return;
        }

        Vector3 hedefPozisyon = transform.position;

        // Kasaya giden müşteri
        if (kuyruktakiSirasi == 0 && !hasBeenServed)
        {
            hedefPozisyon = new Vector3(musteriNoktasi.position.x, musteriYukseklik, musteriNoktasi.position.z);

            float distanceToCounter = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(musteriNoktasi.position.x, 0, musteriNoktasi.position.z)
            );

            if (distanceToCounter < 1.5f && !isAtCounter)
            {
                isAtCounter = true;
                Debug.Log("Müşteri kasaya ulaştı! İstenen ürün: " + istenenUrunSayisi);
            }
        }
        // Ürününü alan müşteri → çıkışa gider
        else if (hasBeenServed)
        {
            hedefPozisyon = new Vector3(musteriFinal.position.x, musteriYukseklik, musteriFinal.position.z);

            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(musteriFinal.position.x, musteriFinal.position.z);

            if (Vector2.Distance(currentXZ, targetXZ) < 0.1f)
            {
                Debug.Log("Müşteri final noktasına ulaştı ve ayrıldı.");
                Destroy(gameObject);
            }
        }
        // Kuyrukta bekleyen müşteriler
        else
        {
            MusteriHareket[] musteriler = MusteriSpawner.musteriKuyrugu.ToArray();
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
        if (other.CompareTag("MusteriSatis"))
        {
            if (!satisAlaniDolu || isAtCounter)
            {
                if (musteriCollider != null)
                {
                    musteriCollider.enabled = false;
                    satisAlaniDolu = true;
                    Debug.Log("Müşteri satış alanına girdi, collider kapatıldı.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MusteriSatis"))
        {
            if (musteriCollider != null && !musteriCollider.enabled)
            {
                musteriCollider.enabled = true;
                satisAlaniDolu = false;
                Debug.Log("Müşteri satış alanından çıktı, collider açıldı.");
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
        if (!CanReceiveProduct())
        {
            Debug.Log($"Müşteri ürün alamaz! hasBeenServed:{hasBeenServed}, alinan:{alinanUrunSayisi}/{istenenUrunSayisi}");
            return;
        }

        alinanUrunSayisi++;
        Debug.Log(name + " ürün aldı -> Toplam: " + alinanUrunSayisi + "/" + istenenUrunSayisi);
        UpdateUI();

        if (alinanUrunSayisi >= istenenUrunSayisi)
        {
            waitingAfterProduct = true;
            waitTimer = 0f;
            Debug.Log("Müşteri doydu, 0.5 saniye bekleniyor...");
        }
    }
    #endregion

    private void UpdateUI()
    {
        if (urunText != null)
        {
            int kalan = istenenUrunSayisi - alinanUrunSayisi;

            // Ürün tipine göre text gösterimi
            string urunAdi = requestedProductType == 0 ? "Çay" : "Soda";
            urunText.text = kalan > 0 ? urunAdi + ": " + kalan.ToString() : "";

            Debug.Log("UpdateUI çağrıldı, kalan: " + kalan + " " + urunAdi);
        }
        else
        {
            Debug.LogWarning("urunText null! UI doğru atanmış mı?");
        }
    }




    private void OnDestroy()
    {
        if (musteriCollider != null && !musteriCollider.enabled)
            satisAlaniDolu = false;
    }
}
