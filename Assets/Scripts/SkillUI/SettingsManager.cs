using UnityEngine;
using UnityEngine.UI; // UI kütüphanesini kullanmak için ekleyin
using DG.Tweening; // DOTween kütüphanesini ekleyin

public class Settings : MonoBehaviour
{
    public GameObject settingsPanel;

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}