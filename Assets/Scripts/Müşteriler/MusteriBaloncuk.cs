using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusteriBaloncuk : MonoBehaviour
{
    [Header("UI Referansları")]
    public Image baloncukArkaPlan;            // Baloncuk arka planı
    public Image cayIkonu;                   // Ürün ikonu
    public Image sodaIkonu;                   // Ürün ikonu
    public TextMeshProUGUI adetText;          // Adet yazısı
    public Image tamamlandiIkonu;             // ✅ Tamamlandı ikonu (başta kapalı olacak)

    [Header("Ürün Sprite'ları")]
    public Sprite caySprite;
    public Sprite sodaSprite;

    [Header("Renkler")]
    public Color normalArkaPlan = Color.white;
    public Color tamamlandiArkaPlan = Color.green;

    [Header("Bağlantılar")]
    public Transform targetCamera;            // Kameraya bakması için

    // Müşterinin siparişi
    private UrunTipi istenenUrun;
    private int istenenAdet;

    void Start()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        // Başlangıçta tamamlandı ikonu kapalı
        if (tamamlandiIkonu != null)
            tamamlandiIkonu.gameObject.SetActive(false);

        if (baloncukArkaPlan != null)
            baloncukArkaPlan.color = normalArkaPlan;
    }

    void LateUpdate()
    {
        if (targetCamera != null)
            transform.LookAt(transform.position + targetCamera.forward);
    }

    public void SetBaloncuk(UrunTipi urun, int adet)
    {
        istenenUrun = urun;
        istenenAdet = adet;

        // Ürün ikonunu değiştir
        switch (urun)
        {
            case UrunTipi.Cay:
                cayIkonu.sprite = caySprite;
                break;
            case UrunTipi.Soda:
                sodaIkonu.sprite = sodaSprite;
                break;
        }

        adetText.text = adet.ToString();

        // Reset arka plan ve ikon
        if (baloncukArkaPlan != null)
            baloncukArkaPlan.color = normalArkaPlan;
        if (tamamlandiIkonu != null)
            tamamlandiIkonu.gameObject.SetActive(false);
    }

    public void UrunVerildi()
    {
        istenenAdet--;

        if (istenenAdet > 0)
        {
            adetText.text = istenenAdet.ToString();
        }
        else
        {
            // Tüm ürünler verildi → baloncuk yeşil + ikon aktif
            if (baloncukArkaPlan != null)
                baloncukArkaPlan.color = tamamlandiArkaPlan;

            adetText.text = ""; // Adet sıfırsa yazıyı temizle

            if (tamamlandiIkonu != null)
                tamamlandiIkonu.gameObject.SetActive(true);
        }
    }
}

public enum UrunTipi
{
    Cay,
    Soda
}
