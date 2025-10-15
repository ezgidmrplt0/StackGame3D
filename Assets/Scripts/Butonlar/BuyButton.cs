// BuyButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyButton : MonoBehaviour
{
    public int itemPrice = 100;
    public TextMeshProUGUI priceText;

    // Buton bileþenine ihtiyacýmýz var
    private Button buttonComponent;

    void Awake()
    {
        // Buton bileþenini Start'tan önce al ki, event'e abone olurken kullanabilelim
        buttonComponent = GetComponent<Button>();
    }

    void Start()
    {
        // Fiyatý UI'da göster
        if (priceText != null)
        {
            priceText.text = itemPrice + "$";
        }

        // Butonun týklama olayýna (OnClick) kendi metodumuzu ekle
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(AttemptPurchase);
        }

        // Baþlangýçta butonun durumunu kontrol et
        UpdateInteractability();
    }

    void OnEnable()
    {
        // MoneyManager'daki para deðiþimi olayýna abone ol
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateInteractability;
        }
    }

    void OnDisable()
    {
        // Sahneden ayrýlýrken veya obje kapanýrken aboneliði iptal et (bellek sýzýntýsý olmasýn diye önemli!)
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateInteractability;
        }
    }

    // Butonun etkileþime açýk olup olmadýðýný kontrol eden metot
    private void UpdateInteractability()
    {
        if (buttonComponent == null || MoneyManager.Instance == null) return;

        // Butonun interactable (etkileþime açýk) özelliðini ayarla:
        // Eðer mevcut para (GetCurrentMoney() yerine direkt MoneyManager'dan alalým) 
        // ürünün fiyatýndan büyük veya eþitse, buton etkileþime açýk olsun.
        buttonComponent.interactable = MoneyManager.Instance.money >= itemPrice;

        // EKSTRA: Butonun rengini de deðiþtirebilirsiniz (Yetersiz para için kýrmýzý vb.)
        // if (buttonComponent.interactable)
        // {
        //     buttonComponent.image.color = Color.white;
        // }
        // else
        // {
        //     buttonComponent.image.color = Color.grey;
        // }
    }

    public void AttemptPurchase()
    {
        bool success = MoneyManager.Instance.SpendMoney(itemPrice);

        if (success)
        {
            Debug.Log($"Baþarýyla satýn alýndý! Ürün: {gameObject.name}");
            ExecutePurchaseAction();
            // Satýn alma baþarýlý olursa, para harcanacaðý için OnMoneyChanged zaten tetiklenecek 
            // ve butonu tekrar kilitleme/açma kontrolü yapýlacak.
        }
        else
        {
            Debug.Log("Satýn alma baþarýsýz. Yetersiz bakiye.");
        }
    }

    private void ExecutePurchaseAction()
    {
        // Örnek: Satýn alýndýktan sonra butonu tamamen pasifleþtirip bir daha kullanýlamaz hale getir.
        // buttonComponent.interactable = false;
        // ... buraya istediðiniz diðer özel iþlevleri ekleyebilirsiniz.
    }
}