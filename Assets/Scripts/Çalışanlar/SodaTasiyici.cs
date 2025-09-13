using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SodaTasiyici : MonoBehaviour
{
    [Header("Depocu Prefab ve Spawn Ayarlarý")]
    public GameObject depocuPrefab;      // Sahnedeki GameManager üzerinden sürükleyip atanacak
    public Transform spawnPoint;         // Depocu spawn noktasý

    [Header("Hareket ve Stack Ayarlarý")]
    public float totalDuration = 20f;            // Tüm rota süresi
    public float cubeHeight = 0.005f;            // Soda stack yüksekliði
    public int maxStack = 10;                     // Maksimum soda sayýsý
    public float spawnDelay = 0.4f;              // Soda spawn hýzý
    public Vector3 sodaTargetScale = new Vector3(0.003f, 0.003f, 0.003f);
    public GameObject sodaPrefab;                // Soda prefab

    [Header("Stack Býrakma Ayarý")]
    public float dropSpacing = 0.002f;

    [Header("Stack Ayarý")]
    [SerializeField] private Transform stackRoot; // Inspector'dan atanabilir


    // Buton tetikleme fonksiyonu
    public void SpawnDepocu()
    {
        if (depocuPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Depocu Prefab veya Spawn Point atanmamýþ!");
            return;
        }

        GameObject depocuGO = Instantiate(depocuPrefab, spawnPoint.position, Quaternion.identity);
        DepocuInstance instance = depocuGO.GetComponent<DepocuInstance>();
        if (instance == null)
        {
            instance = depocuGO.AddComponent<DepocuInstance>();
        }

        instance.Initialize(sodaPrefab, totalDuration, maxStack, spawnDelay, cubeHeight, sodaTargetScale, dropSpacing, spawnPoint.position);
    }
}

// Bu script depocu instance’ýný yönetir
public class DepocuInstance : MonoBehaviour
{
    private GameObject sodaPrefab;
    private float totalDuration;
    private int maxStack;
    private float spawnDelay;
    private float cubeHeight;
    private Vector3 sodaTargetScale;
    private float dropSpacing;
    private Vector3 startPos;

    private Transform stackRoot; // Soda stack root

    private List<Transform> sodaStack = new List<Transform>();
    private List<Transform> sodaDropList = new List<Transform>();

    private Vector3 sodaKapiPos;
    private Vector3 sodaYoluPos;
    private Vector3 stackSilmePos;

    public void Initialize(GameObject sodaPrefab, float totalDuration, int maxStack, float spawnDelay, float cubeHeight, Vector3 sodaTargetScale, float dropSpacing, Vector3 startPos)
    {
        this.sodaPrefab = sodaPrefab;
        this.totalDuration = totalDuration;
        this.maxStack = maxStack;
        this.spawnDelay = spawnDelay;
        this.cubeHeight = cubeHeight;
        this.sodaTargetScale = sodaTargetScale;
        this.dropSpacing = dropSpacing;
        this.startPos = startPos;

        sodaKapiPos = GameObject.FindGameObjectWithTag("SodaKapi").transform.position;
        sodaYoluPos = GameObject.FindGameObjectWithTag("SodaYolu").transform.position;
        stackSilmePos = GameObject.FindGameObjectWithTag("StackSilmeNoktasi0").transform.position;

        // Stack root oluþtur
        stackRoot = new GameObject("StackRoot").transform;
        stackRoot.SetParent(transform);
        stackRoot.localPosition = Vector3.zero;

        StartCoroutine(RunDepocuRoutine());
    }

    private IEnumerator RunDepocuRoutine()
    {
        // 1- SodaAlmaNoktasi
        Transform sodaNoktasi = GameObject.FindGameObjectWithTag("SodaAlmaNoktasi").transform;

        // Önce hedefe git ve hareketi bekle
        yield return transform.DOMove(sodaNoktasi.position, totalDuration * 0.2f).SetEase(Ease.Linear).WaitForCompletion();

        // Stackleme
        while (sodaStack.Count < maxStack)
        {
            AddSoda();
            yield return new WaitForSeconds(spawnDelay);
        }

        // 2- SodaKapi
        yield return transform.DOMove(sodaKapiPos, totalDuration * 0.25f).SetEase(Ease.Linear).WaitForCompletion();
        // 3- SodaYolu
        yield return transform.DOMove(sodaYoluPos, totalDuration * 0.25f).SetEase(Ease.Linear).WaitForCompletion();
        // 4- StackSilmeNoktasi0
        yield return transform.DOMove(stackSilmePos, totalDuration * 0.15f).SetEase(Ease.Linear).WaitForCompletion();
        yield return DropSodasRoutine();
        // 5- Baþlangýç noktasýna geri dön
        yield return transform.DOMove(startPos, totalDuration * 0.15f).SetEase(Ease.Linear).WaitForCompletion();

        Destroy(gameObject);
    }

    private void AddSoda()
    {
        // StackRoot altýna instantiate et
        Vector3 spawnPos = stackRoot.position + Vector3.up * (cubeHeight * sodaStack.Count);
        GameObject newSoda = Instantiate(sodaPrefab, spawnPos, Quaternion.identity, stackRoot);
        newSoda.transform.localScale = Vector3.zero;
        newSoda.transform.DOScale(sodaTargetScale, 0.3f).SetEase(Ease.OutCubic);
        sodaStack.Add(newSoda.transform);
    }

    private IEnumerator DropSodasRoutine()
    {
        while (sodaStack.Count > 0)
        {
            Transform soda = sodaStack[sodaStack.Count - 1];
            sodaStack.RemoveAt(sodaStack.Count - 1);
            sodaDropList.Add(soda);
            soda.tag = "SodaProduct";

            Vector3 targetPos = stackSilmePos + Vector3.up * (cubeHeight * sodaDropList.Count);
            soda.DOJump(targetPos, 0.002f, 1, 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                soda.rotation = Quaternion.identity;
            });

            yield return new WaitForSeconds(0.1f);
        }
    }
}
