using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DepocuCalisan : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 3f;
    public int kapasite = 10;
    public float toplamaAraligi = 0.2f;

    private int uzerindekiCay = 0;
    private bool calisiyor = true;

    [Header("Stack Sistemi")]
    public GameObject hamCayPrefab;
    public Transform stackRoot;
    public float stackSpacing = 0.3f;
    private List<Transform> stack = new List<Transform>();

    [HideInInspector] public Transform toplamaNoktasi;
    [HideInInspector] public Transform birakmaNoktasi;

    void Start()
    {
        StartCoroutine(CalismaRutini());
    }

    IEnumerator CalismaRutini()
    {
        while (calisiyor)
        {
            // 1. Toplama noktasýna git
            yield return StartCoroutine(GitVeTopla());

            // 2. Býrakma noktasýna git
            yield return StartCoroutine(GitVeBirak());
        }
    }

    IEnumerator GitVeTopla()
    {
        if (toplamaNoktasi == null) yield break;

        // Git
        yield return StartCoroutine(Git(toplamaNoktasi.position));

        // Topla
        while (uzerindekiCay < kapasite)
        {
            uzerindekiCay++;
            AddHamCayCube();
            yield return new WaitForSeconds(toplamaAraligi);
        }
    }

    IEnumerator GitVeBirak()
    {
        if (birakmaNoktasi == null) yield break;

        // Git
        yield return StartCoroutine(Git(birakmaNoktasi.position));

        // Býrak
        while (uzerindekiCay > 0)
        {
            uzerindekiCay--;
            RemoveHamCayCube();
            StackCollector.Instance.UretimStokEkle(1); // StackCollector'a stok ekle
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Git(Vector3 hedef)
    {
        // Hedefe dön (sadece Y ekseninde)
        Vector3 direction = (hedef - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0); // sadece Y ekseninde dönsün
            transform.rotation = lookRotation;
        }

        // Hedefe doðru yürü
        while (Vector3.Distance(transform.position, hedef) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);
            yield return null;
        }
    }



    void AddHamCayCube()
    {
        if (hamCayPrefab == null || stackRoot == null) return;
        Vector3 offset = Vector3.up * stackSpacing * stack.Count;
        GameObject newLeaf = Instantiate(hamCayPrefab, stackRoot.position + offset, Quaternion.identity, stackRoot);
        newLeaf.transform.localScale = Vector3.zero;
        newLeaf.transform.DOScale(Vector3.one * 0.3f, 0.3f).SetEase(Ease.OutBack);
        stack.Add(newLeaf.transform);
    }

    void RemoveHamCayCube()
    {
        if (stack.Count == 0) return;
        Transform lastLeaf = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        lastLeaf.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => Destroy(lastLeaf.gameObject));
    }

    public void CalismayiBitir()
    {
        calisiyor = false;
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
