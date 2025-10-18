using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class KahveTasiyicisiNPC : MonoBehaviour
{
    [Header("Süre Ayarları")]
    public float calismaSuresi = 20f;
    public float hareketHizi = 6f;         // Daha hızlı yürüsün
    public float bekleme = 0.05f;          // Toplama arası bekleme az

    [Header("Stack")]
    public Transform stackRoot;
    public GameObject kahveCekirdegiPrefab;
    public int stackLimit = 10;
    public Vector3 kahveTargetScale = new Vector3(0.01f, 0.01f, 0.01f);
    public float stackHeight = 0.35f;
    public Ease tweenEase = Ease.OutBack;
    public float dropInterval = 0.03f;

    [Header("Toplama Ayarları")]
    public string agacTag = "KahveToplamaNoktasi";
    public string birakmaNoktaTag = "KahveBirakmaNoktasi";

    [Header("Ağaç Sallanma")]
    public float shakeDuration = 0.05f;     // Daha kısa
    public Vector3 shakeStrength = new Vector3(5f, 0f, 5f);
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;

    private readonly List<Transform> stack = new List<Transform>();
    private Tween hareketTween;
    private bool isWorking;

    private Transform birakmaNoktasi;
    private List<Transform> kahveAgaclari = new List<Transform>();
    private int aktifAgacIndex = 0;
    private Vector3 spawnPoint;

    void Awake()
    {
        spawnPoint = transform.position;

        birakmaNoktasi = FindNearestTaggedObject(birakmaNoktaTag);

        GameObject[] agaclar = GameObject.FindGameObjectsWithTag(agacTag);
        foreach (var agac in agaclar)
            kahveAgaclari.Add(agac.transform);

        if (kahveAgaclari.Count == 0)
            Debug.LogError("Hiç KahveToplamaNoktasi bulunamadı!");
        if (birakmaNoktasi == null)
            Debug.LogError("Bırakma noktası bulunamadı!");
    }

    Transform FindNearestTaggedObject(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs.Length == 0) return null;

        Transform nearest = objs[0].transform;
        float minDist = Vector3.Distance(transform.position, nearest.position);

        foreach (var o in objs)
        {
            float d = Vector3.Distance(transform.position, o.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = o.transform;
            }
        }
        return nearest;
    }

    public void ActivateNPC()
    {
        if (isWorking) return;
        StartCoroutine(WorkLoop());
    }

    private IEnumerator WorkLoop()
    {
        isWorking = true;
        float endTime = Time.time + calismaSuresi;

        while (Time.time < endTime)
        {
            // --- 1) Bütün ağaçlardan sırayla 1’er kahve topla ---
            while (stack.Count < stackLimit)
            {
                Transform hedefAgac = kahveAgaclari[aktifAgacIndex];
                aktifAgacIndex = (aktifAgacIndex + 1) % kahveAgaclari.Count;

                if (hedefAgac != null && hedefAgac.gameObject.activeInHierarchy)
                {
                    yield return MoveTo(hedefAgac.position);
                    yield return CollectOneTree(hedefAgac.gameObject);
                    yield return new WaitForSeconds(0.05f);
                }
                else
                {
                    yield return null;
                }
            }

            // --- 2) Bırakma noktasına git ---
            yield return MoveTo(birakmaNoktasi.position);

            // --- 3) Stack'i bırak ---
            if (stack.Count > 0)
                yield return DropSequence();
        }

        // Süre bitti → varsa kahveleri bırak, sonra spawn noktasına dön
        if (stack.Count > 0)
        {
            yield return MoveTo(birakmaNoktasi.position);
            yield return DropSequence();
        }

        yield return MoveTo(spawnPoint);

        isWorking = false;
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 hedef)
    {
        float mesafe = Vector3.Distance(transform.position, hedef);
        float sure = Mathf.Max(0.05f, mesafe / Mathf.Max(0.1f, hareketHizi));

        hareketTween?.Kill();
        hareketTween = transform.DOMove(hedef, sure).SetEase(Ease.Linear);
        yield return hareketTween.WaitForCompletion();
    }

    private IEnumerator CollectOneTree(GameObject agac)
    {
        if (agac == null) yield break;

        Tween s = agac.transform.DOShakeRotation(
            shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, true
        );
        yield return s.WaitForCompletion();

        agac.SetActive(false);
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.StartRespawn(agac, 5f);

        AddOneKahveToStack();
    }

    private void AddOneKahveToStack()
    {
        if (stack.Count >= stackLimit) return;

        float yOffset = kahveTargetScale.y * 0.5f;
        Vector3 pos = stackRoot.position + Vector3.up * (stackHeight * stack.Count + yOffset);

        GameObject cube = Instantiate(kahveCekirdegiPrefab, pos, Quaternion.identity, stackRoot);
        cube.transform.localRotation = Quaternion.Euler(0, 90, 0);

        if (cube.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        cube.transform.localScale = Vector3.zero;
        cube.transform.DOScale(kahveTargetScale, 0.15f).SetEase(tweenEase);
        stack.Add(cube.transform);
    }

    private IEnumerator DropSequence()
    {
        var wait = new WaitForSeconds(dropInterval);

        while (stack.Count > 0)
        {
            Transform t = stack[^1];
            stack.RemoveAt(stack.Count - 1);

            t.DOScale(Vector3.zero, dropInterval * 0.8f)
             .SetEase(Ease.InQuad)
             .OnComplete(() => Destroy(t.gameObject));

            if (CoffeeStackCollector.Instance != null)
            {
                CoffeeStackCollector.Instance.kahveStogu++;
                CoffeeStackCollector.Instance.hamKahve--;
            }

            if (MoneyManager.Instance != null)
                MoneyManager.Instance.AddMoney(5);

            yield return wait;
        }

        if (CoffeeStackCollector.Instance != null)
            CoffeeStackCollector.Instance.SendMessage("GuncelleUI", SendMessageOptions.DontRequireReceiver);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
