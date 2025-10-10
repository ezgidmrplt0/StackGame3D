using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    // Statik değişkenler, oyun içindeki tüm ses/müzik çalarların 
    // erişebilmesi için ayarları tutar.
    public static float SfxVolume = 1f;
    public static float MusicVolume = 1f; // Müzik sesi için statik ayar

    public GameObject settingsPanel;

    // Müzik sesini ayarlayacak olan AudioSource bileşeni
    public AudioSource backgroundMusicSource;

    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;

    void Start()
    {
        // NOT: Slider'larınızın Min=1, Max=10 olduğundan emin olun!

        // Kaydedilen sesleri yükle (veya varsayılanı kullan)
        sfxVolumeSlider.value = SfxVolume * 0f;
        musicVolumeSlider.value = MusicVolume * 0f;

        // Slider'lara fonksiyonları bağla
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged); // 👈 Yeni fonksiyonu bağlıyoruz
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // Başlangıçta sesleri uygula
        OnMusicVolumeChanged(musicVolumeSlider.value); // 👈 Müziğin sesini ayarlıyoruz
        OnSfxVolumeChanged(sfxVolumeSlider.value);
    }

    /// <summary>
    /// Müzik Slider'ı değiştiğinde çağrılır. AudioSource'un sesini ayarlar ve maksimum sesi %40 ile sınırlar.
    /// </summary>
    public void OnMusicVolumeChanged(float sliderValue)
    {
        // Slider değerini (1-10 veya 0-10) Unity'nin beklediği volume aralığına (0-1) çevirir
        float userVolume = sliderValue / 10f;

        // MÜZİK SESİNİ %50 İLE SINIRLA (0.5F İLE ÇARP)
        // Eğer userVolume 1.0 ise, newVolume 0.5 olacak.
        // Eğer userVolume 0.1 ise, newVolume 0.05 olacak.
        float maxVolumeMultiplier = 0.4f; // %40'ye sınırlandırmak için
        float newVolume = userVolume * maxVolumeMultiplier;

        MusicVolume = newVolume;

        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = newVolume;
        }

        // Metin güncellemesini çağır (metin hala 1-10 göstermeye devam eder)
        UpdateVolumeText(musicVolumeText, sliderValue);
    }

    // ... (Diğer kodlar)

    /// <summary>
    /// SFX Slider'ı değiştiğinde çağrılır. SFXVolume statik değişkenini ayarlar.
    /// (Burada SFX çalarların bu statik değişkeni okuyup kendi seslerini ayarlamaları gerekir.)
    /// </summary>
    public void OnSfxVolumeChanged(float sliderValue)
    {
        // Slider değerini (1-10) Unity'nin beklediği volume aralığına (0-1) çevirir
        SfxVolume = sliderValue / 10f;

        // Metin güncellemesini çağır
        UpdateVolumeText(sfxVolumeText, sliderValue);
    }

    /// <summary>
    /// Ortak bir metin güncelleme fonksiyonu.
    /// </summary>
    private void UpdateVolumeText(TextMeshProUGUI volumeText, float sliderValue)
    {
        int displayValue = Mathf.RoundToInt(sliderValue);
        volumeText.text = displayValue.ToString();
    }


    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}