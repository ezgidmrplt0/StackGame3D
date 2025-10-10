using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StackUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Ayarları")]
    public int basePrice = 100;
    public float priceIncreaseRate = 0.2f;
    private int currentPrice;
    private int stackMaxCount = 5;

    [Header("UI Elemanları")]
    public TextMeshProUGUI priceText;
    public Button upgradeButton;

    void Start()
    {
        currentPrice = basePrice;
        UpdateUI();
        upgradeButton.onClick.AddListener(BuyUpgrade);
    }

    public void BuyUpgrade()
    {
        if (MoneyManager.Instance.money >= currentPrice)
        {
            MoneyManager.Instance.AddMoney(-currentPrice);
            stackMaxCount++;
            StackCollector.Instance.SetStackLimit(stackMaxCount);

            currentPrice = Mathf.RoundToInt(currentPrice * (1f + priceIncreaseRate));
            UpdateUI();
        }
        else
        {
            Debug.Log("Yeterli paran yok!");
        }
    }

    public void UpdateUI()
    {
        if (priceText != null)
            priceText.text = currentPrice + "$";
    }
}
