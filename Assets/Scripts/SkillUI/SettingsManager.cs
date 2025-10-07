using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Settings : MonoBehaviour
{
    public GameObject settingsPanel;

    // 1. Slider (Örn: Müzik Sesi)
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    // 2. Slider (Örn: Efekt Sesi)
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;

    // --- Baþlangýç ve Listener Ekleme ---

    void Start()
    {
        // Slider'lara deðer deðiþtikçe çaðrýlacak metodlarý ekleyin (Listener)
        // musicVolumeSlider.onValueChanged.AddListener() ile deðer deðiþtikçe 
        // UpdateMusicVolumeText metodunu çaðýracaðýz.
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolumeText);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSFXVolumeText);

        // Baþlangýçta Text alanlarýný ilk slider deðerleriyle güncelleyin
        UpdateMusicVolumeText(musicVolumeSlider.value);
        UpdateSFXVolumeText(sfxVolumeSlider.value);
    }

    // --- Text Güncelleme Metotlarý ---

    // Müzik Sesi Slider deðeri deðiþtiðinde çaðrýlýr
    public void UpdateMusicVolumeText(float value)
    {
        int displayValue = Mathf.RoundToInt(value);
        musicVolumeText.text = ""+ displayValue.ToString();
    }

    public void UpdateSFXVolumeText(float value)
    {
        int displayValue = Mathf.RoundToInt(value);
        sfxVolumeText.text = ""+ displayValue.ToString();
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}