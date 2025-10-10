using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Skills : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Button skillButton;
    public Button skillButton1;
    public RectTransform mainPanel;
    public RectTransform settingsPanel;
    private bool isPanelOpen = false;
    private Vector2 mainPanelClosedPosition;
    private Vector2 mainPanelOpenPosition;
    private Vector2 settingsPanelClosedPosition;
    private Vector2 settingsPanelOpenPosition;
    private float animationDuration = 0.3f;
    private float currentButtonRotation = 0f;

    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;
    public Button button1;
    public Button button2;
    public Button button3;

    [Header("Ses Kontrol")] // YENİ BAŞLIK
    public Slider volumeSlider; // YENİ: Slider referansı

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
            skillButton1.onClick.AddListener(TogglePanel);
        }

        if (mainPanel != null)
        {
            mainPanelClosedPosition = mainPanel.anchoredPosition;
            mainPanelOpenPosition = new Vector2(mainPanelClosedPosition.x + 262f, mainPanelClosedPosition.y);
        }
        else
        {
            Debug.LogError("Ana panel referansı atanmamış!");
        }

        if (settingsPanel != null)
        {
            settingsPanelClosedPosition = settingsPanel.anchoredPosition;
            settingsPanelOpenPosition = new Vector2(settingsPanelClosedPosition.x + 313f, settingsPanelClosedPosition.y);
        }
        else
        {
            Debug.LogWarning("Ayarlar paneli referansı atanmamış. Fonksiyon çalışmayacak.");
        }

        // YENİ: Volume Slider olayını ekle ve başlangıç değerlerini ayarla
        if (volumeSlider != null)
        {
            float initialVolume = volumeSlider.value;

            // AudioSource volume değeri 0-1 arasındadır, bu yüzden ölçekliyoruz
            if (audioSource != null)
            {
                audioSource.volume = initialVolume / 10f;
            }

            // Slider değeri değiştiğinde ChangeVolume fonksiyonunu çağır
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }
        else
        {
            Debug.LogWarning("Volume Slider referansı atanmamış.");
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

        // Başlangıçta alt panellerin durumu
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
            sequence.Append(settingsPanel.DOAnchorPosX(settingsPanelOpenPosition.x, animationDuration).SetEase(Ease.InOutQuad));

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
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);
        if (panel3 != null) panel3.SetActive(false);
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
    public void ChangeVolume(float sliderValue)
    {
        if (audioSource != null)
        {
            // Slider değeri (0-10) / 10f = AudioSource Volume (0.0-1.0)
            float newVolume = sliderValue / 10f;
            audioSource.volume = newVolume;
        }
    }
}