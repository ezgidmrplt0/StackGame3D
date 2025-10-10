using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunKodlari : MonoBehaviour
{
    public static GunKodlari Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Cycle Settings")]
    [SerializeField] private float dayDurationInMinutes = 20f;

    [Header("Lighting")]
    [SerializeField] private Light sceneLight;

    private static float timeOfDay;
    private int dayCount = 1;
    private int lastDisplayedMinute = -1;

    void Awake()
    {
        Instance = this;
        timeOfDay = 10f / 24f;
    }

    void Start()
    {
        if (sceneLight == null)
            sceneLight = GetComponent<Light>();

        UpdateDayText();
        UpdateTimeText();
    }

    void Update()
    {
        timeOfDay += (Time.deltaTime / (dayDurationInMinutes * 60f));

        if (timeOfDay >= 1f)
        {
            timeOfDay = 0f;
            dayCount++;
            UpdateDayText();

            // Gün değiştiğinde otomatik kaydet
            FindObjectOfType<SaveSystem>().SaveData();
        }

        transform.rotation = Quaternion.Euler(new Vector3((timeOfDay * 360f) - 90f, 170f, 0));

        int currentMinute = Mathf.FloorToInt((timeOfDay * 24f * 60f) % 60f);
        if (currentMinute % 10 == 0 && currentMinute != lastDisplayedMinute)
        {
            UpdateTimeText();
            lastDisplayedMinute = currentMinute;
        }
    }

    public void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = "Gün: " + dayCount;
    }

    public void UpdateTimeText()
    {
        if (timeText != null)
        {
            float totalHours = timeOfDay * 24f;
            int hours = Mathf.FloorToInt(totalHours);
            int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);

            string amPm = (hours >= 12) ? "PM" : "AM";
            if (hours > 12) hours -= 12;
            if (hours == 0) hours = 12;

            timeText.text = string.Format("{0:00}:{1:00} {2}", hours, minutes, amPm);
        }
    }

    public static bool IsEvening()
    {
        return timeOfDay > 0.75f || timeOfDay < 0.25f;
    }

    // Getter / Setter
    public static int GetCurrentDay()
    {
        return Instance.dayCount;
    }

    public static void SetDay(int day)
    {
        if (Instance == null) return;
        Instance.dayCount = day;
        Instance.UpdateDayText();
    }

    public static float GetTimeOfDay()
    {
        return timeOfDay;
    }

    public static void SetTimeOfDay(float value)
    {
        timeOfDay = Mathf.Clamp01(value);
        if (Instance != null)
            Instance.UpdateTimeText();
    }
}
