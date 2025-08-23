using UnityEngine;
using System.Collections.Generic;

public class MusteriSpawner : MonoBehaviour
{
    public List<GameObject> musteriPrefabs; // Birden fazla müþteri prefabýný buraya ekle
    public Transform spawnPoint;
    public static Queue<MusteriHareket> musteriKuyrugu = new Queue<MusteriHareket>();

    public float spawnInterval = 3f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMusteri();
            timer = 0f;
        }
    }

    void SpawnMusteri()
    {
        // Rastgele müþteri prefab seç
        int index = Random.Range(0, musteriPrefabs.Count);

        GameObject yeniMusteri = Instantiate(
            musteriPrefabs[index],
            spawnPoint.position,
            Quaternion.identity
        );

        MusteriHareket hareket = yeniMusteri.GetComponent<MusteriHareket>();
        hareket.kuyruktakiSirasi = musteriKuyrugu.Count;
        musteriKuyrugu.Enqueue(hareket);
    }

    public static void UpdateQueuePositions()
    {
        MusteriHareket[] musteriler = musteriKuyrugu.ToArray();
        for (int i = 0; i < musteriler.Length; i++)
        {
            musteriler[i].kuyruktakiSirasi = i;
        }
    }
}
