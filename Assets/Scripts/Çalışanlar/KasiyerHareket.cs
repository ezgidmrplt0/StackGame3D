using UnityEngine;
using DG.Tweening;
using System.Collections;

public class KasiyerHareket : MonoBehaviour
{
    public Transform satisNoktasi;
    public float hareketHizi = 10f;

    [Header("Animasyon")]
    public Transform modelTransform;

    private bool isAtSalesPoint = false;
    private bool isSelling = false;
    private float lastSellTime = 0f;
    public float sellCooldown = 0.3f;
    private Tween _animTween;
    private Vector3 _origScale;

    void Start()
    {
        _origScale = (modelTransform != null ? modelTransform : transform).localScale;

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

        { Transform at = modelTransform != null ? modelTransform : transform; _origScale = at.localScale; _animTween?.Kill(); _animTween = at.DOScaleY(_origScale.y * 1.06f, 0.18f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine); }
        transform.DOMove(hedefPozisyon, sure)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isAtSalesPoint = true;
                { Transform at = modelTransform != null ? modelTransform : transform; _animTween?.Kill(); at.DOScaleY(_origScale.y, 0.12f); }
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
                    (modelTransform != null ? modelTransform : transform).DOPunchScale(Vector3.one * 0.4f, 0.3f, 5, 0.4f);
                    Debug.Log("Kasiyer satış yaptı!");
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        isSelling = false;
    }

    public bool IsAtSalesPoint() => isAtSalesPoint;
}