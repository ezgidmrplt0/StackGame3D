using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StackCollector : MonoBehaviour
{
    [Header("Stack Ayarları")]
    public GameObject cubePrefab;
    public Transform stackRoot;
    public float cubeHeight = 0.3f;
    public float spawnInterval = 0.2f;
    public float tweenDuration = 0.4f;
    public Ease tweenEase = Ease.OutBack;

    [Header("Küp Boyutu")]
    public Vector3 cubeTargetScale = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("Stack Bırakma Ayarları")]
    public Transform stackAreaTarget; // 3. nokta (StackAlanı1)

    private readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;

    private int placedCount = 0; // 3. noktaya bırakılmış küp sayısı
    private bool isInDropArea = false; // drop noktasında mı?

    void OnTriggerEnter(Collider other)
    {
        // 1. Nokta: Stack toplamaya başla
        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop == null)
                stackingLoop = StartCoroutine(SpawnLoop());
        }

        // 2. Nokta: Stack bırak
        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = true; // drop noktasına girildi
            if (stack.Count > 0)
            {
                StartCoroutine(DropSequence());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 1. noktadan çıkınca stack durdur
        if (other.CompareTag("StackNoktasi0"))
        {
            if (stackingLoop != null)
            {
                StopCoroutine(stackingLoop);
                stackingLoop = null;
            }
        }

        // drop noktasından çıkınca bırakmayı durdur
        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = false;
        }
    }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            AddOneCube();
            yield return wait;
        }
    }

    void AddOneCube()
    {
        Vector3 targetLocalPos = new Vector3(0f, cubeHeight * stack.Count, 0f);
        Vector3 spawnWorldPos = stackRoot.position + Vector3.up * 1.5f;

        GameObject go = Instantiate(cubePrefab, spawnWorldPos, Quaternion.identity);
        go.transform.SetParent(stackRoot, true);
        go.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Join(go.transform.DOLocalMove(targetLocalPos, tweenDuration).SetEase(tweenEase));
        seq.Join(go.transform.DOScale(cubeTargetScale, tweenDuration).SetEase(tweenEase));

        stack.Add(go.transform);
    }

    IEnumerator DropSequence()
    {
        float dropSpacing = 0.9f;

        // Coroutine aktif olduğu sürece stack boşalana kadar veya drop noktasından çıkana kadar
        while (stack.Count > 0)
        {
            if (!isInDropArea) yield break; // drop noktasından çıkıldıysa durdur

            Transform cube = stack[stack.Count - 1]; // her seferinde en üstteki küpü al
            stack.RemoveAt(stack.Count - 1);

            int targetIndex = placedCount;
            Vector3 targetPos = stackAreaTarget.position + new Vector3(0f, dropSpacing * targetIndex, 0f);

            cube.SetParent(null);

            yield return cube.DOJump(targetPos, 0.5f, 1, 0.4f)
                             .SetEase(Ease.OutQuad)
                             .WaitForCompletion();

            placedCount++;
        }
    }


    public int Count => stack.Count;
}
