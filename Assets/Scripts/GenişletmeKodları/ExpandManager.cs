using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[System.Serializable]
public class ExpansionStep
{
    public List<GameObject> objectsToDestroy;
    public List<GameObject> objectsToActivate;
    public UnityEvent onComplete;
}

public class ExpandManager : MonoBehaviour
{
    [Header("Geni�letme Ad�mlar�")]
    public List<ExpansionStep> expansionSteps = new List<ExpansionStep>();

    [Header("Fiyat Ayarlar�")]
    public int basePrice = 100;
    public float priceIncreaseRate = 0.5f;
    private int currentPrice;
    private int currentStep = 0;

    [Header("UI Elemanlar�")]
    public TextMeshProUGUI priceText;
    public List<Button> expandButtons = new List<Button>();

    // ButonManager'a referans
    public ButonManager butonManager;

    private Dictionary<Button, int> buttonStepIndices = new Dictionary<Button, int>();

    public GameObject gizlen;

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
        gizlen.SetActive(false);

        if (stepIndex < 0 || stepIndex >= expansionSteps.Count)
        {
            Debug.LogWarning("Ge�ersiz step index: " + stepIndex);
            return;
        }

        if (stepIndex < currentStep)
        {
            Debug.Log("Bu geni�letme ad�m� zaten tamamland�.");
            return;
        }

        if (stepIndex != currentStep)
        {
            Debug.Log("�nce �nceki geni�letme ad�mlar�n� tamamlamal�s�n. �u anki step: " + currentStep);
            return;
        }

        if (MoneyManager.Instance.money < currentPrice)
        {
            Debug.Log("Yeterli paran yok! Geni�letme fiyat�: " + currentPrice);
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

        // ButonManager'� bilgilendir ve ilgili butonu gizle
        if (butonManager != null && stepIndex < expandButtons.Count)
        {
            RectTransform buttonRect = expandButtons[stepIndex].GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                butonManager.HideAndShiftButtons(buttonRect);
            }
        }

        step.onComplete?.Invoke();

        currentStep++;
        currentPrice = Mathf.RoundToInt(currentPrice * (1f + priceIncreaseRate));
        UpdateUI();
    }

    void UpdateUI()
    {
        if (priceText != null)
            priceText.text = currentPrice + "$";

        // Bu d�ng�, sadece ve sadece mevcut ad�ma ait butonu t�klanabilir yapar.
        // Di�er t�m butonlar� kilitler.
        for (int i = 0; i < expandButtons.Count; i++)
        {
            // E�er butonun ad�m indeksi, mevcut ad�m�m�zla ayn�ysa
            if (buttonStepIndices.ContainsKey(expandButtons[i]) && buttonStepIndices[expandButtons[i]] == currentStep)
            {
                expandButtons[i].interactable = true; // Sadece bu butonu a�.
            }
            else
            {
                expandButtons[i].interactable = false; // Di�er hepsini kitle.
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