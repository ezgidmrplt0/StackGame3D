using UnityEngine;

public class SodaButon : MonoBehaviour
{
    public void SodaAc()
    {
        MusteriHareket.sodaAcik = true;
        Debug.Log("Soda açýldý! Artýk müþteriler soda isteyebilir.");
    }
}
