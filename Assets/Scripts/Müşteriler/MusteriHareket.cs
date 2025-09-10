using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

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
    private bool paraKazanildi = false;

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

        // Dondurma müşterisi tezgaha ulaştığında para kazan
        if (musteriTipi == MusteriTipi.Dondurma && isAtCounter && !paraKazanildi)
        {
            int kazanilanPara = istenenUrunSayisi * 10;
            if (MoneyManager.Instance != null)
                MoneyManager.Instance.AddMoney(kazanilanPara);
            paraKazanildi = true;
            hasBeenServed = true;

            if (!kuyruktanCikarildi && MusteriSpawner.dondurmaMusteriKuyrugu.Count > 0 &&
                MusteriSpawner.dondurmaMusteriKuyrugu.Peek() == this)
            {
                MusteriSpawner.dondurmaMusteriKuyrugu.Dequeue();
                MusteriSpawner.UpdateDondurmaQueuePositions();
                kuyruktanCikarildi = true;
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
        else if (hasBeenServed)
        {
            // Dondurma müşterileri silinme noktasına, normal müşteriler final noktasına gitsin
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
        if (urunText != null)
        {
            int kalan = istenenUrunSayisi - alinanUrunSayisi;
            string urunAdi = "";

            switch (requestedProductType)
            {
                case 0: urunAdi = "Çay"; break;
                case 1: urunAdi = "Soda"; break;
                case 2: urunAdi = "Dondurma"; break;
            }

            urunText.text = kalan > 0 ? urunAdi + ": " + kalan.ToString() : "";
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