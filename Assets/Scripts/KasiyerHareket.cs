using UnityEngine;
using DG.Tweening;
using System.Collections;

public class KasiyerHareket : MonoBehaviour
{
    public Transform satisNoktasi;
    public float hareketHizi = 2f;

    private bool isAtSalesPoint = false;
    private bool isSelling = false;
    private float lastSellTime = 0f;
    public float sellCooldown = 1f; // 1 saniyede bir satýþ

    void Start()
    {
        if (satisNoktasi != null)
        {
            transform.DOMove(satisNoktasi.position, hareketHizi)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    isAtSalesPoint = true;
                    Debug.Log("Kasiyer satýþ noktasýna ulaþtý!");
                });
        }
    }

    void Update()
    {
        // Satýþ yapma kontrolü
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

        // StackCollector'dan ürün satmaya çalýþ
        if (StackCollector.Instance != null)
        {
            bool sold = StackCollector.Instance.SellProduct();
            if (sold)
            {
                Debug.Log("Kasiyer satýþ yaptý!");
            }
        }

        yield return new WaitForSeconds(0.1f);
        isSelling = false;
    }

    public bool IsAtSalesPoint() => isAtSalesPoint;
}