using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private TerminalUI terminal;

    [Serializable]
    public class Case
    {
        public string studentName;
        public string prompt;
        public string surfaceSolution;
        public bool isCorrect;
        public List<string> digLayers = new();
        public string memoryFragment;
        public List<string> askLines = new();
    }

    private List<Case> cases;
    private int index = 0;
    private int digIndex = 0;
    private int reputation = 0;
    private int crack = 0;

    private void Awake()
    {
        cases = new List<Case>()
        {
            new Case
            {
                studentName = "Linh Tran",
                prompt = "Perfect philosophy essay. No mistakes. No personality.",
                surfaceSolution = "I just worked harder this time.",
                isCorrect = true,
                digLayers = new List<string>
                {
                    "File created 3 minutes before submission.",
                    "Style matches online academic samples.",
                    "No emotional voice detected.",
                    "Previous works were average."
                },
                memoryFragment = "Perfection used to mean safety.",
                askLines = new List<string>
                {
                    "Why are you suspicious?",
                    "Do you want me to fail?",
                    "I improved. That's all."
                }
            },

            new Case
            {
                studentName = "Minh Nguyen",
                prompt = "Requests deadline extension. Claims laptop screen broke.",
                surfaceSolution = "Here is a photo of the cracked screen.",
                isCorrect = false,
                digLayers = new List<string>
                {
                    "Photo metadata is 2 years old.",
                    "Image found online.",
                    "Assignment edited after the 'incident'.",
                    "Multiple late submissions before."
                },
                memoryFragment = "Small lies feel harmless at first.",
                askLines = new List<string>
                {
                    "You don't trust me?",
                    "It's not my fault.",
                    "Why are you making this hard?"
                }
            }
        };
    }

    private void Start()
    {
        if (terminal == null)
        {
            Debug.LogError("GameController: Terminal reference missing.");
            return;
        }

        terminal.Bind(HandleCommand);
        terminal.PrintLine("Terminal ready. Type 'help'.");
        ShowCase();
    }

    private void ShowCase()
    {
        if (index >= cases.Count)
        {
            terminal.PrintLine("");
            terminal.PrintLine("=== END ===");
            terminal.PrintLine("Final Reputation: " + reputation);
            terminal.PrintLine("Final Crack: " + crack);
            return;
        }

        digIndex = 0;
        var c = cases[index];

        terminal.PrintLine("");
        terminal.PrintLine($"--- Case {index + 1}/{cases.Count} ---");
        terminal.PrintLine("Student: " + c.studentName);
        terminal.PrintLine(c.prompt);
        terminal.PrintLine("");
        terminal.PrintLine("Surface solution:");
        terminal.PrintLine(c.surfaceSolution);
        terminal.PrintLine("");
        terminal.PrintLine("Commands: approve | reject | dig | ask | status");
    }

    private void HandleCommand(string cmd)
    {
        cmd = cmd.ToLower();

        if (cmd == "help")
        {
            terminal.PrintLine("approve  : accept solution");
            terminal.PrintLine("reject   : reject solution");
            terminal.PrintLine("dig      : reveal hidden layer");
            terminal.PrintLine("ask      : student response");
            terminal.PrintLine("status   : show stats");
            return;
        }

        if (cmd == "status")
        {
            terminal.PrintLine("Reputation: " + reputation + " | Crack: " + crack);
            return;
        }

        if (index >= cases.Count)
            return;

        var c = cases[index];

        switch (cmd)
        {
            case "approve":
                reputation += c.isCorrect ? 2 : -1;
                terminal.PrintLine(c.isCorrect ? "Approved." : "Approved... (wrong)");
                NextCase();
                break;

            case "reject":
                reputation += !c.isCorrect ? 2 : -1;
                terminal.PrintLine(!c.isCorrect ? "Rejected." : "Rejected... (wrong)");
                NextCase();
                break;

            case "ask":
                if (c.askLines.Count > 0)
                    terminal.PrintLine(c.studentName + ": " +
                        c.askLines[UnityEngine.Random.Range(0, c.askLines.Count)]);
                break;

            case "dig":
                crack++;
                if (digIndex < c.digLayers.Count)
                {
                    terminal.PrintLine("[DIG] " + c.digLayers[digIndex]);
                    digIndex++;

                    if (crack % 2 == 0)
                        terminal.PrintLine("[MEMORY] " + c.memoryFragment);
                }
                else
                {
                    terminal.PrintLine("Nothing more found.");
                }
                break;

            default:
                terminal.PrintLine("Unknown command.");
                break;
        }
    }

    private void NextCase()
    {
        index++;
        ShowCase();
    }
}