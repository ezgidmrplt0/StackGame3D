using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusteriSpawner : MonoBehaviour
{
    public GameObject musteriPrefab;
    public float spawnInterval = 3f;

    private Transform spawnPoint;

    public static Queue<MusteriHareket> musteriKuyrugu = new Queue<MusteriHareket>();

    void Start()
    {
        spawnPoint = GameObject.FindGameObjectWithTag("BeklemeNoktasi").transform;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnMusteri();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnMusteri()
    {
        GameObject yeniMusteri = Instantiate(musteriPrefab, spawnPoint.position, Quaternion.identity);

        MusteriHareket musteriScript = yeniMusteri.GetComponent<MusteriHareket>();
        musteriKuyrugu.Enqueue(musteriScript);
        musteriScript.kuyruktakiSirasi = musteriKuyrugu.Count - 1;
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
