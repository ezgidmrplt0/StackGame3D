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
    public float destroyAnimationDuration = 0.5f;

    // Yeni: Text Animasyon Ayarlarý
    [Header("Text Animasyon Ayarlarý")]
    public float textScaleDuration = 0.1f;    // Büyüme ve küçülme süresi
    public float textScaleFactor = 1.05f;      // Ne kadar büyüyeceði
    private Vector3 priceTextInitialScale;    // Text'in baþlangýç boyutu
                                              // ----------------------------

    // Ses Ayarlarý (Önceki düzeltmelerden kalan)
    [Header("Ses Ayarlarý")]
    public AudioSource audioSource;
    public AudioClip paymentSound;
    public float basePitch = 1.0f;
    public float maxPitch = 2.0f;

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
            // Yeni: Text'in baþlangýç boyutunu kaydet
            priceTextInitialScale = priceText.transform.localScale;
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

        if (audioSource != null)
        {
            audioSource.pitch = basePitch;
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
            // MoneyManager.Instance'ýn harcama iþlemini kontrol etmeden önce 
            // oyuncunun hala plakada olup olmadýðýný kontrol etmek önemlidir.
            if (!isPlayerOnPlate)
            {
                yield break;
            }

            if (MoneyManager.Instance.SpendMoney(paymentUnit))
            {
                currentPaidUnits++;
                float progress = (float)currentPaidUnits / requiredUnits;

                // Animasyon ve Ses Çaðrýsý
                AnimatePriceText(); // Yeni: Text animasyonunu çaðýr
                PlayPaymentSound(progress);

                Update3DUI(progress);
                yield return new WaitForSeconds(paymentInterval);
            }
            else
            {
                // Yetersiz bakiye varsa döngüyü kýr
                yield break;
            }
        }
        OnPaymentComplete();
    }

    // Yeni Yardýmcý Metot: Price Text'i Büyüt ve Küçült
    private void AnimatePriceText()
    {
        if (priceText == null) return;

        // Önceki Tween'i durdur (hýzlý ödemelerde karýþýklýðý önler)
        priceText.transform.DOKill();

        // 1. Büyütme Animasyonu
        priceText.transform.DOScale(priceTextInitialScale * textScaleFactor, textScaleDuration)
            // 2. Büyütme bitince hemen geri küçültme animasyonunu baþlat
            .OnComplete(() =>
            {
                priceText.transform.DOScale(priceTextInitialScale, textScaleDuration);
            });
    }

    private void PlayPaymentSound(float progress)
    {
        if (audioSource != null && paymentSound != null)
        {
            // ÖNEMLÝ EKLEME: Volume'ü Settings script'inden alýyoruz
            // Eðer Settings.SfxVolume statik deðiþkeni yoksa (Örn: sahneye Settings objesi eklenmediyse) 1f kullan.
            float volume = Settings.SfxVolume;

            // 1. Volume'ü Ayarla
            audioSource.volume = volume;

            // 2. Pitch'i Hesapla ve Ayarla
            float currentPitch = Mathf.Lerp(basePitch, maxPitch, progress);
            audioSource.pitch = currentPitch;

            // 3. Sesi Çal
            audioSource.PlayOneShot(paymentSound);
        }
    }
    // ... (Kalan metodlar: Update3DUI, OnPaymentComplete, ActivateAndAnimate, AnimateAndDestroy ayný kalýr)

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

        if (audioSource != null)
        {
            audioSource.pitch = basePitch;
        }

        ActivateAndAnimate(objectToActivateOnComplete);
        ActivateAndAnimate(extraObjectToActivate1);
        ActivateAndAnimate(extraObjectToActivate2);

        if (destroyPlateOnComplete)
        {
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

    private void AnimateAndDestroy()
    {
        transform.DOScale(Vector3.zero, destroyAnimationDuration)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
}