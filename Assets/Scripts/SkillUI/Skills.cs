using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Skills : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Button skillButton;
    public RectTransform mainPanel; // Panelin adını mainPanel olarak değiştirdim
    public RectTransform settingsPanel; // Yeni: Ayarlar paneli için RectTransform
    private bool isPanelOpen = false;
    private Vector2 mainPanelClosedPosition;
    private Vector2 mainPanelOpenPosition;
    private Vector2 settingsPanelClosedPosition; // Yeni: Ayarlar panelinin kapalı konumu
    private Vector2 settingsPanelOpenPosition; // Yeni: Ayarlar panelinin açık konumu
    private float animationDuration = 0.3f;
    private float currentButtonRotation = 0f;

    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;
    public Button button1;
    public Button button2;
    public Button button3;

    [Header("Sesler")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip buttonSound;

    void Start()
    {
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(TogglePanel);
        }

        if (mainPanel != null)
        {
            // Ana panelin kapalı pozisyonunu mevcut konum olarak ayarla
            mainPanelClosedPosition = mainPanel.anchoredPosition;

            // Ana panelin açık pozisyonunu hesapla (sağa doğru kayacak)
            mainPanelOpenPosition = new Vector2(mainPanelClosedPosition.x + 262f, mainPanelClosedPosition.y);
        }
        else
        {
            Debug.LogError("Ana panel referansı atanmamış!");
        }

        // Ayarlar paneli için başlangıç pozisyonlarını ayarla
        if (settingsPanel != null)
        {
            // Kapalı pozisyon: Ana panelin sağında, görünmeyecek şekilde
            settingsPanelClosedPosition = settingsPanel.anchoredPosition;

            // Açık pozisyon: Ana panel açıldığında görüneceği konum
            settingsPanelOpenPosition = new Vector2(settingsPanelClosedPosition.x + 313f, settingsPanelClosedPosition.y); // Bu değeri UI'nıza göre ayarlayın
        }
        else
        {
            Debug.LogWarning("Ayarlar paneli referansı atanmamış. Fonksiyon çalışmayacak.");
        }


        // Yeni butonlara tıklama olaylarını ekle
        if (button1 != null)
        {
            button1.onClick.AddListener(() => { ToggleSubPanel(panel1); PlayButtonSound(); });
        }
        if (button2 != null)
        {
            button2.onClick.AddListener(() => { ToggleSubPanel(panel2); PlayButtonSound(); });
        }
        if (button3 != null)
        {
            button3.onClick.AddListener(() => { ToggleSubPanel(panel3); PlayButtonSound(); });
        }

        // Başlangıçta tüm alt paneller kapalı
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(false);
        if (panel3 != null) panel3.SetActive(false);
    }

    void TogglePanel()
    {
        // Butonun rotasyonunu animasyonla değiştir
        currentButtonRotation += 180f;
        skillButton.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, currentButtonRotation), animationDuration, RotateMode.Fast).SetEase(Ease.InOutQuad);

        // DO Tween Sequence (Sıralı Animasyon) oluştur
        Sequence sequence = DOTween.Sequence();

        if (isPanelOpen)
        {
            // PANELI KAPATMA
            sequence.Append(mainPanel.DOAnchorPosX(mainPanelClosedPosition.x, animationDuration).SetEase(Ease.InOutQuad));
            sequence.Join(settingsPanel.DOAnchorPosX(settingsPanelClosedPosition.x, animationDuration).SetEase(Ease.InOutQuad));

            isPanelOpen = false;
            PlaySound(closeSound);
        }
        else
        {
            // PANELI AÇMA
            sequence.Append(mainPanel.DOAnchorPosX(mainPanelOpenPosition.x, animationDuration).SetEase(Ease.InOutQuad));
            sequence.Append(settingsPanel.DOAnchorPosX(settingsPanelOpenPosition.x, animationDuration).SetEase(Ease.InOutQuad)); // Ana panelden sonra gelsin

            isPanelOpen = true;
            PlaySound(openSound);
        }

        Debug.Log("Panel durumu: " + isPanelOpen);
    }

    public void ToggleSubPanel(GameObject panelToToggle)
    {
        if (!isPanelOpen) return;
        if (panel1 != null) panel1.SetActive(panel1 == panelToToggle);
        if (panel2 != null) panel2.SetActive(panel2 == panelToToggle);
        if (panel3 != null) panel3.SetActive(panel3 == panelToToggle);
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        if (!isPanelOpen) return;
        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);
        if (panelToOpen != null)
            panelToOpen.SetActive(true);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void PlayButtonSound()
    {
        PlaySound(buttonSound);
    }
}