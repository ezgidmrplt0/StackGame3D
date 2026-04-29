using UnityEngine;
using TMPro;
using System.Collections;

public class KulahYenileme : MonoBehaviour
{
    public static KulahYenileme Instance;

    [Header("Külah Ayarları")]
    [SerializeField] private int maxKulahSayisi = 10;
    public int mevcutKulahSayisi;

    [Header("Yenileme Ayarları")]
    [Tooltip("Oyuncunun yenileme noktasına olan mesafe (yarıçap).")]
    [SerializeField] private float yenilemeMesafesi = 1f;

    [Tooltip("Kaç saniyede bir külah eklensin? (0.1 = saniyede 10 külah)")]
    [SerializeField] private float refillInterval = 0.1f;

    [Tooltip("Oyuncu Transform'u. Boşsa Awake'te Player tag'inden bulunur.")]
    [SerializeField] private Transform oyuncuTransform;

    [Tooltip("Yenileme alanının merkezi. Boşsa bu GameObject'in Transform'u kullanılır.")]
    [SerializeField] private Transform yenilemeNoktasi;

    [Header("UI Ayarları")]
    public TMP_Text kulahText;

    private bool oyuncuAlanaGirdi = false;
    private Coroutine refillCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (oyuncuTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) oyuncuTransform = p.transform;
        }

        if (yenilemeNoktasi == null) yenilemeNoktasi = transform;
        if (kulahText == null) kulahText = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        mevcutKulahSayisi = Mathf.Clamp(maxKulahSayisi, 0, Mathf.Max(0, maxKulahSayisi));
        UpdateUI();
    }

    private void Update()
    {
        if (yenilemeNoktasi == null || oyuncuTransform == null) return;

        // Bazen Y (yükseklik) farkından dolayı mesafe uzak çıkabiliyor, bu yüzden Y eksenini sıfırlıyoruz.
        Vector3 playerPos = oyuncuTransform.position;
        Vector3 targetPos = yenilemeNoktasi.position;
        playerPos.y = 0f;
        targetPos.y = 0f;

        float mesafe = Vector3.Distance(playerPos, targetPos);

        // Ya mesafeden ya da trigger'dan tetiklenmesi için kontrol
        if (mesafe <= yenilemeMesafesi)
        {
            AlanaGir();
        }
        else
        {
            // Eğer trigger'da değilse alandan çık
            if (oyuncuAlanaGirdi && !triggerIcerisinde)
            {
                AlanaCik();
            }
        }
    }

    private bool triggerIcerisinde = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerIcerisinde = true;
            AlanaGir();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerIcerisinde = false;
        }
    }

    private void AlanaGir()
    {
        if (!oyuncuAlanaGirdi)
        {
            oyuncuAlanaGirdi = true;
            StartRefill();
        }
    }

    private void AlanaCik()
    {
        if (oyuncuAlanaGirdi)
        {
            oyuncuAlanaGirdi = false;
            StopRefill();
        }
    }

    private void StartRefill()
    {
        if (refillCoroutine == null)
            refillCoroutine = StartCoroutine(RefillRoutine());
    }

    private void StopRefill()
    {
        if (refillCoroutine != null)
        {
            StopCoroutine(refillCoroutine);
            refillCoroutine = null;
        }
    }

    private IEnumerator RefillRoutine()
    {
        while (oyuncuAlanaGirdi)
        {
            if (mevcutKulahSayisi < maxKulahSayisi)
            {
                mevcutKulahSayisi++;
                UpdateUI();
                yield return new WaitForSeconds(refillInterval);
            }
            else
            {
                // Külahlar maksimumdaysa çıkma, sadece bekle (kullanıldığında tekrar dolsun diye)
                yield return null;
            }
        }
        refillCoroutine = null;
    }

    public void KulahKullan()
    {
        if (mevcutKulahSayisi > 0)
        {
            mevcutKulahSayisi--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (kulahText != null)
            kulahText.text = $"Külah: {mevcutKulahSayisi} / {maxKulahSayisi}";
    }

    private void OnDrawGizmosSelected()
    {
        Transform merkez = yenilemeNoktasi != null ? yenilemeNoktasi : transform;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Gizmos.DrawSphere(merkez.position, Mathf.Max(0.01f, yenilemeMesafesi));
        Gizmos.color = new Color(0f, 0.6f, 1f, 1f);
        Gizmos.DrawWireSphere(merkez.position, Mathf.Max(0.01f, yenilemeMesafesi));
    }
}
