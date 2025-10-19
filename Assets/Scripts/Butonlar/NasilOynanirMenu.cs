using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NasilOynanirMenu : MonoBehaviour
{
    [SerializeField] string sahneAdi = "MainMenu";

    public void AnaMenuyeDon()
    {
        Debug.Log($"[NasilOynanirMenu] Buton týklandý, {sahneAdi} yükleniyor...");
        SceneManager.LoadScene(sahneAdi);
    }
}
