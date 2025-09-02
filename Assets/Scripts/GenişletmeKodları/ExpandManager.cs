using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ExpansionStep
{
    public List<GameObject> objectsToDestroy;
    public List<GameObject> objectsToActivate;
}

public class ExpandManager : MonoBehaviour
{
    [Header("Geniþletme Adýmlarý")]
    public List<ExpansionStep> expansionSteps = new List<ExpansionStep>();

    [Header("Fiyat Ayarlarý")]
    public int basePrice = 100;         // Ýlk fiyat
    public float priceIncreaseRate = 0.5f; // %50 artýþ
    private int currentPrice;
    private int currentStep = 0;

    [Header("UI Elemanlarý")]
    public TextMeshProUGUI priceText;
    public Button expandButton;

    void Start()
    {
        currentPrice = basePrice;
        UpdateUI();

        // Butona týklama event’i baðla
        if (expandButton != null)
            expandButton.onClick.AddListener(OnExpandButtonClick);
    }

    public void OnExpandButtonClick()
    {
        // Tüm adýmlar bitmiþse
        if (currentStep >= expansionSteps.Count)
        {
            Debug.Log("Tüm geniþletme adýmlarý tamamlandý.");
            return;
        }

        // Parayý kontrol et
        if (MoneyManager.Instance.money < currentPrice)
        {
            Debug.Log("Yeterli paran yok! Geniþletme fiyatý: " + currentPrice);
            return;
        }

        // Parayý düþ
        MoneyManager.Instance.AddMoney(-currentPrice);

        // Bu adýmý uygula
        ExpansionStep step = expansionSteps[currentStep];

        foreach (GameObject obj in step.objectsToDestroy)
        {
            if (obj != null) Destroy(obj);
        }

        foreach (GameObject obj in step.objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }

        currentStep++;

        // Sonraki fiyatý %50 artýr
        currentPrice = Mathf.RoundToInt(currentPrice * (1f + priceIncreaseRate));
        UpdateUI();
    }

    void UpdateUI()
    {
        if (priceText != null)
            priceText.text = currentPrice + "$";
    }
}
