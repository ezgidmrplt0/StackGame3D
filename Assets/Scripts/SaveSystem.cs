using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSystem : MonoBehaviour
{
    [Header("UI")]
    public Button saveButton;

    private void Start()
    {
        LoadData();

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveDataButton);
    }

    public void SaveDataButton()
    {
        SaveData();
        Debug.Log("💾 Kaydet butonuna basıldı!");
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("Money", MoneyManager.Instance.money);
        PlayerPrefs.SetInt("Day", GunKodlari.GetCurrentDay());
        PlayerPrefs.SetFloat("TimeOfDay", GunKodlari.GetTimeOfDay());
        PlayerPrefs.Save();

        Debug.Log("💾 Veriler kaydedildi!");
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("Money"))
        {
            MoneyManager.Instance.money = PlayerPrefs.GetInt("Money");
            MoneyManager.Instance.UpdateUI();
        }

        if (PlayerPrefs.HasKey("Day"))
        {
            GunKodlari.SetDay(PlayerPrefs.GetInt("Day"));
        }

        if (PlayerPrefs.HasKey("TimeOfDay"))
        {
            GunKodlari.SetTimeOfDay(PlayerPrefs.GetFloat("TimeOfDay"));
        }

        // 🔥 UI’yı hemen güncelle
        if (GunKodlari.Instance != null)
        {
            GunKodlari.Instance.UpdateDayText();
            GunKodlari.Instance.UpdateTimeText();
        }

        Debug.Log("📂 Veriler yüklendi!");
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("🧹 Kayıt sıfırlandı!");
    }
}
