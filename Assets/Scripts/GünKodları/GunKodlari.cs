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
    [SerializeField] private float minLightIntensity = 0f;
    [SerializeField] private float maxLightIntensity = 0.5f;

    [Header("Lighting Colors")]
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.15f, 0.15f, 0.25f); // Koyu lacivert/gri gece rengi

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
        }

        // Güneş rotasyonunu sildik, ışık hep sabit bir açıda kalacak (böylece yansıma yapmayacak)
        // transform.rotation = ...

        if (sceneLight != null)
        {
            // Saat 06:00 (0.25) ile 18:00 (0.75) arasını gündüz olarak hesapla
            float dayRatio = Mathf.Clamp01(Mathf.Sin((timeOfDay - 0.25f) * Mathf.PI * 2f));

            // Işığın rengini gündüzden geceye doğru yavaşça değiştir
            sceneLight.color = Color.Lerp(nightColor, dayColor, dayRatio);
            
            // Işığın şiddetini de yavaşça azaltıp artır
            sceneLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, dayRatio);
            
            // Işık objesi hep açık, çünkü artık rotasyonu sabit ve rengi geceye uygun
            sceneLight.enabled = true;
        }

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
            dayText.text = "Gun: " + dayCount;
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
