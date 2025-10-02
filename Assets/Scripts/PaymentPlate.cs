using System.Collections;
using UnityEngine;
using TMPro;

public class PaymentPlate : MonoBehaviour
{
    [Header("Ödeme Ayarlarý")]
    public float price = 10f;
    public int paymentUnit = 1;
    public float paymentInterval = 0.2f; // Ödeme birimleri arasýndaki bekleme süresi

    private int currentPaidUnits = 0;
    private int requiredUnits;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnPlate = true;
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnPlate = false;
        }
    }

    private void Start()
    {
        requiredUnits = Mathf.CeilToInt(price / paymentUnit);

        if (priceText != null)
        {
            priceText.text = price.ToString("F0") + "$";
        }

        if (progressBarFill != null)
        {
            initialScale = progressBarFill.localScale;
            initialPosition = progressBarFill.localPosition;
            progressBarFill.localScale = new Vector3(0, initialScale.y, initialScale.z);
        }

        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlayerOnPlate && paymentCoroutine != null)
        {
            StopCoroutine(paymentCoroutine);
            paymentCoroutine = null;
        }
    }

    private IEnumerator PayProcess()
    {
        while (currentPaidUnits < requiredUnits)
        {
            if (MoneyManager.Instance.SpendMoney(paymentUnit))
            {
                currentPaidUnits++;

                float progress = (float)currentPaidUnits / requiredUnits;
                Update3DUI(progress);

                yield return new WaitForSeconds(paymentInterval);
            }
            else
            {
                // Yeterli para yoksa coroutine'i durdur ama UI'ý gizleme
                // Oyuncu para topladýðýnda kaldýðý yerden devam edebilsin
                yield break;
            }

            if (!isPlayerOnPlate)
            {
                yield break;
            }
        }

        OnPaymentComplete();
    }

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
            float remainingAmount = price - (currentPaidUnits * paymentUnit);

            // Fiyat metni ödeme bitene kadar kalan miktarý, bittiðinde ise boþ stringi gösterecek
            priceText.text = (remainingAmount > 0) ? remainingAmount.ToString("F0") + "$" : "";
        }
    }

    private void OnPaymentComplete()
    {
        Debug.Log("Ödeme tamamlandý!");

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