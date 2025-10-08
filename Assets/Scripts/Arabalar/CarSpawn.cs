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

            // Eðer yanlýþ dönen araçlardansa düzelt
            string prefabName = carPrefabs[randomIndex].name;

            if (prefabName == "Bus")
            {
                // 90 derece döndür (sahneye göre ayarlayabilirsin)
                car.transform.Rotate(0f, 90f, 0f);
            }
            if (prefabName == "Gray Car" || prefabName == "Orange Truck" || prefabName == "Hatchback Car_15" || prefabName == "N_Muscle Car_10" || prefabName == "Pick Up_11" || prefabName == "Sport Car_39")
            {
                car.transform.Rotate(0f, -90f, 0);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
