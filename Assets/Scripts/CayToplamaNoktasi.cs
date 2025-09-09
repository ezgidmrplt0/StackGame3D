using UnityEngine;
using DG.Tweening;

public class CayToplamaNoktasi : MonoBehaviour
{
    [Header("Ayarlamalar")]
    public float respawnSuresi = 5f;       // Yeniden çýkma süresi
    public Vector3 orijinalBoyut = Vector3.one; // Baþlangýç boyutu
    public Vector3 yokOlmaBoyutu = Vector3.zero; // Küçülme boyutu

    private bool toplanabilir = true;

    private Collider col; // Çarpýþma için

    void Start()
    {
        col = GetComponent<Collider>();
        if (col == null)
            Debug.LogWarning("Çay noktasý collider içermiyor!");
    }

    public void Toplandi()
    {
        if (!toplanabilir) return;

        toplanabilir = false;

        // Collider kapat -> oyuncu tekrar toplayamasýn
        if (col != null) col.enabled = false;

        // Küçült
        transform.DOScale(yokOlmaBoyutu, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // 5 saniye bekle ve tekrar büyüt
            Invoke(nameof(Respawn), respawnSuresi);
        });
    }

    private void Respawn()
    {
        // Tekrar büyü
        transform.DOScale(orijinalBoyut, 0.8f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (col != null) col.enabled = true;
            toplanabilir = true;
        });
    }

    public bool ToplanabilirMi()
    {
        return toplanabilir;
    }
}
