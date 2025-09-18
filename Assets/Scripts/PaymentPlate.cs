using System.Collections;
using UnityEngine;
using TMPro; // TMP_Text için gerekli

public class PaymentPlate : MonoBehaviour
{
    [Header("Ödeme Ayarlarý")]
    public float price = 10f; // Plaka fiyatý
    public float paymentSpeed = 1f; // Saniyede kaç dolar ödenecek
    private float currentPaidAmount = 0;

    [Header("Görsel Ayarlarý")]
    public Renderer plateRenderer;
    public Color initialColor = Color.white;
    public Color finalColor = Color.green;

    [Header("UI Ayarlarý")]
    public TextMeshPro priceText; // Fiyatý gösterecek TextMeshPro objesi

    [Header("Tamamlama Ayarlarý")]
    public GameObject objectToActivateOnComplete; // Ödeme bitince etkinleþecek obje
    public bool destroyPlateOnComplete = true; // Ödeme bitince plakayý yok et

    private Coroutine paymentCoroutine;
    private bool isPlayerOnPlate = false;

    // Plaka üzerine bir obje girdiðinde çalýþýr
    private void OnTriggerEnter(Collider other)
    {
        // Eðer giren objenin "Player" etiketi varsa
        if (other.CompareTag("Player"))
        {
            isPlayerOnPlate = true;
            // Ödeme coroutine'ini baþlat
            if (paymentCoroutine == null)
            {
                paymentCoroutine = StartCoroutine(PayProcess());
            }
        }
    }

    // Plakadan bir obje çýktýðýnda çalýþýr
    private void OnTriggerExit(Collider other)
    {
        // Eðer çýkan objenin "Player" etiketi varsa
        if (other.CompareTag("Player"))
        {
            isPlayerOnPlate = false;
        }
    }

    // Oyun baþladýðýnda veya etkinleþtirildiðinde
    private void Start()
    {
        // Baþlangýçta fiyat metnini ayarla
        if (priceText != null)
        {
            priceText.text = price.ToString("F0") + "$";
        }
    }

    // Her frame çalýþýr, sürekli kontrol için
    private void Update()
    {
        // Eðer oyuncu plakanýn üzerindeyse ve coroutine durdurulmuþsa tekrar baþlat
        if (isPlayerOnPlate && paymentCoroutine == null && currentPaidAmount < price)
        {
            paymentCoroutine = StartCoroutine(PayProcess());
        }
        // Eðer oyuncu plakanýn üzerinde deðilse, coroutine'i durdur
        else if (!isPlayerOnPlate && paymentCoroutine != null)
        {
            StopCoroutine(paymentCoroutine);
            paymentCoroutine = null;
        }
    }

    // Ödeme sürecini adým adým yöneten coroutine
    private IEnumerator PayProcess()
    {
        while (currentPaidAmount < price)
        {
            // Ödenecek miktarý hesapla
            float amountToPay = paymentSpeed * Time.deltaTime;

            // Eðer kalan para bu miktardan azsa, sadece kalan parayý harca
            if (MoneyManager.Instance.money < amountToPay)
            {
                amountToPay = MoneyManager.Instance.money;
            }

            // Eðer para yoksa, iþlemi durdur
            if (amountToPay <= 0)
            {
                yield break; // Coroutine'i durdur
            }

            // Parayý harca ve harcanan miktarý güncelle
            MoneyManager.Instance.money -= Mathf.RoundToInt(amountToPay);
            currentPaidAmount += amountToPay;

            // Plakanýn rengini güncelle
            float progress = Mathf.Clamp01(currentPaidAmount / price);
            UpdatePlateColor(progress);

            // UI'daki fiyatý güncelle
            if (priceText != null)
            {
                priceText.text = (price - currentPaidAmount).ToString("F0") + "$";
            }

            // Bir sonraki frame'i bekle
            yield return null;
        }

        // Ödeme tamamlandýðýnda yapýlacaklar
        OnPaymentComplete();
    }

    // Plakanýn rengini progress bar gibi günceller
    private void UpdatePlateColor(float progress)
    {
        if (plateRenderer != null)
        {
            // Rengi yavaþ yavaþ beyaza doðru kaydýr
            Color newColor = Color.Lerp(initialColor, finalColor, progress);
            plateRenderer.material.color = newColor;
        }
    }

    // Ödeme tamamlandýðýnda çaðrýlýr
    private void OnPaymentComplete()
    {
        Debug.Log("Ödeme tamamlandý!");

        // Fiyat metnini gizle veya güncelle
        if (priceText != null)
        {
            priceText.gameObject.SetActive(false);
        }
        // Plakayý yok et
        if (destroyPlateOnComplete)
        {
            Destroy(gameObject, 0.5f); // Yarým saniye sonra yok et
        }
        // Eðer bir obje etkinleþtirilecekse
        if (objectToActivateOnComplete != null)
        {
            objectToActivateOnComplete.SetActive(true);
        }
    }
}