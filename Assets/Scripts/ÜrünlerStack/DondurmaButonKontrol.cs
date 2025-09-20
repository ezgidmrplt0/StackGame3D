using UnityEngine;
using UnityEngine.UI;

public class DondurmaButonKontrol : MonoBehaviour
{
    public void DondurmaAc()
    {
        MusteriHareket.dondurmaAcik = true;
        Debug.Log("Dondurma dükkaný açýldý! Artýk dondurma müþterileri gelebilir.");

        // Ýsteðe baðlý: Butonu devre dýþý býrak
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;
        }
    } 
}