using UnityEngine;
using TMPro;

public class KulahYenileme : MonoBehaviour
{
    // Singleton deseni ile diðer scriptlerden kolayca eriþim saðlarýz.
    public static KulahYenileme Instance;

    [Header("Külah Ayarlarý")]
    [SerializeField] private int maxKulahSayisi = 10;
    public int mevcutKulahSayisi;

    [Header("Yenileme Ayarlarý")]
    // Oyuncunun yenileme noktasýna olan mesafesi
    [SerializeField] private float yenilemeMesafesi = 1f;

    // Oyuncunun hareketini kontrol eden ana nesne
    [SerializeField] private Transform oyuncuTransform;

    // Oyuncunun külah yenilemek için duracaðý nokta
    [SerializeField] private Transform yenilemeNoktasi;

    [Header("UI Ayarlarý")]
    public TextMeshPro kulahText;

    private bool oyuncuAlanaGirdi = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mevcutKulahSayisi = maxKulahSayisi;
        UpdateUI();
    }

    void Update()
    {
        // Oyuncunun yenileme noktasýna olan mesafesini kontrol et
        if (yenilemeNoktasi != null && oyuncuTransform != null)
        {
            float mesafe = Vector3.Distance(oyuncuTransform.position, yenilemeNoktasi.position);

            if (mesafe <= yenilemeMesafesi)
            {
                // Eðer yeterince yakýnsa ve daha önce alana girilmediyse
                if (!oyuncuAlanaGirdi)
                {
                    mevcutKulahSayisi = maxKulahSayisi;
                    UpdateUI();
                    oyuncuAlanaGirdi = true;
                }
            }
            else
            {
                // Eðer uzaklaþtýysa durumu sýfýrla
                oyuncuAlanaGirdi = false;
            }
        }
    }

    // Oyuncunun külah kullanmasýný saðlar
    public void KulahKullan()
    {
        if (mevcutKulahSayisi > 0)
        {
            mevcutKulahSayisi--;
            UpdateUI();
        }
    }

    // Külah UI'ýný günceller
    private void UpdateUI()
    {
        if (kulahText != null)
        {
            kulahText.text = "Külah: " + mevcutKulahSayisi + " / " + maxKulahSayisi;
        }
    }
}