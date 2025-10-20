using UnityEngine;
using TMPro;
using System.Collections;

public class KulahYenileme : MonoBehaviour
{
    public static KulahYenileme Instance;

    [Header("Külah Ayarlarý")]
    [SerializeField] private int maxKulahSayisi = 10;
    public int mevcutKulahSayisi;

    [Header("Yenileme Ayarlarý")]
    [Tooltip("Oyuncunun yenileme noktasýna olan mesafe (yarýçap).")]
    [SerializeField] private float yenilemeMesafesi = 1f;

    [Tooltip("Kaç saniyede bir külah eklensin? (0.1 = saniyede 10 külah)")]
    [SerializeField] private float refillInterval = 0.1f;

    [Tooltip("Oyuncu Transform'u. Boþsa Awake'te Player tag'inden bulunur.")]
    [SerializeField] private Transform oyuncuTransform;

    [Tooltip("Yenileme alanýnýn merkezi. Boþsa bu GameObject'in Transform'u kullanýlýr.")]
    [SerializeField] private Transform yenilemeNoktasi;

    [Header("UI Ayarlarý")]
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

        float mesafe = Vector3.Distance(oyuncuTransform.position, yenilemeNoktasi.position);

        if (mesafe <= yenilemeMesafesi)
        {
            if (!oyuncuAlanaGirdi)
            {
                oyuncuAlanaGirdi = true;
                StartRefill();
            }
        }
        else
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
        while (mevcutKulahSayisi < maxKulahSayisi && oyuncuAlanaGirdi)
        {
            mevcutKulahSayisi++;
            UpdateUI();
            yield return new WaitForSeconds(refillInterval);
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
