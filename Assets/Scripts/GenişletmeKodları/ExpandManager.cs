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
    public List<Button> expandButtons = new List<Button>();

    // Her buton için ayrý step index saklayacaðýz
    private Dictionary<Button, int> buttonStepIndices = new Dictionary<Button, int>();

    void Start()
    {
        currentPrice = basePrice;
        UpdateUI();

        // Butonlarý ve step index'lerini baþlat
        InitializeButtons();
    }

    void InitializeButtons()
    {
        // Butonlarý temizle
        foreach (var button in expandButtons)
        {
            button.onClick.RemoveAllListeners();
        }

        // Her butona týklama event'ini baðla ve step index'ini ayarla
        for (int i = 0; i < expandButtons.Count; i++)
        {
            int stepIndex = i; // Her buton için farklý bir step index
            buttonStepIndices[expandButtons[i]] = stepIndex;

            expandButtons[i].onClick.AddListener(() => OnExpandButtonClick(stepIndex));
        }
    }

    // Manuel olarak buton-step eþleþtirmesi yapmak için fonksiyon
    public void SetButtonStepIndex(Button button, int stepIndex)
    {
        if (buttonStepIndices.ContainsKey(button))
        {
            buttonStepIndices[button] = stepIndex;
        }
        else
        {
            buttonStepIndices.Add(button, stepIndex);
        }

        // Butonun listener'ýný güncelle
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnExpandButtonClick(stepIndex));
    }

    public void OnExpandButtonClick(int stepIndex)
    {
        // Geçersiz step index kontrolü
        if (stepIndex < 0 || stepIndex >= expansionSteps.Count)
        {
            Debug.LogWarning("Geçersiz step index: " + stepIndex);
            return;
        }

        // Bu step zaten tamamlanmýþsa
        if (stepIndex < currentStep)
        {
            Debug.Log("Bu geniþletme adýmý zaten tamamlandý.");
            return;
        }

        // Sýradaki step bu deðilse
        if (stepIndex != currentStep)
        {
            Debug.Log("Önce önceki geniþletme adýmlarýný tamamlamalýsýn. Þu anki step: " + currentStep);
            return;
        }

        // Parayý kontrol et
        if (MoneyManager.Instance.money < currentPrice)
        {
            Debug.Log("Yeterli paran yok! Geniþletme fiyatý: " + currentPrice);
            return;
        }

        // Parayý düþür
        MoneyManager.Instance.AddMoney(-currentPrice);

        // Bu adýmý uygula
        ExpansionStep step = expansionSteps[stepIndex];

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

    // Yeni buton eklemek için yardýmcý fonksiyon
    public void AddExpandButton(Button newButton, int stepIndex = -1)
    {
        if (!expandButtons.Contains(newButton))
        {
            expandButtons.Add(newButton);

            // Step index belirtilmemiþse, son step'ten devam et
            if (stepIndex == -1) stepIndex = expandButtons.Count - 1;

            SetButtonStepIndex(newButton, stepIndex);
        }
    }
}