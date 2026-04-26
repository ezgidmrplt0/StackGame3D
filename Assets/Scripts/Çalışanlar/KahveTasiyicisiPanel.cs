using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KahveTasiyicisiPanel : MonoBehaviour
{
    [Header("NPC Ayarları")]
    public GameObject npcPrefab;
    public Transform spawnPoint;
    public float npcCalismaSuresi = 20f;

    [Header("UI Elemanları")]
    public Button satinAlButton;
    public TextMeshProUGUI fiyatText;

    [Header("Fiyat Ayarları")]
    public int baslangicFiyati = 100;
    public float artisOrani = 0.2f; // %20

    private int mevcutFiyat;
    private bool npcAktif = false;
    private bool alaniAcildi = false;

    void Start()
    {
        mevcutFiyat = baslangicFiyati;
        GuncelleFiyatYazisi();
        satinAlButton.onClick.AddListener(SatinAlNPC);
        satinAlButton.interactable = false;
    }

    public void KahveAlaniAc()
    {
        alaniAcildi = true;
        satinAlButton.interactable = true;
    }

    private void GuncelleFiyatYazisi()
    {
        fiyatText.text = $"{mevcutFiyat}$";
    }

    private void SatinAlNPC()
    {
        if (!alaniAcildi)
        {
            Debug.Log("Kahve alanı henüz satın alınmadı!");
            return;
        }

        if (npcAktif)
        {
            Debug.Log("NPC zaten aktif, bekleniyor...");
            return;
        }

        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager sahnede yok!");
            return;
        }

        if (!MoneyManager.Instance.SpendMoney(mevcutFiyat))
        {
            Debug.Log("Yetersiz para!");
            return;
        }

        GameObject npcObj = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        KahveTasiyicisiNPC npc = npcObj.GetComponent<KahveTasiyicisiNPC>();
        npc.calismaSuresi = npcCalismaSuresi;
        npc.ActivateNPC();

        npcAktif = true;
        satinAlButton.interactable = false;
        StartCoroutine(ResetSatinalma(npcCalismaSuresi));

        mevcutFiyat = Mathf.RoundToInt(mevcutFiyat * (1f + artisOrani));
        GuncelleFiyatYazisi();
    }

    private IEnumerator ResetSatinalma(float sure)
    {
        yield return new WaitForSeconds(sure + 0.5f);
        npcAktif = false;
        satinAlButton.interactable = true;
    }
}
