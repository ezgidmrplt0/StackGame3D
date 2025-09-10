using UnityEngine;
using System.Collections.Generic;

public class MusteriSpawner : MonoBehaviour
{
    [Header("Normal Müşteri Ayarları")]
    public List<GameObject> musteriPrefabs;
    public Transform spawnPoint;
    public static Queue<MusteriHareket> musteriKuyrugu = new Queue<MusteriHareket>();

    [Header("Dondurma Müşteri Ayarları")]
    public List<GameObject> dondurmaMusteriPrefabs;
    public Transform dondurmaSpawnPoint;
    public static Queue<MusteriHareket> dondurmaMusteriKuyrugu = new Queue<MusteriHareket>();

    [Header("Spawn Ayarları")]
    public float spawnInterval = 3f;
    private float timer = 0f;
    public int maxMusteri = 15;
    public int maxDondurmaMusteri = 3;

    [Range(0f, 1f)]
    public float dondurmaMusteriOrani = 0.3f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            bool dondurmaSpawn = Random.value < dondurmaMusteriOrani;

            // Dondurma müşterisi yalnızca dondurma açık ise spawn olur
            if (dondurmaSpawn && MusteriHareket.dondurmaAcik && dondurmaMusteriKuyrugu.Count < maxDondurmaMusteri)
            {
                SpawnDondurmaMusteri();
            }

            // Normal müşteriler
            if (musteriKuyrugu.Count < maxMusteri)
            {
                SpawnNormalMusteri();
            }

            timer = 0f;
        }
    }

    void SpawnNormalMusteri()
    {
        if (musteriPrefabs.Count == 0) return;

        int index = Random.Range(0, musteriPrefabs.Count);
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y = 0f;

        GameObject yeniMusteri = Instantiate(
            musteriPrefabs[index],
            spawnPos,
            Quaternion.identity
        );

        MusteriHareket hareket = yeniMusteri.GetComponent<MusteriHareket>();
        if (hareket != null)
        {
            hareket.kuyruktakiSirasi = musteriKuyrugu.Count;
            hareket.musteriTipi = MusteriHareket.MusteriTipi.Normal;
            musteriKuyrugu.Enqueue(hareket);
        }
    }

    void SpawnDondurmaMusteri()
    {
        if (dondurmaMusteriPrefabs.Count == 0) return;

        int index = Random.Range(0, dondurmaMusteriPrefabs.Count);
        Vector3 spawnPos = dondurmaSpawnPoint.position;
        spawnPos.y = 0f;

        GameObject yeniMusteri = Instantiate(
            dondurmaMusteriPrefabs[index],
            spawnPos,
            Quaternion.identity
        );

        MusteriHareket hareket = yeniMusteri.GetComponent<MusteriHareket>();
        if (hareket != null)
        {
            hareket.kuyruktakiSirasi = dondurmaMusteriKuyrugu.Count;
            hareket.musteriTipi = MusteriHareket.MusteriTipi.Dondurma;
            dondurmaMusteriKuyrugu.Enqueue(hareket);
        }
    }

    public static void UpdateQueuePositions()
    {
        MusteriHareket[] musteriler = musteriKuyrugu.ToArray();
        for (int i = 0; i < musteriler.Length; i++)
        {
            musteriler[i].kuyruktakiSirasi = i;
        }
    }

    public static void UpdateDondurmaQueuePositions()
    {
        MusteriHareket[] musteriler = dondurmaMusteriKuyrugu.ToArray();
        for (int i = 0; i < musteriler.Length; i++)
        {
            musteriler[i].kuyruktakiSirasi = i;
        }
    }

    public static void MusteriAyrildi(MusteriHareket musteri)
    {
        if (musteri.musteriTipi == MusteriHareket.MusteriTipi.Normal)
        {
            RemoveFromQueue(musteri, ref musteriKuyrugu, UpdateQueuePositions);
        }
        else
        {
            RemoveFromQueue(musteri, ref dondurmaMusteriKuyrugu, UpdateDondurmaQueuePositions);
        }
    }

    private static void RemoveFromQueue(MusteriHareket musteri, ref Queue<MusteriHareket> kuyruk, System.Action updateAction)
    {
        if (kuyruk.Contains(musteri))
        {
            Queue<MusteriHareket> yeniKuyruk = new Queue<MusteriHareket>();
            foreach (var m in kuyruk)
            {
                if (m != musteri)
                    yeniKuyruk.Enqueue(m);
            }
            kuyruk = yeniKuyruk;
            updateAction?.Invoke();
        }
    }
}
