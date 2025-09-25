using System.Collections;
using UnityEngine;
using TMPro;

public class PaymentPlate : MonoBehaviour
{
    [Header("Ödeme Ayarlarý")]
    public float price = 10f;
    public float paymentSpeed = 1f;
    private float currentPaidAmount = 0;

    [Header("Görsel Ayarlarý")]
    public Transform progressBarFill;
    private Vector3 initialScale;
    private Vector3 initialPosition;

    [Header("UI Ayarlarý")]
    public TextMeshPro priceText;
    public GameObject uiContainer;

    [Header("Tamamlama Ayarlarý")]
    public GameObject objectToActivateOnComplete;
    public bool destroyPlateOnComplete = true;

    private Coroutine paymentCoroutine;
    private bool isPlayerOnPlate = false;

    // Plaka üzerine bir obje girdiðinde çalýþýr
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnPlate = true;

            // Oyuncu alana girdiðinde UI'ý etkinleþtir
            if (uiContainer != null)
            {
                uiContainer.SetActive(true);
            }

            if (paymentCoroutine == null)
            {
                paymentCoroutine = StartCoroutine(PayProcess());
            }
        }
    }

    // Plakadan bir obje çýktýðýnda çalýþýr
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnPlate = false;
            // Buradaki UI'ý devre dýþý býrakma satýrý kaldýrýldý
        }
    }

    // Oyun baþladýðýnda veya etkinleþtirildiðinde
    private void Start()
    {
        // Fiyat metnini baþlangýçta ayarla
        if (priceText != null)
        {
            priceText.text = price.ToString("F0") + "$";
        }

        if (progressBarFill != null)
        {
            // Progress bar'ýn baþlangýç ölçeðini ve pozisyonunu kaydet
            initialScale = progressBarFill.localScale;
            initialPosition = progressBarFill.localPosition;

            // Baþlangýçta progress bar'ý sýfýrla
            progressBarFill.localScale = new Vector3(0, initialScale.y, initialScale.z);
        }

        // Baþlangýçta UI'ý gizle
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
    }

    // Her frame çalýþýr, sürekli kontrol için
    private void Update()
    {
        if (isPlayerOnPlate && paymentCoroutine == null && currentPaidAmount < price)
        {
            paymentCoroutine = StartCoroutine(PayProcess());
        }
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
            float amountToPay = paymentSpeed * Time.deltaTime;

            if (MoneyManager.Instance.money < amountToPay)
            {
                amountToPay = MoneyManager.Instance.money;
            }

            if (amountToPay <= 0)
            {
                yield break;
            }

            MoneyManager.Instance.money -= Mathf.RoundToInt(amountToPay);
            currentPaidAmount += amountToPay;

            float progress = Mathf.Clamp01(currentPaidAmount / price);
            Update3DUI(progress);

            yield return null;
        }

        OnPaymentComplete();
    }

    // 3D progress bar'ý güncelleyen metod
    private void Update3DUI(float progress)
    {
        if (progressBarFill != null)
        {
            float newScaleX = initialScale.x * progress;
            progressBarFill.localScale = new Vector3(newScaleX, initialScale.y, initialScale.z);

            float newPositionX = initialPosition.x - (initialScale.x - newScaleX) / 2;
            progressBarFill.localPosition = new Vector3(newPositionX, initialPosition.y, initialPosition.z);
        }

        if (priceText != null)
        {
            float remainingAmount = price - currentPaidAmount;
            priceText.text = (remainingAmount <= 0) ? "TAMAMLANDI!" : remainingAmount.ToString("F0") + "$";
        }
    }

    // Ödeme tamamlandýðýnda çaðrýlýr
    private void OnPaymentComplete()
    {
        Debug.Log("Ödeme tamamlandý!");

        // Ödeme bitince UI'ý devre dýþý býrak
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }

        if (destroyPlateOnComplete)
        {
            Destroy(gameObject, 0.5f);
        }

        if (objectToActivateOnComplete != null)
        {
            objectToActivateOnComplete.SetActive(true);
        }
    }
}