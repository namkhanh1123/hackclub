using TMPro;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Visual")]
    [SerializeField] private bool enableStyledOutput = true;
    [SerializeField] private int maxBufferedLines = 180;

    private System.Action<string> onCommand;

    private RectTransform content;
    private RectTransform viewport;
    private RectTransform outputRect;
    private readonly List<string> bufferedLines = new();

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
        outputRect = outputText.rectTransform;

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
        outputText.richText = true;
        outputText.enableWordWrapping = true;
        outputText.overflowMode = TextOverflowModes.Overflow;
        outputText.lineSpacing = 6f;

        FocusInput();
        RebuildContentHeight();
        ScrollToBottomImmediate();
    }

    public void Bind(System.Action<string> handler) => onCommand = handler;

    public void PrintLine(string line)
    {
        bool wasNearBottom = IsNearBottom();
        string styledLine = enableStyledOutput ? StyleLine(line) : line;
        AppendBufferedLine(styledLine);

        RebuildContentHeight();

        // Only autoscroll if player was already near bottom
        if (wasNearBottom)
            ScrollToBottomImmediate();
    }

    public void PrintTransientLine(string line, float duration)
    {
        if (duration <= 0f)
            duration = 0.5f;

        StartCoroutine(PrintTransientRoutine(line, duration));
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

        float textHeight = outputText.preferredHeight;
        float targetH = outputText.preferredHeight + extraBottomPadding;

        // Ensure content is always at least slightly taller than viewport,
        // otherwise ScrollRect may behave like "no scroll"
        float minH = viewport.rect.height + 1f;
        if (targetH < minH) targetH = minH;

        outputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);

        LayoutRebuilder.ForceRebuildLayoutImmediate(outputRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void ScrollToBottomImmediate()
    {
        scrollRect.verticalNormalizedPosition = 0f;
        scrollRect.velocity = Vector2.zero;
    }

    private IEnumerator PrintTransientRoutine(string line, float duration)
    {
        bool wasNearBottomWhenAdded = IsNearBottom();
        string displayLine = enableStyledOutput ? StyleLine(line) : line;
        string uniqueLine = "<alpha=#CC>" + displayLine + "</alpha><size=1><color=#00000000>"
            + System.Guid.NewGuid() + "</color></size>";

        outputText.text += uniqueLine + "\n";
        RebuildContentHeight();

        if (wasNearBottomWhenAdded)
            ScrollToBottomImmediate();

        yield return new WaitForSeconds(duration);

        string needle = uniqueLine + "\n";
        int startIndex = outputText.text.IndexOf(needle);
        if (startIndex >= 0)
        {
            outputText.text = outputText.text.Remove(startIndex, needle.Length);
            RebuildContentHeight();

            if (IsNearBottom())
                ScrollToBottomImmediate();
        }
    }

    private void AppendBufferedLine(string line)
    {
        bufferedLines.Add(line);

        if (maxBufferedLines > 0 && bufferedLines.Count > maxBufferedLines)
        {
            int removeCount = bufferedLines.Count - maxBufferedLines;
            bufferedLines.RemoveRange(0, removeCount);
        }

        outputText.text = string.Join("\n", bufferedLines);
        if (bufferedLines.Count > 0)
            outputText.text += "\n";
    }

    private string StyleLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return line;

        if (line.StartsWith("--- Case "))
        {
            string clean = line.Replace("--- ", "").Replace(" ---", "");
            return "<b><color=#7EF7C9>════════ " + clean + " ════════</color></b>";
        }

        if (line.StartsWith("Student:"))
            return "<b><color=#B4A7FF>▶ " + line + "</color></b>";

        if (line.StartsWith("Surface solution:"))
            return "<b><color=#FFD97A>● Surface Solution</color></b>";

        if (line.StartsWith("Commands:"))
            return StyleCommandsLine(line);

        if (line.StartsWith("Reputation:") || line.StartsWith("Morse Found:"))
            return "<color=#9CC2FF>" + line + "</color>";

        if (line.StartsWith("[STATS]"))
            return "<b><color=#6EE7FF>" + line + "</color></b>";

        if (line.StartsWith("Decision logged:"))
            return "<b><color=#E7F59A>" + line + "</color></b>";

        if (line.StartsWith("[DIG]"))
            return "<color=#FFD97A>" + line + "</color>";

        if (line.StartsWith("[MEMORY]"))
            return "<color=#FF8CD8>" + line + "</color>";

        if (line.StartsWith("[SIGNAL]"))
            return "<b><color=#FF6B8A>" + line + "</color></b>";

        if (line.StartsWith("[FLASHBACK]"))
            return "<color=#FFB184>" + line + "</color>";

        if (line.StartsWith("[IMPACT]"))
            return "<b><color=#74FF9B>" + line + "</color></b>";

        if (line.StartsWith("[OVERUSE]") || line.StartsWith("[OVERTHINK]"))
            return "<b><color=#FF6B6B>" + line + "</color></b>";

        if (line.StartsWith("[uhhidk]"))
            return "<color=#E9A2FF>" + line + "</color>";

        if (line.StartsWith("> "))
            return "<color=#78FFB1>" + line + "</color>";

        if (line == "Unknown command.")
            return "<color=#FF8E8E>Unknown command. Type <b>help</b>.</color>";

        int separatorIndex = line.IndexOf(": ");
        if (separatorIndex > 0 && !line.StartsWith("[") && !line.StartsWith("Commands:") && !line.StartsWith("Reputation:"))
        {
            string speaker = line.Substring(0, separatorIndex);
            string speech = line.Substring(separatorIndex + 2);
            return "<color=#A6D7FF>" + speaker + "</color>: <color=#EAEAEA>\"" + speech + "\"</color>";
        }

        if (line.StartsWith("=== "))
            return "<b><color=#E8F8E8>════════ " + line.Replace("=== ", "").Replace(" ===", "") + " ════════</color></b>";

        return "<color=#CFCFCF>" + line + "</color>";
    }

    private string StyleCommandsLine(string line)
    {
        string body = line.Replace("Commands:", "").Trim();
        string[] commands = body.Split('|');
        for (int commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            commands[commandIndex] = "<b><color=#9AD8FF>[" + commands[commandIndex].Trim() + "]</color></b>";

        return "<color=#9BA3AF>Commands</color> " + string.Join("  ", commands);
    }
}