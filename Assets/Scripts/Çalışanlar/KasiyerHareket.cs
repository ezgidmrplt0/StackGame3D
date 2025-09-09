using UnityEngine;
using DG.Tweening;
using System.Collections;

public class KasiyerHareket : MonoBehaviour
{
    public Transform satisNoktasi;
    public float hareketHizi = 10f;

    private bool isAtSalesPoint = false;
    private bool isSelling = false;
    private float lastSellTime = 0f;
    public float sellCooldown = 0.3f;

    void Start()
    {
        // Eğer inspector'dan atanmadıysa otomatik bul
        if (satisNoktasi == null)
        {
            GameObject hedefObj = GameObject.FindGameObjectWithTag("SatisNoktasi");
            if (hedefObj != null)
            {
                satisNoktasi = hedefObj.transform;
            }
            else
            {
                Debug.LogError("Sahne içinde 'SatisNoktasi' tag'li bir obje yok!");
                return;
            }
        }

        // Hedef pozisyonu al ve Y eksenini sabitle
        Vector3 hedefPozisyon = satisNoktasi.position;
        hedefPozisyon.y = 2f;

        // Sabit hızla gitmesi için süre hesapla
        float mesafe = Vector3.Distance(transform.position, hedefPozisyon);
        float sure = mesafe / hareketHizi;

        transform.DOMove(hedefPozisyon, sure)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isAtSalesPoint = true;
                Debug.Log("Kasiyer satış noktasına ulaştı!");
            });
    }

    void Update()
    {
        if (isAtSalesPoint && !isSelling && Time.time - lastSellTime > sellCooldown)
        {
            TrySellProducts();
        }
    }

    public void TrySellProducts()
    {
        if (isSelling) return;
        StartCoroutine(SellProducts());
    }

    IEnumerator SellProducts()
    {
        isSelling = true;
        lastSellTime = Time.time;

        if (StackCollector.Instance != null && MusteriSpawner.musteriKuyrugu.Count > 0)
        {
            MusteriHareket customer = MusteriSpawner.musteriKuyrugu.Peek();

            if (customer != null && customer.CanReceiveProduct())
            {
                bool sold = false;

                // Müşteri çay mı soda mı istiyor kontrol et
                if (customer.IsRequestingTea() && StackCollector.Instance.DropCount > 0)
                {
                    sold = StackCollector.Instance.SellProductToCustomer(customer);
                }
                else if (customer.IsRequestingSoda() && SodaStack.Instance != null &&
                         SodaStack.Instance.sodaDropList.Count > 0)
                {
                    sold = StackCollector.Instance.SellSodaToCustomer(customer);
                }

                if (sold)
                {
                    Debug.Log("Kasiyer satış yaptı!");
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        isSelling = false;
    }

    public bool IsAtSalesPoint() => isAtSalesPoint;
}