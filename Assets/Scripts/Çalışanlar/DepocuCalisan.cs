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

    [Header("Animasyon")]
    public Transform modelTransform;
    private Tween _animTween;
    private Vector3 _origScale;
    private bool _isWalkingAnim;

    void Start()
    {
        _origScale = (modelTransform != null ? modelTransform : transform).localScale;
        StartCoroutine(CalismaRutini());
    }

    IEnumerator CalismaRutini()
    {
        while (calisiyor)
        {
            // 1. Toplama noktas�na git
            yield return StartCoroutine(GitVeTopla());

            // 2. B�rakma noktas�na git
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

        // B�rak
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
        if (!_isWalkingAnim)
        {
            _isWalkingAnim = true;
            Transform at = modelTransform != null ? modelTransform : transform;
            _animTween?.Kill();
            _animTween = at.DOScaleY(_origScale.y * 1.06f, 0.18f)
                .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        // Hedefe d�n (sadece Y ekseninde)
        Vector3 direction = (hedef - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0); // sadece Y ekseninde d�ns�n
            transform.rotation = lookRotation;
        }

        // Hedefe do�ru y�r�
        while (Vector3.Distance(transform.position, hedef) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);
            yield return null;
        }

        if (_isWalkingAnim)
        {
            _isWalkingAnim = false;
            Transform at = modelTransform != null ? modelTransform : transform;
            _animTween?.Kill();
            at.DOScaleY(_origScale.y, 0.12f);
        }
    }

    void AddHamCayCube()
    {
        if (hamCayPrefab == null || stackRoot == null) return;
        Vector3 offset = Vector3.up * stackSpacing * stack.Count;
        GameObject newLeaf = Instantiate(hamCayPrefab, stackRoot.position + offset, Quaternion.identity, stackRoot);

        // Collider'lar� kapat
        Collider[] colliders = newLeaf.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        newLeaf.transform.localScale = Vector3.zero;
        newLeaf.transform.DOScale(Vector3.one * 0.3f, 0.3f).SetEase(Ease.OutBack);
        stack.Add(newLeaf.transform);
        (modelTransform != null ? modelTransform : transform).DOPunchScale(Vector3.one * 0.25f, 0.2f, 4, 0.5f);
    }


    void RemoveHamCayCube()
    {
        if (stack.Count == 0) return;
        Transform lastLeaf = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        lastLeaf.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => Destroy(lastLeaf.gameObject));
        if (stack.Count == 0)
            (modelTransform != null ? modelTransform : transform).DOScale(_origScale, 0.2f);
    }

    public void CalismayiBitir()
    {
        calisiyor = false;
        StopAllCoroutines();

        // E�er �zerinde �ay varsa, �nce onlar� b�rak sonra yok ol
        if (uzerindekiCay > 0)
        {
            StartCoroutine(CaylariBirakVeYokOl());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator CaylariBirakVeYokOl()
    {
        // B�rakma noktas�na git
        if (birakmaNoktasi != null)
        {
            yield return StartCoroutine(Git(birakmaNoktasi.position));

            // T�m �aylar� b�rak
            while (uzerindekiCay > 0)
            {
                uzerindekiCay--;
                RemoveHamCayCube();
                StackCollector.Instance.UretimStokEkle(1);
                yield return new WaitForSeconds(0.1f);
            }
        }

        // T�m �aylar b�rak�ld�ktan sonra yok ol
        Destroy(gameObject);
    }
}