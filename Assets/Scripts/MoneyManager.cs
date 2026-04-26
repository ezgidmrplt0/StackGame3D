using UnityEngine;
using TMPro;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;
    public event Action OnMoneyChanged;

    public int money = 0;
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // DontDestroyOnLoad sadece root objelerde çalışır
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
        OnMoneyChanged?.Invoke();
        Debug.Log($"Para eklendi: {amount}, Toplam: {money}");
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateUI();
            Debug.Log($"Para harcand�: {amount}, Kalan: {money}");
            OnMoneyChanged?.Invoke();
            return true;
        }

        Debug.LogWarning($"Yetersiz bakiye! Gerekli: {amount}, Mevcut: {money}");
        return false;
    }

    public void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = $"{money}$";
    }

    public void SetMoney(int amount = 0)
    {
        money = amount;
        UpdateUI();
        OnMoneyChanged?.Invoke();
    }
}
