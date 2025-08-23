using UnityEngine;
using TMPro; // TextMeshPro için

public class InventoryUI : MonoBehaviour
{
    public StackCollector collector;      // StackCollector referansý
    public TextMeshProUGUI stackText;     // Stack sayýsý için TMP
    public TextMeshProUGUI dropText;      // Drop sayýsý için TMP

    void Update()
    {
        if (collector != null)
        {
            stackText.text = "Stack: " + collector.StackCount;
            dropText.text = "Drop: " + collector.DropCount;
        }
    }
}
