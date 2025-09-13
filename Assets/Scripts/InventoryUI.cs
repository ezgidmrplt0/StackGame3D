using UnityEngine;
using TMPro; // TextMeshPro için

public class InventoryUI : MonoBehaviour
{
    public StackCollector collector;      // StackCollector referansý
    public TextMeshPro stackText;     // Stack sayýsý için TMP
    public TextMeshPro dropText;      // Drop sayýsý için TMP

    void Update()
    {
        if (collector != null)
        {
            dropText.text = "" + collector.DropCount;
        }
    }
}
