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

    private void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
        Debug.Log("Para eklendi: " + amount + ", Toplam: " + money);
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateUI();
            Debug.Log("Para harcandý: " + amount + ", Kalan: " + money);
            return true;
        }

        Debug.Log("Yetersiz bakiye! Gerekli: " + amount + " Mevcut: " + money);
        return false;
    }

    public void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = money + "$";
    }
}
