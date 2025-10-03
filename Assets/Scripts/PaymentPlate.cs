using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening; // Dotween kütüphanesini eklemeyi unutmayýn!

public class PaymentPlate : MonoBehaviour
{
    [Header("Ödeme Ayarlarý")]
    public float price = 10f;
    public int paymentUnit = 1;
    public float paymentInterval = 0.2f;

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

    [Header("Ek Tamamlama Objeleri")]
    public GameObject extraObjectToActivate1;
    public GameObject extraObjectToActivate2;

    [Header("Animasyon Ayarlarý")]
    public float animationDuration = 0.5f;
    public Ease animationEase = Ease.OutBack;
    public float destroyAnimationDuration = 0.5f; // Yeni: Yok olma animasyonunun süresi

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

        if (objectToActivateOnComplete != null) objectToActivateOnComplete.SetActive(false);
        if (extraObjectToActivate1 != null) extraObjectToActivate1.SetActive(false);
        if (extraObjectToActivate2 != null) extraObjectToActivate2.SetActive(false);
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

        ActivateAndAnimate(objectToActivateOnComplete);
        ActivateAndAnimate(extraObjectToActivate1);
        ActivateAndAnimate(extraObjectToActivate2);

        if (destroyPlateOnComplete)
        {
            // Orijinal satýr yerine animasyonlu yok etme metodunu çaðýrýyoruz
            AnimateAndDestroy();
        }
    }

    private void ActivateAndAnimate(GameObject obj)
    {
        if (obj != null)
        {
            Vector3 originalScale = obj.transform.localScale;
            obj.SetActive(true);
            obj.transform.localScale = Vector3.zero;

            obj.transform.DOScale(originalScale, animationDuration)
                .SetEase(animationEase);
        }
    }

    // Yeni Yardýmcý Metot: Objeyi animasyonla yok eder
    private void AnimateAndDestroy()
    {
        // Ödeme tablasýný küçültme ve saydamlaþtýrma animasyonu
        transform.DOScale(Vector3.zero, destroyAnimationDuration)
            .OnComplete(() =>
            {
                // Animasyon bittiðinde objeyi yok et
                Destroy(gameObject);
            });
    }
}