using UnityEngine;
using System.Collections;
using TMPro; // ✅ TextMeshPro kullanımı için

public class MusteriHareket : MonoBehaviour
{
    public float moveSpeed = 10f;
    public int kuyruktakiSirasi;
    public int istenenUrunSayisi;
    public int alinanUrunSayisi = 0;

    private Transform musteriNoktasi;   // Kasadaki nokta
    private Transform spawnPoint;       // Kuyruk başlangıç noktası
    private Transform musteriFinal;     // İş bitince gidilecek yer

    private float takipMesafesi = 6f;
    private Animator animator;
    private float musteriYukseklik = 7.5f;

    private bool isAtCounter = false;
    private bool hasBeenServed = false;
    private bool isLeaving = false;

    // ✅ TextMeshPro referansı
    [Header("UI")]
    public TextMeshProUGUI urunText;

    void Start()
    {
        musteriNoktasi = GameObject.FindGameObjectWithTag("MusteriNoktasi").transform;
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi").transform;
        musteriFinal = GameObject.FindGameObjectWithTag("MusteriFinal").transform;

        animator = GetComponent<Animator>();

        // Rastgele ürün sayısı
        istenenUrunSayisi = 1;

        // ✅ TextMeshPro’ya yazdır
        if (urunText != null)
        {
            urunText.text = istenenUrunSayisi.ToString();
        }

        transform.position = new Vector3(
            transform.position.x,
            musteriYukseklik,
            transform.position.z
        );
    }

    void Update()
    {
        if (isLeaving) return;

        Vector3 hedefPozisyon = transform.position;

        // 1) Kasaya git
        if (kuyruktakiSirasi == 0 && !hasBeenServed)
        {
            hedefPozisyon = new Vector3(
                musteriNoktasi.position.x,
                musteriYukseklik,
                musteriNoktasi.position.z
            );

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
        // 2) İsteği karşılandı → Final noktasına git
        else if (hasBeenServed)
        {
            hedefPozisyon = new Vector3(
                musteriFinal.position.x,
                musteriYukseklik,
                musteriFinal.position.z
            );

            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(musteriFinal.position.x, musteriFinal.position.z);

            if (Vector2.Distance(currentXZ, targetXZ) < 0.1f)
            {
                Debug.Log("Müşteri final noktasına ulaştı ve ayrıldı.");
                Destroy(gameObject);
            }
        }
        // 3) Kuyrukta bekleme
        else
        {
            MusteriHareket[] musteriler = MusteriSpawner.musteriKuyrugu.ToArray();
            if (kuyruktakiSirasi > 0 && kuyruktakiSirasi <= musteriler.Length)
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
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    10f * Time.deltaTime
                );
            }

            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            if (animator != null) animator.SetBool("isWalking", false);
        }
    }

    // Kuyruk/ürün kontrol fonksiyonları
    public bool IsAtCounter()
    {
        return isAtCounter && !hasBeenServed && !isLeaving;
    }

    public bool NeedsMoreProducts()
    {
        return alinanUrunSayisi < istenenUrunSayisi;
    }

    public void ReceiveProduct()
    {
        Debug.Log("ReceiveProduct çağrıldı. Şu an: " + alinanUrunSayisi + "/" + istenenUrunSayisi);

        if (hasBeenServed) return;
        if (alinanUrunSayisi >= istenenUrunSayisi) return;

        alinanUrunSayisi++;
        Debug.Log("Müşteri ürün aldı: " + alinanUrunSayisi + "/" + istenenUrunSayisi);

        // 💰 Para ekle
        MoneyManager.Instance.AddMoney(1);

        // ✅ Text güncelle
        if (urunText != null)
        {
            int kalan = istenenUrunSayisi - alinanUrunSayisi;
            urunText.text = kalan > 0 ? kalan.ToString() : "";
        }

        if (alinanUrunSayisi >= istenenUrunSayisi)
        {
            hasBeenServed = true;
            Debug.Log("Müşteri doydu, MusteriFinal noktasına gidiyor...");

            // Kuyruktan çıkar
            if (MusteriSpawner.musteriKuyrugu.Count > 0 && MusteriSpawner.musteriKuyrugu.Peek() == this)
            {
                MusteriSpawner.musteriKuyrugu.Dequeue();
            }
            MusteriSpawner.UpdateQueuePositions();
        }
    }
}
