using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class KahveTasiyicisiNPC : MonoBehaviour
{
    [Header("Hareket")]
    public Transform toplamaNoktasi;
    public Transform birakmaNoktasi;
    public float hareketHizi = 4f;            // DOTween ile gidip gelme hızı (m/sn)
    public float bekleme = 0.1f;              // Varışta ufak bekleme

    [Header("Stack")]
    public Transform stackRoot;
    public GameObject kahveCekirdegiPrefab;
    public int stackLimit = 10;
    public Vector3 kahveTargetScale = new Vector3(1, 1, 1);
    public float stackHeight = 0.5f;
    public Ease tweenEase = Ease.OutBack;
    public float dropInterval = 0.05f;

    [Header("Toplama Algılama")]
    public float toplamaYaricapi = 2.5f;      // toplamaNoktasi etrafında ağaç arama
    public string agacTag = "KahveToplamaNoktasi";
    public string birakmaTag = "KahveBirakmaNoktasi";

    // Respawn için HamKahveToplama’daki statik ayarlara uyum
    private float shakeDuration => HamKahveToplama.StaticShakeDuration;
    private Vector3 shakeStrength => HamKahveToplama.StaticShakeStrength;
    private int shakeVibrato => HamKahveToplama.StaticShakeVibrato;
    private float shakeRandomness => HamKahveToplama.StaticShakeRandomness;

    private readonly List<Transform> stack = new List<Transform>();
    private Tween hareketTween;
    private bool isWorking = false;
    private System.Action onFinishedCb;

    // Dışarıdan çağrılır
    public void StartWork(float sure, System.Action onFinished)
    {
        if (isWorking) return;
        isWorking = true;
        onFinishedCb = onFinished;
        StartCoroutine(WorkLoop(sure));
    }

    private IEnumerator WorkLoop(float sure)
    {
        float endTime = Time.time + sure;

        while (Time.time < endTime)
        {
            // 1) Toplama noktasına git
            yield return MoveTo(toplamaNoktasi.position);

            // 2) Stack dolu değilse ağaç topla
            if (stack.Count < stackLimit)
            {
                TryCollectOneTree();
            }

            yield return new WaitForSeconds(bekleme);

            // 3) Bırakma noktasına git
            yield return MoveTo(birakmaNoktasi.position);

            // 4) Stack boşalt
            if (stack.Count > 0)
            {
                yield return DropSequence();
            }

            yield return new WaitForSeconds(bekleme);
        }

        // Süre bitti → spawn noktasına yakınsa kal, değilse en son bulunduğu yerdeyken kapan
        isWorking = false;
        onFinishedCb?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 hedef)
    {
        // Mesafeye göre süre
        float mesafe = Vector3.Distance(transform.position, hedef);
        float sure = Mathf.Max(0.01f, mesafe / Mathf.Max(0.1f, hareketHizi));

        hareketTween?.Kill();
        hareketTween = transform.DOMove(hedef, sure).SetEase(Ease.Linear);
        yield return hareketTween.WaitForCompletion();
    }

    private void TryCollectOneTree()
    {
        // toplamaNoktasi çevresinde aktif bir ağaç bul
        Collider[] cols = Physics.OverlapSphere(toplamaNoktasi.position, toplamaYaricapi);
        GameObject aktifAgac = null;

        foreach (var c in cols)
        {
            if (c.CompareTag(agacTag) && c.gameObject.activeInHierarchy)
            {
                aktifAgac = c.gameObject;
                break;
            }
        }

        if (aktifAgac == null) return;

        // Ağaç sallanır → deaktif → Respawn manager’a verilir → stack’e eklenir
        aktifAgac.transform.DOShakeRotation(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: shakeVibrato,
            randomness: shakeRandomness,
            fadeOut: true
        ).OnComplete(() =>
        {
            aktifAgac.SetActive(false);
            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.StartRespawn(aktifAgac, delay: 5f); // istersen panelden parametre geçebilirsin
            }

            AddOneKahveToStack();

            // Sayaçlar: oyuncuda yaptığınla aynı davranış
            if (CoffeeStackCollector.Instance != null)
            {
                CoffeeStackCollector.Instance.hamKahve++;
            }
        });
    }

    private void AddOneKahveToStack()
    {
        if (stack.Count >= stackLimit) return;

        float yOffset = kahveTargetScale.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (stackHeight * stack.Count + yOffset);

        GameObject newObj = Instantiate(kahveCekirdegiPrefab, spawnPosition, Quaternion.identity, stackRoot);
        newObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        var rb = newObj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        newObj.transform.localScale = Vector3.zero;
        newObj.transform.DOScale(kahveTargetScale, 0.4f).SetEase(tweenEase);

        stack.Add(newObj.transform);
    }

    private IEnumerator DropSequence()
    {
        var wait = new WaitForSeconds(dropInterval);

        while (stack.Count > 0)
        {
            Transform cubeToDrop = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            cubeToDrop.DOScale(Vector3.zero, dropInterval * 0.8f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(cubeToDrop.gameObject));

            if (CoffeeStackCollector.Instance != null)
            {
                CoffeeStackCollector.Instance.kahveStogu++;
                CoffeeStackCollector.Instance.hamKahve--;
            }

            yield return wait;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (toplamaNoktasi != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(toplamaNoktasi.position, toplamaYaricapi);
        }
    }
}
