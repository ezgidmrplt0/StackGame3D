using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DialogueSystem : MonoBehaviour
{
    [System.Serializable]
    public class Line
    {
        [TextArea(2, 5)] public string text;
    }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Button nextButton;

    [Header("Typing Settings")]
    [SerializeField] private float charsPerSecond = 45f;

    [Header("Input Lock Settings")]
#if ENABLE_INPUT_SYSTEM
    // Yeni Input System kullan�yorsan PlayerInput ba�lan�r (opsiyonel)
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayActionMap = "Gameplay";
    [SerializeField] private string uiActionMap = "UI";
#endif

    // Diyalog a��kken devre d��� b�rak�lacak scriptler (mesela PlayerMovement, Joystick vb.)
    [SerializeField] private List<Behaviour> disableWhileOpen = new List<Behaviour>();

    [Header("Story")]
    [SerializeField] private List<Line> story = new List<Line>();

    private int index = -1;
    private bool isTyping = false;
    private string currentFullText = "";

    private readonly List<(Behaviour comp, bool wasEnabled)> rememberStates = new();

    void Start()
    {
        if (panel) panel.SetActive(false);

        if (story.Count == 0)
            FillDefaultStory();

        StartDialogue();
    }

    public void StartDialogue()
    {
        panel.SetActive(true);
        LockInput();
        index = -1;
        NextLine();
    }

    public void OnNextButton()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
            bodyText.text = currentFullText;
        }
        else
        {
            NextLine();
        }
    }

    private void NextLine()
    {
        index++;
        if (index >= story.Count)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypeLine(story[index].text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        bodyText.text = "";
        currentFullText = text;

        float delay = Mathf.Approximately(charsPerSecond, 0f) ? 0f : 1f / charsPerSecond;
        foreach (char c in currentFullText)
        {
            bodyText.text += c;
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            else
                yield return null;
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        panel.SetActive(false);
        UnlockInput();
    }

    private void LockInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (playerInput != null && !string.IsNullOrEmpty(uiActionMap))
            playerInput.SwitchCurrentActionMap(uiActionMap);
#endif
        rememberStates.Clear();
        foreach (var comp in disableWhileOpen)
        {
            if (comp == null) continue;
            rememberStates.Add((comp, comp.enabled));
            comp.enabled = false;
        }
    }

    private void UnlockInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (playerInput != null && !string.IsNullOrEmpty(gameplayActionMap))
            playerInput.SwitchCurrentActionMap(gameplayActionMap);
#endif
        foreach (var state in rememberStates)
        {
            if (state.comp != null)
                state.comp.enabled = state.wasEnabled;
        }
        rememberStates.Clear();
    }

    private void FillDefaultStory()
    {
        story = new List<Line>
        {
            new Line{ text = "Evlat sonunda geldin. Bu cay ocagi artik sana emanet." },
            new Line{ text = "Bu ocak yillardir mahalleye sadece cay degil; emek, saygi ve sicaklik dagitmistir." },
            new Line{ text = "Zaman degismis olabilir ama cayin bereketi degismez." },
            new Line{ text = "Sen bu mirasi modern caga tasiyacaksin. Duzeni kur, musteriyi memnun et, emegi unutma." },
            new Line{ text = "Hazirsan basliyoruz... ocagi yakmanin vakti geldi!" }
        };
    }
}
