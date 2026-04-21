using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyButton : MonoBehaviour
{
    public int itemPrice = 100;
    public TextMeshProUGUI priceText;

    private Button buttonComponent;

    void Awake()
    {
        buttonComponent = GetComponent<Button>();
    }

    void Start()
    {
        if (priceText != null)
        {
            priceText.text = itemPrice + "$";
        }

        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(AttemptPurchase);
            buttonComponent.interactable = true;
        }

        UpdateInteractability();
    }

    void OnEnable()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateInteractability;
        }
        if (buttonComponent != null) buttonComponent.interactable = true;
    }

    void OnDisable()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateInteractability;
        }
    }

    private void UpdateInteractability()
    {
        if (buttonComponent == null) return;
        buttonComponent.interactable = true;
    }

    public void AttemptPurchase()
    {
        bool success = MoneyManager.Instance.SpendMoney(itemPrice);

        if (success)
        {
            Debug.Log($"Basariyla satin alindi! Urun: {gameObject.name}");
            ExecutePurchaseAction();
        }
        else
        {
            Debug.Log("Satin alma basarisiz. Yetersiz bakiye.");
        }
    }

    private void ExecutePurchaseAction()
    {
    }
}
