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
    private int askCountInCase = 0;
    private int extraDigCountInCase = 0;
    private bool signalUnlocked = false;
    private bool uhhidkUnlocked = false;
    private readonly bool[] capturedMorseParts = new bool[4];
    private int totalCapturedMorseParts = 0;

    private readonly string[] morseSignals =
    {
        "aHR0cHM6",
        "Ly9zaG9y",
        "dHVybC5h",
        "dC9VYVpVcg=="
    };

    private void Awake()
    {
        cases = new List<Case>()
        {
            new Case
            {
                studentName = "Linh Tran",
                prompt = "Essay quality jumps overnight. The writing is perfect, but it reads like someone removed the human part.",
                surfaceSolution = "I rewrote it until it stopped sounding like me. That's how you get full marks, right?",
                isCorrect = true,
                digLayers = new List<string>
                {
                    "Revision history: 38 deletions in 57 seconds.",
                    "Removed sentence: 'If I fail this class, I disappear at home.'",
                    "Final version contains zero first-person pronouns.",
                    "Autosave cluster appears at 02:41 AM."
                },
                memoryFragment = "At 02:41, I learned how easy it is to cut myself out of my own words.",
                askLines = new List<string>
                {
                    "If I sound stable, can this end here?",
                    "I didn't copy. I just edited until I felt nothing.",
                    "Please don't contact my parents. They'll call this a success."
                }
            },

            new Case
            {
                studentName = "Minh Nguyen",
                prompt = "Urgent extension request. Student reports panic attack and device failure during submission window.",
                surfaceSolution = "Please give me one night. I can still fix this before morning.",
                isCorrect = false,
                digLayers = new List<string>
                {
                    "Attached 'proof' image matches a 2021 tech forum upload.",
                    "Project files were actively edited during the claimed blackout.",
                    "Unsent draft: 'If this class drops, I lose my scholarship.'",
                    "Final save timestamp: 02:41 AM."
                },
                memoryFragment = "I called it survival the first time. After that, it became habit.",
                askLines = new List<string>
                {
                    "I know the photo was old. I panicked.",
                    "You can fail me. Just don't escalate this.",
                    "I was online at 02:41. I don't remember opening the file."
                }
            },

            new Case
            {
                studentName = "Sara Le",
                prompt = "Attendance audit: one student marked present despite no entry scan and no camera hit.",
                surfaceSolution = "Scanner errors happen all the time. I corrected what looked wrong.",
                isCorrect = false,
                digLayers = new List<string>
                {
                    "Door logs show no physical entry at any point.",
                    "Attendance was edited remotely using your credentials.",
                    "Correction note was rewritten three times, then anonymized.",
                    "Maintenance window overlaps exactly with 02:41 AM."
                },
                memoryFragment = "I told myself I was helping. I never checked who I was helping.",
                askLines = new List<string>
                {
                    "I thought I was fixing a harmless mistake.",
                    "The cursor moved before my hand touched the mouse.",
                    "If a false record gets signed, does it become truth?"
                }
            },

            new Case
            {
                studentName = "Unknown (Recovered Draft)",
                prompt = "Recovered draft appears in queue under your reviewer ID. No student account claims ownership.",
                surfaceSolution = "System note: submitter unknown.",
                isCorrect = true,
                digLayers = new List<string>
                {
                    "Opening line: 'I learned to score pain faster than assignments.'",
                    "Author metadata points to your workstation name.",
                    "Autosave snapshots exist while your terminal status is idle.",
                    "All snapshots generated at 02:41 AM."
                },
                memoryFragment = "If I keep evaluating everyone else, no one asks where my missing hour went.",
                askLines = new List<string>
                {
                    "...",
                    "This draft was never submitted by any student.",
                    "Reject it if you want. It will reappear tomorrow."
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
        terminal.PrintLine("Integrity Review Terminal v0.9");
        terminal.PrintLine("Night shift loaded. Type 'help'.");
        terminal.PrintLine("Signal noise detected in archive channel.");
        ShowCase();
    }

    private void ShowCase()
    {
        if (index >= cases.Count)
        {
            PrintEnding();
            return;
        }

        digIndex = 0;
        askCountInCase = 0;
        extraDigCountInCase = 0;
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
        PrintQuickStats();
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

        if (cmd == "<3")
        {
            uhhidkUnlocked = true;
            terminal.PrintLine("[uhhidk] hidden command unlocked: show");
            return;
        }

        if (cmd == "show" && uhhidkUnlocked)
        {
            terminal.PrintLine("[uhhidk] base64 captures:");
            for (int partIndex = 0; partIndex < morseSignals.Length; partIndex++)
            {
                string state = capturedMorseParts[partIndex] ? "FOUND" : "MISSING";
                string payload = capturedMorseParts[partIndex]
                    ? morseSignals[partIndex]
                    : "?? ?? ?? ??";
                terminal.PrintLine($"part {partIndex + 1}/4 [{state}] {payload}");
            }
            terminal.PrintLine("[uhhidk] captured: " + totalCapturedMorseParts + "/4");

            return;
        }

        if (cmd == "status")
        {
            terminal.PrintLine("Reputation: " + reputation + " | Crack: " + crack);
            terminal.PrintLine("Morse Found: " + totalCapturedMorseParts + "/4");
            TryRandomSignalPulse(0.15f);
            return;
        }

        if (index >= cases.Count)
            return;

        var c = cases[index];

        switch (cmd)
        {
            case "approve":
                int approveDelta = c.isCorrect ? 3 : -1;
                reputation += approveDelta;
                terminal.PrintLine(c.isCorrect ? "Decision logged: APPROVED." : "Decision logged: APPROVED (inconsistent).");
                terminal.PrintLine("[IMPACT] Reputation " + FormatDelta(approveDelta) + " | Crack +0");
                ApplyOverthinkingPenalty();
                PrintQuickStats();
                TryRandomSignalPulse(0.2f);
                NextCase();
                break;

            case "reject":
                int rejectDelta = !c.isCorrect ? 3 : -1;
                reputation += rejectDelta;
                terminal.PrintLine(!c.isCorrect ? "Decision logged: REJECTED." : "Decision logged: REJECTED (inconsistent).");
                terminal.PrintLine("[IMPACT] Reputation " + FormatDelta(rejectDelta) + " | Crack +0");
                ApplyOverthinkingPenalty();
                PrintQuickStats();
                TryRandomSignalPulse(0.2f);
                NextCase();
                break;

            case "ask":
                askCountInCase++;

                if (askCountInCase <= 2 && c.askLines.Count > 0)
                    terminal.PrintLine(c.studentName + ": " +
                        c.askLines[UnityEngine.Random.Range(0, c.askLines.Count)]);

                int askCrackDelta = askCountInCase > 1 ? 1 : 0;
                if (askCrackDelta > 0)
                {
                    crack++;
                }

                if (askCountInCase > 2)
                    terminal.PrintLine(c.studentName + ": I already told you everything.");

                terminal.PrintLine("[IMPACT] Reputation +0 | Crack " + FormatDelta(askCrackDelta));
                PrintQuickStats();
                TryRandomSignalPulse(0.3f);
                break;

            case "dig":
                if (digIndex < c.digLayers.Count)
                {
                    crack++;
                    terminal.PrintLine("[DIG] " + c.digLayers[digIndex]);
                    digIndex++;
                    terminal.PrintLine("[IMPACT] Reputation +0 | Crack +1");

                    if (crack % 2 == 0)
                        terminal.PrintLine("[MEMORY] " + c.memoryFragment);

                    if (crack >= 3 && !signalUnlocked)
                    {
                        signalUnlocked = true;
                        terminal.PrintLine("[SIGNAL] Unknown channel detected.");
                    }

                    if (signalUnlocked && crack % 3 == 0)
                        TryRandomSignalPulse(1f);

                    PrintQuickStats();
                    TryRandomSignalPulse(0.55f);
                }
                else
                {
                    extraDigCountInCase++;
                    terminal.PrintLine("No additional artifacts found.");
                    terminal.PrintLine("[IMPACT] Reputation +0 | Crack +0");

                    if (extraDigCountInCase >= 3)
                    {
                        reputation -= 1;
                        terminal.PrintLine("[OVERUSE] Repeated blind digging. Reputation -1.");
                    }

                    PrintQuickStats();
                    TryRandomSignalPulse(0.4f);
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

    private void TryRandomSignalPulse(float chance)
    {
        if (!signalUnlocked)
            return;

        if (UnityEngine.Random.value > chance)
            return;

        int clueIndex = UnityEngine.Random.Range(0, morseSignals.Length);
        float duration = UnityEngine.Random.Range(0.5f, 0.6f);
        terminal.PrintTransientLine("[SIGNAL] " + morseSignals[clueIndex], duration);

        if (UnityEngine.Random.value < 0.3f)
            PrintFlashbackBurst();

        if (uhhidkUnlocked && !capturedMorseParts[clueIndex])
        {
            capturedMorseParts[clueIndex] = true;
            totalCapturedMorseParts++;
        }
    }

    private void PrintFlashbackBurst()
    {
        float duration = UnityEngine.Random.Range(0.5f, 0.6f);
        string[] burst =
        {
            "[FLASHBACK] 02:41:09 > monitor glow / no keyboard input",
            "[FLASHBACK] 02:41:10 > cursor moved to GRADING_OVERRIDE",
            "[FLASHBACK] 02:41:10 > user: YOU / action: ACCEPT",
            "[FLASHBACK] 02:41:11 > audio: ... --- ...",
            "[FLASHBACK] 02:41:11 > memory checksum mismatch"
        };

        int linesToShow = UnityEngine.Random.Range(2, 4);
        for (int lineIndex = 0; lineIndex < linesToShow; lineIndex++)
        {
            terminal.PrintTransientLine(burst[lineIndex], duration);
        }
    }

    private void PrintEnding()
    {
        terminal.PrintLine("");
        terminal.PrintLine("=== SHIFT COMPLETE ===");
        terminal.PrintLine("Final Reputation: " + reputation);
        terminal.PrintLine("Final Crack: " + crack);

        if (reputation >= 6 && crack <= 3)
        {
            terminal.PrintLine("You kept the system clean, and still went home uneasy.");
            terminal.PrintLine("At logout, one draft remains open: 'I did everything correctly. Why am I still afraid?' ");
        }
        else if (reputation >= 3)
        {
            terminal.PrintLine("You held the line, but the numbers never settled.");
            terminal.PrintLine("Building lights shut off. Your monitor wakes again at 02:41.");
        }
        else if (crack >= 8)
        {
            terminal.PrintLine("You dug until records and memories became the same file.");
            terminal.PrintLine("A new case appears with no student name. Status: awaiting your statement.");
        }
        else
        {
            terminal.PrintLine("Your decisions are final, but not convincing. Even to you.");
            terminal.PrintLine("Before shutdown, the terminal types one line on its own: 'See you at 02:41.'");
        }
    }

    private string FormatDelta(int value)
    {
        if (value > 0)
            return "+" + value;
        if (value < 0)
            return value.ToString();
        return "+0";
    }

    private void ApplyOverthinkingPenalty()
    {
        int overuse = askCountInCase + extraDigCountInCase;
        if (overuse >= 6)
        {
            reputation -= 1;
            terminal.PrintLine("[OVERTHINK] Too many repeated probes this case. Reputation -1.");
        }
    }

    private void PrintQuickStats()
    {
        terminal.PrintLine("[STATS] REP " + reputation + " | CRACK " + crack + " | MORSE " + totalCapturedMorseParts + "/4");
    }
}