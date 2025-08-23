using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int money = 0;
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
        Debug.Log("Para eklendi: " + amount + ", Toplam: " + money);
    }

    void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "Money: " + money;
    }
}