using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CayToplamaAnim : MonoBehaviour
{
    private Vector3 originalScale;
    private Tween activeTween;

    [Header("Küçülme Ayarları")]
    [Tooltip("Orijinal Y ölçeğinin bu katına kadar küçülsün (0 - 1 arası). 0.25 = %25")]
    public float minYFactor = 0.25f;
    public float shrinkDuration = 0.25f;

    [Header("Geri Dönüş Ayarları")]
    public float waitTime = 5f;          // Küçüldükten sonra bekleme süresi
    public float growDuration = 2f;      // Ne kadar sürede büyüsün
    public Ease growEase = Ease.OutElastic; // Büyürken efekt

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Depocu"))
        {
            TriggerShrink();
        }
    }

    public void TriggerShrink()
    {
        // Önce varsa aktif animasyonu durdur
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        // 1) Yavaşça küçült (Y ekseninde)
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * Mathf.Clamp01(minYFactor), originalScale.z);
        transform.DOScale(targetScale, shrinkDuration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // 2) Bekleme süresinden sonra tekrar büyüt
            activeTween = DOVirtual.DelayedCall(waitTime, () =>
            {
                transform.DOScale(originalScale, growDuration).SetEase(growEase);
            });
        });
    }
}
