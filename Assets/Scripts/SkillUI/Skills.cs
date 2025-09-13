using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Skills : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Button skillButton;
    public RectTransform panel;
    private bool isPanelOpen = false;
    private Vector2 panelClosedPosition;
    private Vector2 panelOpenPosition;
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
        if (panel != null)
        {
            panelClosedPosition = panel.anchoredPosition;
            panelOpenPosition = panelClosedPosition + new Vector2(panel.rect.width, 0);
        }
        else
        {
            Debug.LogError("Panel referansı atanmamış!");
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
        currentButtonRotation += 180f;
        skillButton.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, currentButtonRotation), animationDuration, RotateMode.Fast).SetEase(Ease.InOutQuad);

        if (isPanelOpen)
        {
            panel.DOAnchorPosX(panelClosedPosition.x, animationDuration).SetEase(Ease.InOutQuad);
            isPanelOpen = false;
            PlaySound(closeSound);
        }
        else
        {
            panel.DOAnchorPosX(panelOpenPosition.x, animationDuration).SetEase(Ease.InOutQuad);
            isPanelOpen = true;
            PlaySound(openSound);
        }

        Debug.Log("Panel durumu: " + isPanelOpen);
    }

    public void ToggleSubPanel(GameObject panelToToggle)
    {
        if (!isPanelOpen) return; // Ana panel kapalıysa alt panel açma
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

    // Ses oynatma yardımcı fonksiyonları
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
