using UnityEngine;
using System.Collections;

public class MusteriHareket : MonoBehaviour
{
    public float moveSpeed = 10f;
    public int kuyruktakiSirasi;
    public int istenenUrunSayisi;
    public int alinanUrunSayisi = 0;

    private Transform musteriNoktasi;       // Kasadaki nokta
    private Transform spawnPoint;           // Kuyruk başlangıç noktası
    private Transform musteriDonmeNoktasi;  // İlk dönüş noktası
    private Transform musteriFinal;         // ✅ Yeni: iş bitince gidilecek yer

    private float takipMesafesi = 3f;
    private Animator animator;
    private float musteriYukseklik = 7.5f;

    private bool isAtCounter = false;
    private bool hasBeenServed = false;
    private bool isLeaving = false;
    private bool isGoingToTurnPoint = true;

    void Start()
    {
        musteriNoktasi = GameObject.FindGameObjectWithTag("MusteriNoktasi").transform;
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi").transform;
        musteriDonmeNoktasi = GameObject.FindGameObjectWithTag("MusteriDonmeNoktasi").transform;
        musteriFinal = GameObject.FindGameObjectWithTag("MusteriFinal").transform; // ✅ Final noktası

        animator = GetComponent<Animator>();

        istenenUrunSayisi = Random.Range(1, 9);

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

        // 1) Önce Donme Noktasına git
        if (isGoingToTurnPoint)
        {
            hedefPozisyon = new Vector3(
                musteriDonmeNoktasi.position.x,
                musteriYukseklik,
                musteriDonmeNoktasi.position.z
            );

            if (Vector3.Distance(transform.position, hedefPozisyon) < 0.5f)
            {
                isGoingToTurnPoint = false;
                StartCoroutine(DonVeDevamEt());
            }
        }
        // 2) Kasaya git (sırası gelmişse)
        else if (kuyruktakiSirasi == 0 && !hasBeenServed)
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
        // 3) İsteği karşılandı → MusteriFinal noktasına git
        else if (hasBeenServed)
        {
            hedefPozisyon = new Vector3(
                musteriFinal.position.x,
                musteriYukseklik,
                musteriFinal.position.z
            );

            if (Vector3.Distance(transform.position, musteriFinal.position) < 0.5f)
            {
                Debug.Log("Müşteri final noktasına ulaştı ve ayrıldı.");
                Destroy(gameObject);
            }

        }
        // 4) Kuyrukta bekleme mantığı
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

    IEnumerator DonVeDevamEt()
    {
        // 90 derece sola dön
        Quaternion hedefRotasyon = transform.rotation * Quaternion.Euler(0, -90, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // dönüş hızı
            transform.rotation = Quaternion.Slerp(transform.rotation, hedefRotasyon, t);
            yield return null;
        }

        Debug.Log("Müşteri döndü, kasaya gidiyor...");
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
        alinanUrunSayisi++;
        Debug.Log("Müşteri ürün aldı: " + alinanUrunSayisi + "/" + istenenUrunSayisi);

        // 💰 Her ürün için 1 para ekle
        MoneyManager.Instance.AddMoney(1);

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
