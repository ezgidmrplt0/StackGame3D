using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SodaStack : MonoBehaviour
{
    [Header("Soda Ayarları")]
    public GameObject sodaPrefab;
    public Transform stackRoot;
    public float cubeHeight = 0.005f;
    public float tweenDuration = 0.3f;
    public Ease tweenEase = Ease.OutCubic;
    public int maxStack = 10;
    public float spawnDelay = 0.4f;

    [Header("Büyütülebilir Ölçek")]
    public Vector3 sodaTargetScale = new Vector3(0.003f, 0.003f, 0.003f);

    [Header("Bırakma Ayarları")]
    public Transform sodaDropTarget;
    public float dropSpacing = 0.002f;

    // Soda listesi (StackCollector dropList'ten bağımsız)
    public List<Transform> sodaStack = new List<Transform>();
    public List<Transform> sodaDropList = new List<Transform>();

    private bool canCollect = false;
    private bool isInDropArea = false;
    private Coroutine collectRoutine;
    private Coroutine dropRoutine;

    // Singleton
    public static SodaStack Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        UpdateStackPositions();
    }

    private void UpdateStackPositions()
    {
        for (int i = 0; i < sodaStack.Count; i++)
        {
            Transform soda = sodaStack[i];
            Vector3 targetPos = stackRoot.position + Vector3.up * cubeHeight * i;
            soda.position = Vector3.Lerp(soda.position, targetPos, Time.deltaTime * 10f);
            soda.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SodaNoktasi"))
        {
            if (!canCollect)
            {
                canCollect = true;
                collectRoutine = StartCoroutine(CollectSodaRoutine());
            }
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = true;
            if (dropRoutine == null)
                dropRoutine = StartCoroutine(DropSodasRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SodaNoktasi"))
        {
            canCollect = false;
            if (collectRoutine != null)
                StopCoroutine(collectRoutine);
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = false;
            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
                dropRoutine = null;
            }
        }
    }

    private IEnumerator CollectSodaRoutine()
    {
        while (canCollect && sodaStack.Count < maxStack)
        {
            AddSoda();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void AddSoda()
    {
        Vector3 spawnPos = stackRoot.position + Vector3.up * (cubeHeight * sodaStack.Count);
        GameObject newSoda = Instantiate(sodaPrefab, spawnPos, Quaternion.identity);

        newSoda.transform.localScale = Vector3.zero;
        newSoda.transform.SetParent(stackRoot);
        newSoda.transform.DOScale(sodaTargetScale, tweenDuration).SetEase(tweenEase);

        sodaStack.Add(newSoda.transform);
    }

    public IEnumerator DropSodasRoutine()
    {
        while (isInDropArea && sodaStack.Count > 0)
        {
            Transform soda = sodaStack[sodaStack.Count - 1];
            sodaStack.RemoveAt(sodaStack.Count - 1);
            soda.SetParent(null);

            sodaDropList.Add(soda);
            soda.tag = "SodaProduct";

            if (soda.GetComponent<SodaProduct>() == null)
                soda.gameObject.AddComponent<SodaProduct>();

            int dropIndex = sodaDropList.Count - 1;
            Vector3 targetPos = sodaDropTarget.position + Vector3.up * (cubeHeight * dropIndex);

            soda.DOJump(targetPos, 0.002f, 1, 0.4f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    soda.rotation = Quaternion.identity;
                });

            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool SellSodaProduct()
    {
        if (sodaDropList.Count == 0) return false;

        Transform soda = sodaDropList[sodaDropList.Count - 1];
        sodaDropList.RemoveAt(sodaDropList.Count - 1);
        Destroy(soda.gameObject);

        return true;
    }

    public int SodaDropCount => sodaDropList.Count;

    // Yeni eklenen fonksiyonlar: StackCollector pivot değişimi için
    public int SodaStackCount => sodaStack.Count;

    public Transform GetSodaAt(int index)
    {
        if (index < 0 || index >= sodaStack.Count) return null;
        return sodaStack[index];
    }
}

// Soda ürünü tanımlama
public class SodaProduct : MonoBehaviour
{
    public int price = 2;
}