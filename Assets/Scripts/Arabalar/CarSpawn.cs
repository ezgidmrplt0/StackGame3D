using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSpawner : MonoBehaviour
{
    [Header("Araç Prefablarý (6 adet)")]
    public GameObject[] carPrefabs; // Inspector’dan 6 prefabý ekle

    [Header("Spawn Ayarlarý")]
    public Transform spawnPoint;    // ArabaSpawn alanýna yerleþtir
    public float spawnInterval = 10f; // Kaç saniyede bir araba çýkacak

    private void Start()
    {
        StartCoroutine(SpawnCars());
    }

    IEnumerator SpawnCars()
    {
        while (true)
        {
            // Rastgele prefab seç
            int randomIndex = Random.Range(0, carPrefabs.Length);
            GameObject car = Instantiate(carPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
