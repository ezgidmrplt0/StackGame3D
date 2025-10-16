using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class KahveTasiyicisiPanel : MonoBehaviour
{
    [Header("UI Ögeleri")]
    public Button satinAlButton;          // Inspector'dan bağla
    public TextMeshProUGUI fiyatText;     // Fiyat yazısı
    public TextMeshProUGUI durumText;     // Durum / Kalan süre

    [Header("Satın Alma Ayarları")]
    public int fiyat = 250;
    public int oyuncuPara = 1000;         // Şimdilik test amaçlı

    [Header("Spawn & NPC Ayarları")]
    public GameObject kahveTasiyicisiPrefab;
    public Transform spawnPoint;
    public Transform toplamaNoktasi;
    public Transform birakmaNoktasi;
    public float calismaSuresi = 20f;

    [Header("Stack Ayarları (NPC’ye aktarılacak)")]
    public Transform npcStackRoot;
    public GameObject kahveCekirdegiPrefab;
    public int stackLimit = 10;
    public Vector3 kahveTargetScale = new Vector3(1, 1, 1);
    public float stackHeight = 0.5f;
    public Ease stackTweenEase = Ease.OutBack;
    public float dropInterval = 0.05f;

    private KahveTasiyicisiNPC aktifNPC;
    private float kalanSure = 0f;

    void Start()
    {
        if (fiyatText) fiyatText.text = fiyat + " $";
        if (durumText) durumText.text = "";

        // Inspector'dan atamasan bile buton çalışsın diye:
        if (satinAlButton != null)
            satinAlButton.onClick.AddListener(SatinAl);
    }

    void Update()
    {
        // Kalan süreyi ekrana yaz
        if (aktifNPC != null && kalanSure > 0f)
        {
            kalanSure -= Time.deltaTime;
            if (durumText)
                durumText.text = "Kalan: " + Mathf.Max(0f, kalanSure).ToString("0.0") + " sn";
        }
    }

    // ✅ Asıl fonksiyon (Inspector OnClick listesinde "SatinAl" olarak görünür)
    public void SatinAl()
    {
        if (aktifNPC != null)
        {
            if (durumText) durumText.text = "Zaten çalışıyor!";
            return;
        }

        if (oyuncuPara < fiyat)
        {
            if (durumText) durumText.text = "Yetersiz bakiye!";
            return;
        }

        oyuncuPara -= fiyat;
        SpawnVeBaslat();
    }

    private void SpawnVeBaslat()
    {
        var go = Instantiate(kahveTasiyicisiPrefab, spawnPoint.position, spawnPoint.rotation);
        aktifNPC = go.GetComponent<KahveTasiyicisiNPC>();

        // Gerekli ayarları NPC'ye aktar
        aktifNPC.stackRoot = npcStackRoot;
        aktifNPC.kahveCekirdegiPrefab = kahveCekirdegiPrefab;
        aktifNPC.stackLimit = stackLimit;
        aktifNPC.kahveTargetScale = kahveTargetScale;
        aktifNPC.stackHeight = stackHeight;
        aktifNPC.tweenEase = stackTweenEase;
        aktifNPC.dropInterval = dropInterval;
        aktifNPC.toplamaNoktasi = toplamaNoktasi;
        aktifNPC.birakmaNoktasi = birakmaNoktasi;

        // Süre başlat
        kalanSure = calismaSuresi;
        if (satinAlButton) satinAlButton.interactable = false;
        if (durumText) durumText.text = "Kalan: " + kalanSure.ToString("0.0") + " sn";

        aktifNPC.StartWork(calismaSuresi, onFinished: () =>
        {
            if (durumText) durumText.text = "Görev tamamlandı.";
            if (satinAlButton) satinAlButton.interactable = true;
            aktifNPC = null;
        });
    }

    // ✅ İSTEDİĞİN "satinal" İSMİ İÇİN AYRI KÖPRÜ METOD
    // Butonun OnClick listesinde "Satinal" olarak görünür ve SatinAl()'ı çağırır.
    public void Satinal()
    {
        SatinAl();
    }
}
