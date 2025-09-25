using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Buton Paneli")]
    public Transform buttonParent;
    public List<Button> buttons = new List<Button>();

    void Start()
    {
        buttons.Clear();
        foreach (Transform child in buttonParent)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                buttons.Add(btn);

                // ✅ Local kopya
                Button localBtn = btn;
                localBtn.onClick.AddListener(() => OnButtonClicked(localBtn));
            }
        }
    }

    void OnButtonClicked(Button clickedButton)
    {
        int index = buttons.IndexOf(clickedButton);

        if (index >= 0 && index + 1 < buttons.Count)
        {
            Button nextButton = buttons[index + 1];

            // Alt butonu üst butonun yerine taşı
            nextButton.transform.position = clickedButton.transform.position;
            nextButton.transform.rotation = clickedButton.transform.rotation;
            nextButton.transform.localScale = clickedButton.transform.localScale;
        }

        // Önce listeden çıkar
        buttons.Remove(clickedButton);

        // Sonra butonu yok et
        Destroy(clickedButton.gameObject);
    }
}
