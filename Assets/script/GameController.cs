using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TerminalUI terminal;

    [Header("Cases")]
    [SerializeField] private CaseData[] cases;

    private int index = 0;
    private int digIndex = 0;

    private int reputation = 0;
    private int crack = 0;

    private void Start()
    {
        terminal.Bind(HandleCommand);

        terminal.PrintLine("Welcome to Campfire!");
        terminal.PrintLine("\"Beneath the Surface\" — type 'help' to see commands.");
        terminal.PrintLine("");

        ShowCurrentCase();
    }

    private void ShowCurrentCase()
    {
        if (index >= cases.Length)
        {
            ShowEnding();
            return;
        }

        digIndex = 0;
        var c = cases[index];

        terminal.PrintLine($"--- Case {index + 1}/{cases.Length} ---");
        terminal.PrintLine($"Student: {c.studentName}");
        terminal.PrintLine(c.prompt);
        terminal.PrintLine("");
        terminal.PrintLine("Surface solution:");
        terminal.PrintLine(c.surfaceSolution);
        terminal.PrintLine("");
        terminal.PrintLine("Commands: approve | reject | dig | ask | status");
    }

    private void HandleCommand(string cmdRaw)
    {
        var cmd = cmdRaw.ToLowerInvariant();

        if (cmd == "help")
        {
            terminal.PrintLine("approve  : accept the solution");
            terminal.PrintLine("reject   : reject the solution");
            terminal.PrintLine("dig      : look beneath the surface (reveals layers, increases Crack)");
            terminal.PrintLine("ask      : ask the student (small dialogue)");
            terminal.PrintLine("status   : show Reputation/Crack");
            return;
        }

        if (cmd == "status")
        {
            terminal.PrintLine($"Reputation: {reputation} | Crack: {crack}");
            return;
        }

        if (index >= cases.Length) { ShowEnding(); return; }

        var c = cases[index];

        switch (cmd)
        {
            case "approve":
                if (c.isCorrect) reputation += 2;
                else reputation -= 1;

                terminal.PrintLine(c.isCorrect ? "Approved. (Routine.)" : "Approved... something feels off.");
                NextCase();
                break;

            case "reject":
                if (!c.isCorrect) reputation += 2;
                else reputation -= 1;

                terminal.PrintLine(!c.isCorrect ? "Rejected. (Clean.)" : "Rejected... maybe too harsh.");
                NextCase();
                break;

            case "ask":
                if (c.askLines != null && c.askLines.Length > 0)
                {
                    var line = c.askLines[Random.Range(0, c.askLines.Length)];
                    terminal.PrintLine($"{c.studentName}: {line}");
                }
                else terminal.PrintLine($"{c.studentName}: ...");
                break;

            case "dig":
                DoDig(c);
                break;

            default:
                terminal.PrintLine("Unknown command. Type 'help'.");
                break;
        }
    }

    private void DoDig(CaseData c)
    {
        crack += 1;

        if (c.digLayers != null && digIndex < c.digLayers.Length)
        {
            terminal.PrintLine("[DIG] You peel back a layer...");
            terminal.PrintLine(c.digLayers[digIndex]);
            digIndex++;

            if (!string.IsNullOrWhiteSpace(c.memoryFragment) && crack % 2 == 0)
            {
                terminal.PrintLine("");
                terminal.PrintLine("[MEMORY] " + c.memoryFragment);
            }
        }
        else
        {
            terminal.PrintLine("[DIG] Nothing else here. Just dirt and silence.");
        }

        if (crack >= 8)
            terminal.PrintLine("[SYSTEM] Your access feels... watched.");
    }

    private void NextCase()
    {
        terminal.PrintLine("");
        index++;
        ShowCurrentCase();
    }

    private void ShowEnding()
    {
        terminal.PrintLine("");
        terminal.PrintLine("=== END ===");

        if (crack >= 8 && reputation <= 0)
            terminal.PrintLine("Truth Ending: You dug until the ground gave way.");
        else if (crack < 5 && reputation >= 6)
            terminal.PrintLine("Surface Ending: Perfect record. Nothing breaks.");
        else
            terminal.PrintLine("Buried Ending: You saw enough. You chose to stop.");

        terminal.PrintLine($"Final Reputation: {reputation} | Final Crack: {crack}");
        terminal.PrintLine("Restart the scene to play again.");
    }
}