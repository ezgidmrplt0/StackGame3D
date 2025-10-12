using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{   // Play butonuna ver
    public void OnClickPlay()
    {
        SceneManager.LoadScene("MainGame");
    }

    // Settings butonuna ver
    public void OnClickSettings()
    {
        SceneManager.LoadScene("Settings");
    }
}
