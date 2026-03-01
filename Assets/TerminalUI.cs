using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TerminalUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI outputText;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] float bottomPadding = 30f;
    [SerializeField] float scrollSnap = 0.06f;

    [Header("Sound")]
    [SerializeField] AudioSource typeSfx;

    System.Action<string> onCommand;
    RectTransform content, viewport;

    void Awake()
    {
        if (!outputText || !scrollRect || !inputField) { enabled = false; return; }

        content = scrollRect.content;
        viewport = scrollRect.viewport;
        if (!content || !viewport) { enabled = false; return; }

        inputField.onSubmit.AddListener(OnSubmit);
        scrollRect.inertia = false;
    }

    void Start()
    {
        if (!typeSfx)
        {
            typeSfx = gameObject.AddComponent<AudioSource>();
            typeSfx.playOnAwake = false;
            var clip = Resources.Load<AudioClip>("typeclick");
            if (clip) typeSfx.clip = clip;
        }

        FocusInput();
        Rebuild();
        SnapBottom();
    }

    public void Bind(System.Action<string> cb) => onCommand = cb;

    public void PrintLine(string line)
    {
        bool snap = NearBottom();
        outputText.text += line + "\n";
        Rebuild();
        if (snap) SnapBottom();
    }

    public void PrintTransientLine(string line, float dur)
    {
        if (dur <= 0f) dur = 0.5f;
        StartCoroutine(TransientRoutine(line, dur));
    }

    public void TriggerFlicker(float intensity = 0.08f) { }

    IEnumerator TransientRoutine(string line, float dur)
    {
        bool snap = NearBottom();
        string tagged = $"<color=#8A8A8A>{line}</color><size=1><color=#00000000>{System.Guid.NewGuid()}</color></size>";

        outputText.text += tagged + "\n";
        Rebuild();
        if (snap) SnapBottom();

        yield return new WaitForSeconds(dur);

        string needle = tagged + "\n";
        int idx = outputText.text.IndexOf(needle);
        if (idx >= 0)
        {
            outputText.text = outputText.text.Remove(idx, needle.Length);
            Rebuild();
            if (NearBottom()) SnapBottom();
        }
    }

    void OnSubmit(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text)) { inputField.text = ""; FocusInput(); return; }

        if (typeSfx && typeSfx.clip)
            typeSfx.PlayOneShot(typeSfx.clip, 0.3f);

        bool snap = NearBottom();
        outputText.text += "> " + text + "\n";
        Rebuild();
        if (snap) SnapBottom();

        inputField.text = "";
        FocusInput();
        onCommand?.Invoke(text);
    }

    void FocusInput()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    bool NearBottom() => scrollRect.verticalNormalizedPosition <= scrollSnap;

    void Rebuild()
    {
        outputText.ForceMeshUpdate();
        float h = Mathf.Max(outputText.preferredHeight + bottomPadding, viewport.rect.height + 1f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        Canvas.ForceUpdateCanvases();
    }

    void SnapBottom()
    {
        scrollRect.verticalNormalizedPosition = 0f;
        scrollRect.velocity = Vector2.zero;
    }
}
