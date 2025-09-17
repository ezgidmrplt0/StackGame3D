using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunKodlari : MonoBehaviour
{
    // Inspector'dan atayacaðýn objeler
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText; // Saat için TextMeshPro objesi

    // Gün döngüsü ayarlarý
    [Header("Cycle Settings")]
    [SerializeField] private float dayDurationInMinutes = 5f;
    [SerializeField] private float rotationSpeed;

    // Özel ýþýk ayarlarý
    [Header("Lighting")]
    [SerializeField] private Light sceneLight;

    // timeOfDay'ý static yaparak diðer sýnýflarýn eriþimini saðla
    private static float timeOfDay;
    private int dayCount = 1;

    // Son güncellenen dakikayý tutmak için deðiþken
    private int lastDisplayedMinute = -1;

    void Start()
    {
        if (sceneLight == null)
        {
            sceneLight = GetComponent<Light>();
        }

        rotationSpeed = 360f / (dayDurationInMinutes * 60f);
        timeOfDay = 0.5f; // Baþlangýç öðlen 12:00

        UpdateDayText();
        UpdateTimeText(); // Baþlangýçta saati bir kez göster
    }

    void Update()
    {
        timeOfDay += Time.deltaTime * rotationSpeed / 360f;

        if (timeOfDay >= 1f)
        {
            timeOfDay = 0f;
            dayCount++;
            UpdateDayText();
        }

        transform.rotation = Quaternion.Euler(new Vector3((timeOfDay * 360f) - 90f, 170f, 0));

        // Saati yalnýzca 10 dakikada bir güncelle
        int currentMinute = Mathf.FloorToInt((timeOfDay * 24f * 60f) % 60f);
        if (currentMinute % 10 == 0 && currentMinute != lastDisplayedMinute)
        {
            UpdateTimeText();
            lastDisplayedMinute = currentMinute;
        }
    }

    private void UpdateDayText()
    {
        if (dayText != null)
        {
            dayText.text = "Gün: " + dayCount.ToString();
        }
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
        {
            float totalHours = timeOfDay * 24f;
            int hours = Mathf.FloorToInt(totalHours);
            int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);

            string amPm = "AM";
            if (hours >= 12)
            {
                amPm = "PM";
            }
            if (hours > 12)
            {
                hours -= 12;
            }
            if (hours == 0)
            {
                hours = 12;
            }

            timeText.text = string.Format("{0:00}:{1:00} {2}", hours, minutes, amPm);
        }
    }

    // Akþam vakti mi olduðunu kontrol eden public static metot
    public static bool IsEvening()
    {
        // 18:00 (0.75) ile 06:00 (0.25) arasý akþam/gece kabul edilir.
        return timeOfDay > 0.75f || timeOfDay < 0.25f;
    }
}