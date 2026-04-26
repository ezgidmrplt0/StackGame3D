using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UrunTasiyici : MonoBehaviour
{
    private enum UrunTuru { Cay, Kahve }

    [Header("Genel Ayarlar")]
    public float speed = 12f;
    public float toplamaAraligi = 0.2f;

    [Tooltip("Bu turda toplanabilecek toplam adet (çay + kahve).")]
    public int toplamKapasite = 10;

    // ------------------ ÇAY (Mevcut Alanlar) ------------------
    [Header("ÇAY Referanslar (MEVCUT)")]
    public GameObject urunPrefab;                 // ÇAY prefab
    public Transform stackRoot;                   // ÇAY stack root
    public float stackSpacing = 0.05f;             // ÇAY item'lar arası ekstra boşluk (boyut.y üzerine eklenir)
    public Transform stackAreaTarget;             // ÇAY drop hedefi

    [Header("ÇAY Çalışma Noktaları")]
    public Transform stackAlmaNoktasi;            // ÇAY alma noktası
    public Transform stackBirakmaNoktasi;         // ÇAY bırakma noktası

    [Header("Ham Çay Kontrolü")]
    public StackCollector stackCollector;         // ÇAY collector
    public int gerekliHamCayMiktari = 1;          // 1 çay ürünü için tüketim

    [Header("ÇAY Ürün Boyutu")]
    public Vector3 urunBoyutu = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("ÇAY Stack Ayarları")]
    public int stackLimit = 5;                    // ÇAY’a özel üst limit
    public float tweenDuration = 0.3f;            // ÇAY scale anim süresi
    public Ease tweenEase = Ease.OutBack;         // ÇAY ease
    public float dropSpacing = 2f;                // ÇAY drop aralığı

    // ------------------ KAHVE (Yeni) ------------------
    [Header("KAHVE Referanslar (YENİ)")]
    public CoffeeStackCollector coffeeCollector;  // KAHVE collector
    public GameObject kahvePrefab;                // KAHVE prefab
    public Transform kahveStackRoot;              // KAHVE stack root
    public Transform kahveDropAreaTarget;         // KAHVE drop hedefi

    [Header("KAHVE Çalışma Noktaları (YENİ)")]
    public Transform kahveAlmaNoktasi;            // KAHVE alma noktası
    public Transform kahveBirakmaNoktasi;         // KAHVE bırakma noktası

    [Header("Ham Kahve Kontrolü (YENİ)")]
    public int gerekliHamKahveMiktari = 1;        // 1 kahve ürünü için tüketim

    [Header("KAHVE Stack Ayarları (YENİ)")]
    public int kahveStackLimit = 5;               // KAHVE’ye özel üst limit
    public float kahveDropSpacing = 2f;           // KAHVE drop aralığı

    // ------------------ Dahili ------------------
    // Aynı turda iki ürünü de toplayacağımız için iki ayrı elde liste tutuyoruz.
    private readonly List<Transform> stackCay = new List<Transform>();
    private readonly List<Transform> stackKahve = new List<Transform>();

    private bool calisiyor = true;
    private bool isInDropArea = false;

    void Update()
    {
        UpdateKahveStackPositions();
    }

    void UpdateKahveStackPositions()
    {
        if (coffeeCollector == null || kahveStackRoot == null) return;

        float stackHeight = coffeeCollector.stackHeight;
        Vector3 targetScale = coffeeCollector.kahveTargetScale;
        float yOffset = targetScale.y * 0.5f;

        for (int i = 0; i < stackKahve.Count; i++)
        {
            if (stackKahve[i] == null) continue;
            stackKahve[i].position = kahveStackRoot.position + Vector3.up * (stackHeight * i + yOffset);
            stackKahve[i].rotation = Quaternion.identity;
        }
    }

    void Start()
    {
        if (stackCollector == null)
            stackCollector = FindObjectOfType<StackCollector>();

        if (coffeeCollector == null)
            coffeeCollector = FindObjectOfType<CoffeeStackCollector>();

        // Hedefler boşsa collector'lardan al
        if (stackAreaTarget == null && stackCollector != null)
            stackAreaTarget = stackCollector.stackAreaTarget;
        if (kahveDropAreaTarget == null && coffeeCollector != null)
            kahveDropAreaTarget = coffeeCollector.dropAreaTarget;

        if (toplamKapasite <= 0)
            toplamKapasite = Mathf.Max(5, stackLimit + kahveStackLimit);

        StartCoroutine(CalismaRutini());
    }

    IEnumerator CalismaRutini()
    {
        while (calisiyor)
        {
            bool cayVar = CayToplanabilir();
            bool kahveVar = KahveToplanabilir();

            if (!cayVar && !kahveVar)
            {
                // hiçbir stok yok → bekle
                yield return new WaitForSeconds(0.75f);
                continue;
            }

            // --- DURUM 1: SADECE ÇAY ---
            if (cayVar && !kahveVar)
            {
                yield return StartCoroutine(Git(stackAlmaNoktasi.position));
                yield return StartCoroutine(ToplaCay());
                if (stackCay.Count > 0 && stackBirakmaNoktasi != null)
                {
                    yield return StartCoroutine(Git(stackBirakmaNoktasi.position));
                    isInDropArea = true; yield return StartCoroutine(DropSequenceCay()); isInDropArea = false;
                }
                continue;
            }

            // --- DURUM 2: SADECE KAHVE ---
            if (!cayVar && kahveVar)
            {
                yield return StartCoroutine(Git(kahveAlmaNoktasi.position));
                yield return StartCoroutine(ToplaKahve());
                if (stackKahve.Count > 0 && kahveBirakmaNoktasi != null)
                {
                    yield return StartCoroutine(Git(kahveBirakmaNoktasi.position));
                    isInDropArea = true; yield return StartCoroutine(DropSequenceKahve()); isInDropArea = false;
                }
                continue;
            }

            // --- DURUM 3: İKİSİ DE VAR → sırayla topla ---
            // ÖNCE ÇAY
            yield return StartCoroutine(Git(stackAlmaNoktasi.position));
            yield return StartCoroutine(ToplaCay());

            // SONRA KAHVE (kapasite kaldıysa)
            if (ToplamAdet() < toplamKapasite)
            {
                yield return StartCoroutine(Git(kahveAlmaNoktasi.position));
                yield return StartCoroutine(ToplaKahve());
            }

            // BIRAK: önce çay sonra kahve
            if (stackCay.Count > 0 && stackBirakmaNoktasi != null)
            {
                yield return StartCoroutine(Git(stackBirakmaNoktasi.position));
                isInDropArea = true; yield return StartCoroutine(DropSequenceCay()); isInDropArea = false;
            }
            if (stackKahve.Count > 0 && kahveBirakmaNoktasi != null)
            {
                yield return StartCoroutine(Git(kahveBirakmaNoktasi.position));
                isInDropArea = true; yield return StartCoroutine(DropSequenceKahve()); isInDropArea = false;
            }
        }
    }

    // ---------- Koşullar ----------
    bool CayToplanabilir()
    {
        return stackAlmaNoktasi && stackCollector && urunPrefab && stackRoot
               && stackCollector.uretimStogu >= gerekliHamCayMiktari
               && stackCay.Count < stackLimit
               && ToplamAdet() < toplamKapasite;
    }

    bool KahveToplanabilir()
    {
        return kahveAlmaNoktasi && coffeeCollector && kahvePrefab && kahveStackRoot
               && coffeeCollector.kahveStogu >= gerekliHamKahveMiktari
               && stackKahve.Count < kahveStackLimit
               && ToplamAdet() < toplamKapasite;
    }

    int ToplamAdet() => stackCay.Count + stackKahve.Count;

    // ---------- TOPLAMA ----------
    IEnumerator ToplaCay()
    {
        while (ToplamAdet() < toplamKapasite && stackCay.Count < stackLimit && stackCollector.uretimStogu >= gerekliHamCayMiktari)
        {
            stackCollector.uretimStogu -= gerekliHamCayMiktari;
            AddUrun(UrunTuru.Cay);
            yield return new WaitForSeconds(toplamaAraligi);
        }
    }

    IEnumerator ToplaKahve()
    {
        while (ToplamAdet() < toplamKapasite && stackKahve.Count < kahveStackLimit && coffeeCollector.kahveStogu >= gerekliHamKahveMiktari)
        {
            coffeeCollector.kahveStogu -= gerekliHamKahveMiktari;
            AddUrun(UrunTuru.Kahve);
            yield return new WaitForSeconds(toplamaAraligi);
        }
    }

    void AddUrun(UrunTuru tur)
    {
        if (tur == UrunTuru.Cay)
        {
            AddCay();
        }
        else
        {
            AddKahve();
        }
    }

    void AddCay()
    {
        if (urunPrefab == null || stackRoot == null) return;

        int index = stackCay.Count;
        float yOffset = urunBoyutu.y * 0.5f;
        float perItem = urunBoyutu.y + stackSpacing;
        Vector3 spawnPos = stackRoot.position + Vector3.up * (perItem * index + yOffset);
        GameObject newObj = Instantiate(urunPrefab, spawnPos, Quaternion.identity, stackRoot);

        if (newObj.TryGetComponent<Rigidbody>(out var rb))
        { rb.isKinematic = true; rb.useGravity = false; rb.drag = 10f; }

        if (newObj.TryGetComponent<Collider>(out var col) && col is BoxCollider box)
            box.size = urunBoyutu * 0.9f;

        Vector3 parentScale = stackRoot.lossyScale;
        Vector3 localBoyut = new Vector3(
            urunBoyutu.x / parentScale.x,
            urunBoyutu.y / parentScale.y,
            urunBoyutu.z / parentScale.z
        );
        newObj.transform.localScale = Vector3.zero;
        newObj.transform.DOScale(localBoyut, tweenDuration).SetEase(tweenEase);

        stackCay.Add(newObj.transform);
    }

    void AddKahve()
    {
        if (kahvePrefab == null || kahveStackRoot == null || coffeeCollector == null) return;

        // Oyuncuyla birebir aynı değerleri kullan
        float stackHeight = coffeeCollector.stackHeight;
        Vector3 targetScale = coffeeCollector.kahveTargetScale;
        Ease ease = coffeeCollector.tweenEase;

        int index = stackKahve.Count;
        float yOffset = targetScale.y * 0.5f;
        Vector3 spawnPos = kahveStackRoot.position + Vector3.up * (stackHeight * index + yOffset);
        GameObject newObj = Instantiate(kahvePrefab, spawnPos, Quaternion.identity, kahveStackRoot);

        foreach (var rb in newObj.GetComponentsInChildren<Rigidbody>(true))
        { rb.isKinematic = true; rb.useGravity = false; }

        foreach (var col in newObj.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        // Oyuncunun stackRoot'undaki world scale'i hesapla, NPC parent'ına göre local scale bul
        Vector3 playerParent = coffeeCollector.stackRoot.lossyScale;
        Vector3 hedefWorldScale = new Vector3(
            targetScale.x * playerParent.x,
            targetScale.y * playerParent.y,
            targetScale.z * playerParent.z
        );
        Vector3 npcParent = kahveStackRoot.lossyScale;
        Vector3 localHedefScale = new Vector3(
            hedefWorldScale.x / npcParent.x,
            hedefWorldScale.y / npcParent.y,
            hedefWorldScale.z / npcParent.z
        );
        newObj.transform.localScale = Vector3.zero;
        newObj.transform.DOScale(localHedefScale, 0.4f).SetEase(ease);

        stackKahve.Add(newObj.transform);
    }

    // ---------- BIRAKMA ----------
    IEnumerator DropSequenceCay()
    {
        while (stackCay.Count > 0 && isInDropArea)
        {
            Transform cube = stackCay[stackCay.Count - 1];
            stackCay.RemoveAt(stackCay.Count - 1);

            cube.SetParent(null);

            int targetIndex = (stackCollector != null && stackCollector.dropList != null)
                ? stackCollector.dropList.Count : 0;
            Transform hedefT = stackAreaTarget;
            if (hedefT == null) yield break;

            Vector3 targetPos = hedefT.position + new Vector3(0f, dropSpacing * targetIndex, 0f);
            cube.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);

            if (stackCollector != null)
            {
                if (stackCollector.dropList == null) stackCollector.dropList = new List<Transform>();
                stackCollector.dropList.Add(cube);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DropSequenceKahve()
    {
        while (stackKahve.Count > 0 && isInDropArea)
        {
            Transform cup = stackKahve[stackKahve.Count - 1];
            stackKahve.RemoveAt(stackKahve.Count - 1);

            cup.SetParent(null);

            int targetIndex = (coffeeCollector != null && coffeeCollector.dropList != null)
                ? coffeeCollector.dropList.Count : 0;
            Transform hedefT = kahveDropAreaTarget;
            if (hedefT == null) yield break;

            Vector3 targetPos = hedefT.position + new Vector3(0f, kahveDropSpacing * targetIndex, 0f);
            cup.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);

            if (coffeeCollector != null)
            {
                if (coffeeCollector.dropList == null) coffeeCollector.dropList = new List<Transform>();
                coffeeCollector.dropList.Add(cup);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    // ---------- HAREKET ----------
    IEnumerator Git(Vector3 hedef)
    {
        while (Vector3.Distance(transform.position, hedef) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);

            Vector3 dir = (hedef - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

            yield return null;
        }
    }

    public void CalismayiBitir()
    {
        calisiyor = false;
        StopAllCoroutines();
        StartCoroutine(UrunleriBirakVeYokOl());
    }

    IEnumerator UrunleriBirakVeYokOl()
    {
        // Önce çayları bırak
        if (stackCay.Count > 0 && stackBirakmaNoktasi != null)
        {
            yield return StartCoroutine(Git(stackBirakmaNoktasi.position));
            isInDropArea = true; yield return StartCoroutine(DropSequenceCay()); isInDropArea = false;
        }
        // Sonra kahveleri bırak
        if (stackKahve.Count > 0 && kahveBirakmaNoktasi != null)
        {
            yield return StartCoroutine(Git(kahveBirakmaNoktasi.position));
            isInDropArea = true; yield return StartCoroutine(DropSequenceKahve()); isInDropArea = false;
        }
        Destroy(gameObject);
    }
}
