using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    public int basePrice = 100;
    public float priceIncreaseRate = 0.5f;
    private int currentPrice;
    private int currentStep = 0;

    [Header("UI Elemanlarý")]
    public TextMeshProUGUI priceText;
    public List<Button> expandButtons = new List<Button>();

    // ButonManager'a referans
    public ButonManager butonManager;

    private Dictionary<Button, int> buttonStepIndices = new Dictionary<Button, int>();

    void Start()
    {
        currentPrice = basePrice;
        InitializeButtons();
        UpdateUI();
    }

    void InitializeButtons()
    {
        foreach (var button in expandButtons)
        {
            button.onClick.RemoveAllListeners();
        }

        for (int i = 0; i < expandButtons.Count; i++)
        {
            int stepIndex = i;
            buttonStepIndices[expandButtons[i]] = stepIndex;
            expandButtons[i].onClick.AddListener(() => OnExpandButtonClick(stepIndex));
        }
    }

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

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnExpandButtonClick(stepIndex));
    }

    public void OnExpandButtonClick(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= expansionSteps.Count)
        {
            Debug.LogWarning("Geçersiz step index: " + stepIndex);
            return;
        }

        if (stepIndex < currentStep)
        {
            Debug.Log("Bu geniþletme adýmý zaten tamamlandý.");
            return;
        }

        if (stepIndex != currentStep)
        {
            Debug.Log("Önce önceki geniþletme adýmlarýný tamamlamalýsýn. Þu anki step: " + currentStep);
            return;
        }

        if (MoneyManager.Instance.money < currentPrice)
        {
            Debug.Log("Yeterli paran yok! Geniþletme fiyatý: " + currentPrice);
            return;
        }

        MoneyManager.Instance.AddMoney(-currentPrice);

        ExpansionStep step = expansionSteps[stepIndex];

        foreach (GameObject obj in step.objectsToDestroy)
        {
            if (obj != null) Destroy(obj);
        }

        foreach (GameObject obj in step.objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }

        // ButonManager'ý bilgilendir ve ilgili butonu gizle
        if (butonManager != null && stepIndex < expandButtons.Count)
        {
            RectTransform buttonRect = expandButtons[stepIndex].GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                butonManager.HideAndShiftButtons(buttonRect);
            }
        }

        currentStep++;
        currentPrice = Mathf.RoundToInt(currentPrice * (1f + priceIncreaseRate));
        UpdateUI();
    }

    void UpdateUI()
    {
        if (priceText != null)
            priceText.text = currentPrice + "$";

        // Bu döngü, sadece ve sadece mevcut adýma ait butonu týklanabilir yapar.
        // Diðer tüm butonlarý kilitler.
        for (int i = 0; i < expandButtons.Count; i++)
        {
            // Eðer butonun adým indeksi, mevcut adýmýmýzla aynýysa
            if (buttonStepIndices.ContainsKey(expandButtons[i]) && buttonStepIndices[expandButtons[i]] == currentStep)
            {
                expandButtons[i].interactable = true; // Sadece bu butonu aç.
            }
            else
            {
                expandButtons[i].interactable = false; // Diðer hepsini kitle.
            }
        }
    }

    public void AddExpandButton(Button newButton, int stepIndex = -1)
    {
        if (!expandButtons.Contains(newButton))
        {
            expandButtons.Add(newButton);
            if (stepIndex == -1) stepIndex = expandButtons.Count - 1;
            SetButtonStepIndex(newButton, stepIndex);
        }
    }
}