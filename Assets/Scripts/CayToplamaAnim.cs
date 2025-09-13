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
    public float growDuration = 5f;      // Ne kadar sürede büyüsün
    public Ease growEase = Ease.OutElastic; // Büyürken efekt

    [Header("Cay Toplama Objeleri")]
    public GameObject cayToplamaNoktasi; // Başlangıçta aktif nesne
    public GameObject cayToplamaKapali;  // Player temasında aktif olacak nesne

    private bool isCollecting = false;

    private void Start()
    {
        if (cayToplamaNoktasi != null)
            originalScale = cayToplamaNoktasi.transform.localScale;

        if (cayToplamaKapali != null)
            cayToplamaKapali.SetActive(false); // Başlangıçta kapalı nesne devre dışı
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Depocu")) && !isCollecting)
        {
            isCollecting = true; // Tekrar tetiklemeyi engelle
            StartCoroutine(CollectCay());
        }
    }

    private IEnumerator CollectCay()
    {
        if (cayToplamaNoktasi != null)
        {
            // Küçülme animasyonu
            Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * Mathf.Clamp01(minYFactor), originalScale.z);
            Tween shrinkTween = cayToplamaNoktasi.transform.DOScale(targetScale, shrinkDuration).SetEase(Ease.InOutSine);

            // Animasyonun bitmesini bekle
            yield return shrinkTween.WaitForCompletion();
        }

        // Objeleri değiştir
        if (cayToplamaNoktasi != null) cayToplamaNoktasi.SetActive(false);
        if (cayToplamaKapali != null) cayToplamaKapali.SetActive(true);

        // waitTime bekle
        yield return new WaitForSeconds(waitTime);

        // Orijinal objeyi tekrar aç ve büyüme animasyonunu uygula
        if (cayToplamaNoktasi != null)
        {
            cayToplamaNoktasi.SetActive(true);
            cayToplamaNoktasi.transform.localScale = new Vector3(originalScale.x, originalScale.y * minYFactor, originalScale.z);
            cayToplamaNoktasi.transform.DOScale(originalScale, growDuration).SetEase(growEase);
        }

        if (cayToplamaKapali != null) cayToplamaKapali.SetActive(false);

        isCollecting = false; // tekrar tetiklemeye izin ver
    }

    public void TriggerShrink()
    {
        // Mevcut animasyonu durdur
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        if (cayToplamaNoktasi != null)
        {
            // Küçülme animasyonu (eski mantık korunuyor)
            Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * Mathf.Clamp01(minYFactor), originalScale.z);
            activeTween = cayToplamaNoktasi.transform.DOScale(targetScale, shrinkDuration).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                // Bekleme süresinden sonra büyüt
                activeTween = DOVirtual.DelayedCall(waitTime, () =>
                {
                    if (cayToplamaNoktasi != null)
                        cayToplamaNoktasi.transform.DOScale(originalScale, growDuration).SetEase(growEase);
                });
            });
        }
    }
}
