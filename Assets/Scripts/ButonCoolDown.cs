using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButonCoolDown : MonoBehaviour
{
    [Header("Cooldown Süresi (saniye)")]
    public float cooldownTime = 30f;

    [Header("Cooldown Eklenecek Butonlar")]
    public List<Button> buttons = new List<Button>();

    void Start()
    {
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => StartCooldown(btn));
        }
    }

    void StartCooldown(Button button)
    {
        StartCoroutine(CooldownRoutine(button));
    }

    IEnumerator CooldownRoutine(Button button)
    {
        button.interactable = false; // Butonu devre dýþý býrak
        yield return new WaitForSeconds(cooldownTime);
        button.interactable = true;  // Tekrar aktif yap
    }
}
