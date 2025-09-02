using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExpansionStep
{
    public List<GameObject> objectsToDestroy;
    public List<GameObject> objectsToActivate;
}

public class ExpandManager : MonoBehaviour
{
    [Header("Geniþletme Adýmlarý")]
    public List<ExpansionStep> expansionSteps = new List<ExpansionStep>();

    private int currentStep = 0;

    public void OnExpandButtonClick()
    {
        if (currentStep >= expansionSteps.Count)
        {
            Debug.Log("Tüm geniþletme adýmlarý tamamlandý.");
            return;
        }

        ExpansionStep step = expansionSteps[currentStep];

        // Bu adýmda silinecekleri yok et
        foreach (GameObject obj in step.objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // Bu adýmda açýlacaklarý aktif et
        foreach (GameObject obj in step.objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        currentStep++;
    }
}
