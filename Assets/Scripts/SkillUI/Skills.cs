using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Skills : MonoBehaviour
{
    public Button skillButton; // SkillButton referansı
    public RectTransform panel; // Panel referansı
    private bool isPanelOpen = false; // Panelin açık/kapalı durumu
    private Vector2 panelClosedPosition; // Panelin kapalı olduğu pozisyon
    private Vector2 panelOpenPosition; // Panelin açık olduğu pozisyonu
    private float animationDuration = 0.3f; // Animasyon süresi (saniye)
    private float currentButtonRotation = 0f; // Butonun mevcut rotasyonu

    void Start()
    {
        // SkillButton'a tıklama olayını ekle
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(TogglePanel);
        }

        // Panelin başlangıç ve bitiş pozisyonlarını ayarla
        if (panel != null)
        {
            panelClosedPosition = panel.anchoredPosition; // Panelin başlangıç pozisyonu (kapalı)
            panelOpenPosition = panelClosedPosition + new Vector2(panel.rect.width, 0); // Panelin açık pozisyonu
        }
    }

    void TogglePanel()
    {
        // Butonun rotasyonunu güncelle (her tıklama 180 derece döner)
        currentButtonRotation += 180f;
        skillButton.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, currentButtonRotation), animationDuration, RotateMode.Fast).SetEase(Ease.InOutQuad);

        if (isPanelOpen)
        {
            // Paneli kapat
            panel.DOAnchorPosX(panelClosedPosition.x, animationDuration).SetEase(Ease.InOutQuad);
            isPanelOpen = false;
        }
        else
        {
            // Paneli aç
            panel.DOAnchorPosX(panelOpenPosition.x, animationDuration).SetEase(Ease.InOutQuad);
            isPanelOpen = true;
        }
    }
}