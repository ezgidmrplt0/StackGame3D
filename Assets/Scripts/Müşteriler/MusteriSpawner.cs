using UnityEngine;
using System.Collections.Generic;

public class MusteriSpawner : MonoBehaviour
{
    public List<GameObject> musteriPrefabs; // Birden fazla müşteri prefabını buraya ekle
    public Transform spawnPoint;
    public static Queue<MusteriHareket> musteriKuyrugu = new Queue<MusteriHareket>();

    public float spawnInterval = 3f;
    private float timer = 0f;

    public int maxMusteri = 8;  // Maksimum müşteri sayısı

    void Update()
    {
        timer += Time.deltaTime;

        // Kuyrukta maksimumdan az müşteri varsa ve zaman dolmuşsa spawn et
        if (timer >= spawnInterval && musteriKuyrugu.Count < maxMusteri)
        {
            SpawnMusteri();
            timer = 0f;
        }
    }

    void SpawnMusteri()
    {
        int index = Random.Range(0, musteriPrefabs.Count);

        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y = 0f; // Y eksenini sıfırla (zemine sabitle)

        GameObject yeniMusteri = Instantiate(
            musteriPrefabs[index],
            spawnPos,
            Quaternion.identity
        );

        MusteriHareket hareket = yeniMusteri.GetComponent<MusteriHareket>();
        hareket.kuyruktakiSirasi = musteriKuyrugu.Count;
        musteriKuyrugu.Enqueue(hareket);
    }




    // Kuyruktaki sıraları güncelle
    public static void UpdateQueuePositions()
    {
        MusteriHareket[] musteriler = musteriKuyrugu.ToArray();
        for (int i = 0; i < musteriler.Length; i++)
        {
            musteriler[i].kuyruktakiSirasi = i;
        }
    }

    // Müşteri işini bitirip gittiğinde çağırılacak fonksiyon
    public static void MusteriAyrildi(MusteriHareket musteri)
    {
        if (musteriKuyrugu.Contains(musteri))
        {
            Queue<MusteriHareket> yeniKuyruk = new Queue<MusteriHareket>();

            foreach (var m in musteriKuyrugu)
            {
                if (m != musteri)
                    yeniKuyruk.Enqueue(m);
            }

            musteriKuyrugu = yeniKuyruk;
            UpdateQueuePositions();
        }
    }
}
