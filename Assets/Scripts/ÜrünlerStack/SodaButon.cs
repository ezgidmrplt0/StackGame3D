using UnityEngine;

public class SodaButon : MonoBehaviour
{
    public void SodaAc()
    {
        MusteriHareket.sodaAcik = true;
        StackCollector.Instance.sodacýAktif.SetActive(false);
        Debug.Log("Soda açýldý! Artýk müþteriler soda isteyebilir.");
    }
}
