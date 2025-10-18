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

    void Start()
    {
        mevcutFiyat = baslangicFiyati;
        GuncelleFiyatYazisi();
        satinAlButton.onClick.AddListener(SatinAlNPC);
    }

    private void GuncelleFiyatYazisi()
    {
        fiyatText.text = $"{mevcutFiyat}$";
    }

    private void SatinAlNPC()
    {
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

        // Para kontrol
        if (!MoneyManager.Instance.SpendMoney(mevcutFiyat))
        {
            Debug.Log("Yetersiz para!");
            return;
        }

        // NPC spawn
        GameObject npcObj = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        KahveTasiyicisiNPC npc = npcObj.GetComponent<KahveTasiyicisiNPC>();
        npc.calismaSuresi = npcCalismaSuresi;
        npc.ActivateNPC();

        npcAktif = true;
        StartCoroutine(ResetSatinalma(npcCalismaSuresi));

        // Fiyatı %20 artır
        mevcutFiyat = Mathf.RoundToInt(mevcutFiyat * (1f + artisOrani));
        GuncelleFiyatYazisi();
    }

    private IEnumerator ResetSatinalma(float sure)
    {
        yield return new WaitForSeconds(sure + 0.5f);
        npcAktif = false;
    }
}
