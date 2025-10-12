using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini kullandýðýmýzdan emin olun

public class HamKahveToplama : MonoBehaviour
{
    // Kahve prefab'ýný oyuncuya/envantere ekleyeceðimiz için bir referansa ihtiyacýmýz var.
    // Ancak kahveyi ekleme iþini yapan baþka bir script'e (örneðin OyuncuEnvanteri) de ihtiyacýmýz var.
    // Bu kodda, basitçe sadece prefab'ý oluþturup/yok edip, aðacý deaktif etme mantýðýný göstereceðim.

    [Header("Toplanacak Eþya Ayarlarý")]
    public GameObject kahvePrefab; // Oyuncuya eklenecek kahve prefab'ý (Stack için kullanýlacak)

    [Header("Animasyon Ayarlarý")]
    [Tooltip("Orijinal Y ölçeðinin bu katýna kadar küçülsün (0 - 1 arasý). 0.25 = %25")]
    public float minYFactor = 0.1f; // Daha belirgin bir küçülme için 0.1
    public float shrinkDuration = 0.5f; // Küçülme süresi
    public Ease shrinkEase = Ease.OutSine; // Yumuþak bir küçülme
    // Not: "Sarkýntýlý" bir etki isterseniz: Ease.OutElastic veya Ease.InBack deneyebilirsiniz.

    private Vector3 originalScale;
    private Tween activeTween;
    private bool isReadyToCollect = true;

    // Örnek: Stacklama/Envanter sistemi için bir referans
    // public StackManager stackManager; 

    private void Start()
    {
        originalScale = transform.localScale;

        // Kahve prefab'ý atanmadýysa uyarý ver
        if (kahvePrefab == null)
        {
            Debug.LogError("Kahve Prefab'ý KahveToplama script'ine atanmamýþ! Stacklama yapýlamayacak.");
        }

        // Baþlangýçta toplanabilir kahve aðacýný aktif hale getir (Eðer zaten aktif deðilse)
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Toplama iþlemini baþlatan ana fonksiyon.
    /// </summary>
    public void TriggerCollect()
    {
        // Eðer zaten toplanmýþ veya toplanmaya hazýr deðilse iþlem yapma
        if (!isReadyToCollect) return;

        // Aktif animasyon varsa durdur
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        // Objeyi toplandýðý için toplanabilirliðini kapat
        isReadyToCollect = false;

        // 1) Yavaþça küçült (Y ekseninde)
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * Mathf.Clamp01(minYFactor), originalScale.z);

        activeTween = transform.DOScale(targetScale, shrinkDuration).SetEase(shrinkEase).OnComplete(() =>
        {
            // 2) Küçülme animasyonu bitince yapýlmasý gerekenler:

            // A) Kahve objesini oyuncunun envanterine ekle (Stacklama Olayý)
            CollectItemAndStack();

            // B) Aðacý deaktif et
            gameObject.SetActive(false);

            // NOT: Eðer aðacýn "toplanmýþ" versiyonunu aktif etmek istiyorsanýz,
            // burada deaktif etmek yerine "toplanmýþ" versiyonunu aktif etme mantýðý yazýlabilir.
            // Örn: ToplanmýþKahveAgaci.SetActive(true);
        });
    }

    /// <summary>
    /// Kahve objesini oluþturur ve stacklama sistemine ekler.
    /// </summary>
    private void CollectItemAndStack()
    {
        // Gerçek Stacklama Mantýðý buraya yazýlýr.
        // Basitçe:

        if (kahvePrefab != null)
        {
            // Eðer bir "StackManager" script'iniz varsa:
            // stackManager.AddItem(kahvePrefab);

            // Þimdilik sadece bir Debug.Log ile stacklandýðýný varsayalým.
            Debug.Log(gameObject.name + " objesinden Kahve toplandý ve Stack'e eklendi!");

            // Eðer prefab'ý dünyada instantiate edip sonra stack'e taþýyacaksanýz:
            // GameObject collectedCoffee = Instantiate(kahvePrefab, transform.position, Quaternion.identity);
            // collectedCoffee.GetComponent<CoffeeItem>().AddToStack(); // Örnek bir fonksiyon
        }
    }


    // Oyuncunun temasý (Trigger) ile toplama iþlemini baþlatma
    private void OnTriggerEnter(Collider other)
    {
        // Temas eden objenin Tag'ini kontrol edin. (Örnekteki "Depocu" tag'i kullanýldý)
        if (other.CompareTag("Player") || other.CompareTag("Depocu")) // Oyuncu Tag'ýný kontrol edin
        {
            // Toplama iþlemini baþlat
            TriggerCollect();
        }
    }
}