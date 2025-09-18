using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CayToplamaAnim : MonoBehaviour
{
    private Vector3 originalScale;
    private Tween activeTween;

    public bool isReadyToCollect = true; // Objenin toplanmaya hazır olup olmadığını kontrol eder.

    [Header("Küçülme Ayarları")]
    [Tooltip("Orijinal Y ölçeğinin bu katına kadar küçülsün (0 - 1 arası). 0.25 = %25")]
    public float minYFactor = 0.25f;
    public float shrinkDuration = 0.25f;

    [Header("Geri Dönüş Ayarları")]
    public float waitTime = 5f;
    public float growDuration = 2f;
    public Ease growEase = Ease.OutElastic;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void TriggerShrink()
    {


        // Eğer zaten toplanmış veya toplanmaya hazır değilse işlem yapma
        if (!isReadyToCollect) return;

        // Aktif animasyon varsa durdur
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();
        
        // Objeyi toplandığı için toplanabilirliğini kapat
        isReadyToCollect = false;

        // 1) Yavaşça küçült (Y ekseninde)
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * Mathf.Clamp01(minYFactor), originalScale.z);
        activeTween = transform.DOScale(targetScale, shrinkDuration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // 2) Belirtilen süre kadar bekle ve sonra tekrar büyüt
            activeTween = DOVirtual.DelayedCall(waitTime, () =>
            {
                transform.DOScale(originalScale, growDuration).SetEase(growEase).OnComplete(() =>
                {
                    // Büyüme animasyonu bitince tekrar toplanabilir yap
                    isReadyToCollect = true;
                });
            });
        });
    }

    private void OnTriggerEnter(Collider other)
    {
        // Eğer temas eden objenin tag'i "Depocu" ise...
        if (other.CompareTag("Depocu"))
        {
            // Toplama animasyonunu başlat
            TriggerShrink();
        }
    }
}