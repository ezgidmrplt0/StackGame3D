using UnityEngine;
using TMPro;

public class Popularity : MonoBehaviour
{
    // Singleton deseni: Diðer scriptlerin kolayca eriþebilmesi için
    public static Popularity Instance;

    public int popularityScore = 0;

    // Popülarite puanýný ekranda göstermek için kullanýlacak UI metin bileþeni.
    [SerializeField] private TextMeshProUGUI popularityText;

    private void Awake()
    {
        // Singleton ayarý
        if (Instance == null)
        {
            Instance = this;
            // Diðer sahnelerde de kalmasýný istiyorsanýz alttaki satýrý kullanýn
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Oyun baþladýðýnda popülarite puanýný 0'a ayarla.
        popularityScore = 0;
        UpdatePopularityUI();
    }

    /// <summary>
    /// Popülarite puanýný belirtilen miktar kadar artýrýr.
    /// </summary>
    /// <param name="amount">Eklenecek popülarite puaný miktarý.</param>
    public void IncreasePopularity(int amount)
    {
        popularityScore += amount;
        UpdatePopularityUI();
        Debug.Log("Popülarite arttý! Yeni puan: " + popularityScore);
    }

    /// <summary>
    /// Popülarite puanýný belirtilen miktar kadar azaltýr.
    /// </summary>
    /// <param name="amount">Çýkarýlacak popülarite puaný miktarý.</param>
    public void DecreasePopularity(int amount)
    {
        popularityScore -= amount;
        UpdatePopularityUI();
        Debug.Log("Popülarite azaldý! Yeni puan: " + popularityScore);
    }

    /// <summary>
    /// Ekranda gösterilen popülarite metnini günceller.
    /// </summary>
    public void UpdatePopularityUI()
    {
        if (popularityText != null)
        {
            popularityText.text = "Popülarite: " + popularityScore.ToString();
        }
    }
}