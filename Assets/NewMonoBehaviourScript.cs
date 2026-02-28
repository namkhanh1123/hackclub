using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TerminalManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField terminalInput;
    public TextMeshProUGUI terminalHistoryText;

    [Header("Terminal Settings")]
    public int maxLines = 15;
    private List<string> lineHistory = new List<string>();

    void Start()
    {
        // Setup initial terminal screen
        ClearTerminal();
        AddLineToHistory("SYS_OS v2.4 initialized.");
        AddLineToHistory("Type 'help' for a list of available commands.");
        
        // Keep focus on the input field so the player doesn't have to click it constantly
        terminalInput.ActivateInputField();
        terminalInput.onSubmit.AddListener(OnInputSubmit);
    }

    void OnInputSubmit(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        // Echo the player's input to the screen
        AddLineToHistory("<color=#00FF00>> " + input + "</color>");
        
        // Parse the command
        ParseCommand(input.ToLower().Trim());

        // Clear input field and refocus
        terminalInput.text = "";
        terminalInput.ActivateInputField();
    }

    void ParseCommand(string command)
    {
        switch (command)
        {
            case "help":
                AddLineToHistory("AVAILABLE COMMANDS:");
                AddLineToHistory(" - calc_thermal : Calculate heat transfer variables");
                AddLineToHistory(" - solve_calc   : Compute standard integrals");
                AddLineToHistory(" - run_prgm     : Execute user-defined programs");
                AddLineToHistory(" - clear        : Clear terminal output");
                break;

            case "clear":
                ClearTerminal();
                break;

            case "calc_thermal":
                AddLineToHistory("Calculating... Error: Missing variable 'q'.");
                AddLineToHistory("System under too much pressure. Unable to stabilize.");
                break;

            case "solve_calc":
                AddLineToHistory("Processing geometry and integrals...");
                AddLineToHistory("Result: Undefined. The limit does not exist.");
                break;

            case "run_prgm":
                // This is where the surface cracks
                AddLineToHistory("<color=red>WARN: PRGM corrupted.</color>");
                AddLineToHistory("<color=red>I can't keep staring at these equations.</color>");
                AddLineToHistory("<color=red>Type 'read_log' to recover fragmented data.</color>");
                break;

            case "read_log":
                // The emotional payload
                AddLineToHistory("<color=#FFA500>--- RECOVERED FRAGMENT 01 ---</color>");
                AddLineToHistory("<color=#FFA500>I told them I was ready for the finals.</color>");
                AddLineToHistory("<color=#FFA500>I lied. I'm drowning here. The numbers aren't making sense anymore.</color>");
                AddLineToHistory("<color=#FFA500>If I fail this, what happens to everything else?</color>");
                AddLineToHistory("<color=#FFA500>-----------------------------</color>");
                break;

            default:
                AddLineToHistory("Command not recognized: '" + command + "'. Type 'help'.");
                break;
        }
    }

    void AddLineToHistory(string line)
    {
        lineHistory.Add(line);
        
        // Keep the terminal from getting too cluttered
        if (lineHistory.Count > maxLines)
        {
            lineHistory.RemoveAt(0);
        }

        // Update the actual UI text
        terminalHistoryText.text = string.Join("\n", lineHistory);
    }

    void ClearTerminal()
    {
        lineHistory.Clear();
        terminalHistoryText.text = "";
    }
}