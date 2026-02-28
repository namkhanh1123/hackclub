using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_InputField inputField;

    [Header("Scroll")]
    [SerializeField] private float extraBottomPadding = 30f;
    [SerializeField] private float autoScrollThreshold = 0.06f; // 0 = bottom, 1 = top

    private System.Action<string> onCommand;

    private RectTransform content;
    private RectTransform viewport;

    private void Awake()
    {
        if (!outputText || !scrollRect || !inputField)
        {
            Debug.LogError("TerminalUI: Missing references (outputText/scrollRect/inputField).");
            enabled = false;
            return;
        }

        content = scrollRect.content;
        viewport = scrollRect.viewport;

        if (!content || !viewport)
        {
            Debug.LogError("TerminalUI: ScrollRect missing Content/Viewport references.");
            enabled = false;
            return;
        }

        // New Input System compatible
        inputField.onSubmit.AddListener(OnSubmit);

        // Optional: reduce weird motion
        scrollRect.inertia = false;
    }

    private void Start()
    {
        FocusInput();
        RebuildContentHeight();
        ScrollToBottomImmediate();
    }

    public void Bind(System.Action<string> handler) => onCommand = handler;

    public void PrintLine(string line)
    {
        bool wasNearBottom = IsNearBottom();

        outputText.text += line + "\n";

        RebuildContentHeight();

        // Only autoscroll if player was already near bottom
        if (wasNearBottom)
            ScrollToBottomImmediate();
    }

    private void OnSubmit(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            inputField.text = "";
            FocusInput();
            return;
        }

        PrintLine("> " + text);

        inputField.text = "";
        FocusInput();

        onCommand?.Invoke(text);
    }

    private void FocusInput()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    private bool IsNearBottom()
    {
        // When at bottom: verticalNormalizedPosition ~ 0
        return scrollRect.verticalNormalizedPosition <= autoScrollThreshold;
    }

    private void RebuildContentHeight()
    {
        // Force TMP to calculate preferredHeight accurately
        outputText.ForceMeshUpdate();

        float targetH = outputText.preferredHeight + extraBottomPadding;

        // Ensure content is always at least slightly taller than viewport,
        // otherwise ScrollRect may behave like "no scroll"
        float minH = viewport.rect.height + 1f;
        if (targetH < minH) targetH = minH;

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);

        Canvas.ForceUpdateCanvases();
    }

    private void ScrollToBottomImmediate()
    {
        scrollRect.verticalNormalizedPosition = 0f;
        scrollRect.velocity = Vector2.zero;
    }
}