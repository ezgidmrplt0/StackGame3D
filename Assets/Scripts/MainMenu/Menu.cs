using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Ayarlar sahnesine geçiþ metodu
    public void Ayarlar()
    {
        SceneManager.LoadScene("Settings");
    }

    // Ana menüye geri dönme metodu
    public void GeriDon()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Çýkýþ()
    {
        Application.Quit();
    }

    public void Oyna()
    {
        SceneManager.LoadScene("MainGame");
    }
}