using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] TerminalUI terminal;

    [Header("Music")]
    [SerializeField] AudioSource bgm;
    [SerializeField] AudioClip bgmClip;
    [SerializeField] float bgmVolume = 0.15f;

    [Serializable]
    public class Case
    {
        public string studentName;
        public string prompt;
        public string surface;
        public bool correct;
        public List<string> digLayers = new();
        public string fragment;
        public List<string> askLines = new();
    }

    List<Case> cases;
    int caseIdx, digIdx;
    int rep, crack;
    int asks, extraDigs;
    bool signalOn, heartUnlocked;
    bool[] captured = new bool[4];
    int capturedCount;
    List<int> shuffledDigOrder;
    List<int> shuffledAskOrder;

    readonly string[] b64 = { "aHR0cHM6L", "y9zaG9ydH", "VybC5hdC9", "VYVpVcg==" };

    // random approve/reject responses — player can't tell if they were right
    readonly string[] approveLines =
    {
        "Decision logged.",
        "Noted. Case closed.",
        "Approved. Moving on.",
        "The system accepts your judgment.",
        "Filed. Next case loading.",
        "Acknowledged."
    };
    readonly string[] rejectLines =
    {
        "Decision logged.",
        "Noted. Case escalated.",
        "Rejected. File flagged.",
        "The system accepts your judgment.",
        "Filed. Next case loading.",
        "Acknowledged."
    };

    void Awake()
    {
        cases = new List<Case>
        {
            new()
            {
                studentName = "Linh Tran",
                prompt = "Flagged for academic dishonesty. Essay quality jumped three grade levels between drafts.",
                surface = "I rewrote it. I kept rewriting until it stopped sounding like me.",
                correct = true,
                digLayers = new()
                {
                    "Revision history shows 38 deletions in under a minute. All personal sentences removed.",
                    "Deleted line: 'If I fail this, I stop existing at home.'",
                    "Final draft contains zero first-person pronouns. She wrote herself out.",
                    "File was flagged for review at 02:41 AM — but not by any teacher.",
                    "The flagging account has no name. Just a workstation number."
                },
                fragment = "Someone flagged her file in the middle of the night. Who reviews essays at 02:41?",
                askLines = new()
                {
                    "I didn't copy anything. I just edited until there was nothing left to question.",
                    "Can we stop here? If I sound fine, I am fine.",
                    "Don't call my parents. They'll say I finally did something right.",
                    "I deleted the parts that were real. Is that cheating?",
                    "The words were mine. I just made them sound like someone else's."
                }
            },
            new()
            {
                studentName = "Minh Nguyen",
                prompt = "Extension request with attached proof of device failure. Flagged because proof image is recycled.",
                surface = "My laptop died during submission. I can still fix this if you give me one night.",
                correct = false,
                digLayers = new()
                {
                    "The 'proof' screenshot matches an image uploaded to a tech forum in 2021.",
                    "His project files were being edited during the exact window he claimed the blackout.",
                    "Unsent email draft: 'If I lose this class I lose my scholarship. Then I go home to nothing.'",
                    "His grade file was opened from workstation #7 at 02:41 AM. Workstation #7 is yours.",
                    "Access log shows the file was viewed for 4 minutes. No changes were saved — that time."
                },
                fragment = "Your workstation accessed his file. You don't remember doing that.",
                askLines = new()
                {
                    "Yeah, the photo was old. I panicked. Wouldn't you?",
                    "Fail me if you want. Just don't send it higher.",
                    "Someone opened my file that night. It wasn't me. Was it you?",
                    "I know what I did. But someone else was in my records too.",
                    "You're asking me to be honest while sitting at that workstation."
                }
            },
            new()
            {
                studentName = "Sara Le",
                prompt = "Attendance tampering. One student marked present with no door scan, no camera record, no physical trace.",
                surface = "Scanners glitch all the time. I just fixed what looked like an error.",
                correct = false,
                digLayers = new()
                {
                    "Building access logs show zero physical entry for the marked student.",
                    "The attendance edit was made remotely — using reviewer credentials, not student credentials.",
                    "The credentials used were yours. Login from workstation #7.",
                    "Edit timestamp: 02:41 AM. Same window as Case 1 flag and Case 2 file access.",
                    "The student who was marked present had a failing grade. The grade was changed the same night."
                },
                fragment = "Three cases. Same timestamp. Same workstation. All roads lead back to your chair.",
                askLines = new()
                {
                    "I thought it was a glitch. I was trying to help.",
                    "If the record was changed from your terminal... why am I the one sitting here?",
                    "You look like someone who's starting to remember something.",
                    "I didn't touch anything. But you already know that.",
                    "Ask the system who was logged in. You won't like the answer."
                }
            },
            new()
            {
                studentName = "Unknown",
                prompt = "Recovered draft queued under your reviewer ID. No student account is attached. The system filed it as a case automatically.",
                surface = "[system note] Author field: empty. Workstation origin: #7. Time: 02:41 AM.",
                correct = true,
                digLayers = new()
                {
                    "First line of the draft: 'There was a student I couldn't save through normal channels.'",
                    "Author metadata resolves to your workstation login.",
                    "The draft describes overriding a grade to keep a student enrolled. The grade was changed at 02:41 AM.",
                    "Final line: 'I don't know if I helped them or if I just needed to feel like I could.'",
                    "Attached note: 'If you're reading this, you're the one who wrote it. Try to remember.'"
                },
                fragment = "You wrote this. You don't remember writing it. But the system does.",
                askLines = new()
                {
                    "...",
                    "This was never a student submission. You know that.",
                    "Approve or reject. Either way, the system already has your answer from last night.",
                    "You can't ask a file a question. But you're still trying.",
                    "The author is you. The reviewer is you. What happens when both agree?"
                }
            }
        };
    }

    void Start()
    {
        if (!terminal) return;

        // music
        if (bgm && bgmClip)
        {
            bgm.clip = bgmClip;
            bgm.loop = true;
            bgm.volume = bgmVolume;
            bgm.Play();
        }
        else if (bgm)
        {
            // if no clip assigned but AudioSource exists, try Resources folder
            var clip = Resources.Load<AudioClip>("bgm");
            if (clip)
            {
                bgm.clip = clip;
                bgm.loop = true;
                bgm.volume = bgmVolume;
                bgm.Play();
            }
        }

        terminal.Bind(HandleCommand);
        terminal.PrintLine("<color=#666666>INTEGRITY REVIEW TERMINAL v0.9</color>");
        terminal.PrintLine("<color=#666666>Shift: NIGHT / Reviewer: [REDACTED] / Workstation: #7</color>");
        terminal.PrintLine("");
        terminal.PrintLine("Four cases queued for review.");
        terminal.PrintLine("Process each case. Approve or reject. Dig if something feels wrong.");
        terminal.PrintLine("Type 'help' for commands.");
        terminal.PrintLine("<color=#333333>// the archive channel has been noisy tonight</color>");
        ShowCase();
    }

    void ShowCase()
    {
        if (caseIdx >= cases.Count) { PrintEnding(); return; }

        digIdx = 0;
        asks = 0;
        extraDigs = 0;
        var c = cases[caseIdx];

        // shuffle dig and ask order each case so replays feel different
        shuffledDigOrder = ShuffleRange(c.digLayers.Count);
        shuffledAskOrder = ShuffleRange(c.askLines.Count);

        terminal.PrintLine("");

        if (caseIdx == 1)
            terminal.PrintLine("<color=#555555>System note: reviewer pulse elevated during previous case. Logging.</color>");
        else if (caseIdx == 2)
            terminal.PrintLine("<color=#555555>System note: workstation #7 appears in case records. Cross-referencing.</color>");
        else if (caseIdx == 3)
        {
            terminal.PrintLine("<color=#555555>System note: credential match confirmed across Cases 1-3. Loading final entry.</color>");
            terminal.PrintLine("<color=#444444>This case is different. You'll understand why.</color>");
        }

        terminal.PrintLine($"--- CASE {caseIdx + 1}/4 ---");
        terminal.PrintLine("Subject: " + c.studentName);
        terminal.PrintLine(c.prompt);
        terminal.PrintLine("");
        terminal.PrintLine("Statement: \"" + c.surface + "\"");
        terminal.PrintLine("");
        terminal.PrintLine("approve | reject | dig | ask | status");
        Stats();
    }

    void HandleCommand(string cmd)
    {
        cmd = cmd.ToLower();

        if (cmd == "help")
        {
            terminal.PrintLine("approve  — accept the student's explanation");
            terminal.PrintLine("reject   — flag for disciplinary action");
            terminal.PrintLine("dig      — pull another layer from the file");
            terminal.PrintLine("ask      — hear what the student says");
            terminal.PrintLine("status   — current stats");
            terminal.PrintLine("<color=#444444>some commands aren't listed.</color>");
            return;
        }

        if (cmd == "<3")
        {
            heartUnlocked = true;
            terminal.PrintLine("<color=#888888>The terminal warms slightly. Command unlocked: show</color>");
            return;
        }

        if (cmd == "show" && heartUnlocked)
        {
            terminal.PrintLine("intercepted signal fragments:");
            for (int i = 0; i < b64.Length; i++)
            {
                string s = captured[i] ? "CAPTURED" : "---";
                string val = captured[i] ? b64[i] : "?????????";
                terminal.PrintLine($"  [{i + 1}/4] {s}  {val}");
            }
            terminal.PrintLine(capturedCount + "/4 recovered. Decode with base64 when complete.");
            return;
        }

        if (cmd == "status")
        {
            terminal.PrintLine("Reputation: " + rep + "  |  Crack depth: " + crack);
            if (capturedCount > 0)
                terminal.PrintLine("Signal fragments: " + capturedCount + "/4");
            SignalPulse(0.15f);
            return;
        }

        if (caseIdx >= cases.Count) return;
        var c = cases[caseIdx];

        switch (cmd)
        {
            case "approve":
            {
                int d = c.correct ? 3 : -1;
                rep += d;

                // case 4 special line, otherwise random neutral response
                if (caseIdx == 3)
                    terminal.PrintLine("You approved your own file. The system accepts this.");
                else
                    terminal.PrintLine(Pick(approveLines));

                OverthinkCheck();
                Stats();
                SignalPulse(0.2f);
                caseIdx++;
                ShowCase();
                break;
            }

            case "reject":
            {
                int d = !c.correct ? 3 : -1;
                rep += d;

                if (caseIdx == 3)
                    terminal.PrintLine("You rejected your own file. The system accepts this too.");
                else
                    terminal.PrintLine(Pick(rejectLines));

                OverthinkCheck();
                Stats();
                SignalPulse(0.2f);
                caseIdx++;
                ShowCase();
                break;
            }

            case "ask":
            {
                asks++;

                // pick from shuffled ask lines so each playthrough gives different order
                if (asks <= 3 && shuffledAskOrder.Count > 0)
                {
                    int pick = shuffledAskOrder[(asks - 1) % shuffledAskOrder.Count];
                    terminal.PrintLine("\"" + c.askLines[pick] + "\"");
                }

                if (asks > 1) crack++;

                if (asks > 3)
                {
                    if (caseIdx == 3)
                        terminal.PrintLine("<color=#666666>There's no one on the other end. This file is yours.</color>");
                    else
                    {
                        string[] exhaust =
                        {
                            c.studentName + " has nothing more to say.",
                            c.studentName + " stares at the screen.",
                            c.studentName + ": \"I already answered that.\"",
                            "Silence.",
                            c.studentName + " looks away."
                        };
                        terminal.PrintLine(Pick(exhaust));
                    }

                    if (!heartUnlocked && UnityEngine.Random.value < 0.4f)
                        terminal.PrintTransientLine("<color=#555555>the terminal responds to affection, not commands</color>", 1f);
                }

                Stats();
                SignalPulse(0.3f);
                break;
            }

            case "dig":
            {
                if (digIdx < c.digLayers.Count)
                {
                    crack++;
                    int realIdx = shuffledDigOrder[digIdx];
                    terminal.PrintLine(c.digLayers[realIdx]);
                    terminal.TriggerFlicker(0.06f);
                    digIdx++;

                    if (crack % 2 == 0)
                        terminal.PrintLine("<color=#777777>" + c.fragment + "</color>");

                    if (crack >= 3 && !signalOn)
                    {
                        signalOn = true;
                        terminal.PrintLine("<color=#666666>An old signal is bleeding through the archive channel.</color>");
                        terminal.PrintLine("<color=#555555>It seems to respond to warmth.</color>");
                    }

                    if (crack >= 5) HintHeart();
                    if (signalOn && crack % 3 == 0) SignalPulse(1f);
                    Stats();
                    SignalPulse(0.55f);
                }
                else
                {
                    extraDigs++;
                    string[] emptyDig =
                    {
                        "Nothing left in this file.",
                        "You've already seen everything here.",
                        "The file is empty now.",
                        "Digging in circles.",
                        "No new layers."
                    };
                    terminal.PrintLine(Pick(emptyDig));

                    if (extraDigs >= 3)
                    {
                        rep--;
                        terminal.PrintLine("<color=#888888>Compulsive searching noted. Reputation -1.</color>");
                    }
                    Stats();
                    SignalPulse(0.4f);
                }
                break;
            }

            default:
                terminal.PrintLine("Unrecognized input.");
                break;
        }
    }

    void SignalPulse(float chance)
    {
        if (!signalOn || UnityEngine.Random.value > chance) return;

        int idx = UnityEngine.Random.Range(0, b64.Length);
        float dur = UnityEngine.Random.Range(0.5f, 0.6f);
        terminal.PrintTransientLine("[SIGNAL] " + b64[idx], dur);
        terminal.TriggerFlicker(0.1f);

        if (UnityEngine.Random.value < 0.3f) Flashback();

        if (heartUnlocked && !captured[idx])
        {
            captured[idx] = true;
            capturedCount++;
        }
    }

    void HintHeart()
    {
        if (heartUnlocked || UnityEngine.Random.value > 0.35f) return;

        string[] hints =
        {
            "<  3",
            "unrecognized input: <3 ... really?",
            "love is a valid command somewhere",
            "try typing what you feel",
            "< + 3 = ?"
        };
        terminal.PrintTransientLine(
            "<color=#555555>" + Pick(hints) + "</color>",
            UnityEngine.Random.Range(0.8f, 1.2f)
        );
    }

    void Flashback()
    {
        string[] lines =
        {
            "<color=#444444>02:41:09 — monitor glow, no keyboard input</color>",
            "<color=#444444>02:41:10 — cursor moves to GRADE_OVERRIDE on its own</color>",
            "<color=#444444>02:41:10 — user: [YOUR ID] / action: ACCEPT</color>",
            "<color=#444444>02:41:11 — the building was empty. who was sitting here?</color>",
            "<color=#444444>02:41:11 — memory file: corrupted</color>"
        };
        float dur = UnityEngine.Random.Range(0.5f, 0.6f);
        int count = UnityEngine.Random.Range(2, 4);
        for (int i = 0; i < count; i++)
            terminal.PrintTransientLine(lines[i], dur);
    }

    void PrintEnding()
    {
        terminal.PrintLine("");
        terminal.PrintLine("========================================");
        terminal.PrintLine("  SHIFT COMPLETE");
        terminal.PrintLine("========================================");
        terminal.PrintLine("");

        if (rep >= 6 && crack <= 3)
        {
            terminal.PrintLine("You processed every case by the book.");
            terminal.PrintLine("Clean decisions. No unnecessary digging. Efficient.");
            terminal.PrintLine("");
            terminal.PrintLine("The system marks your shift as COMPLETE.");
            terminal.PrintLine("Your workstation powers down.");
            terminal.PrintLine("");
            terminal.PrintLine("But the draft from Case 4 is still in the queue.");
            terminal.PrintLine("It will be here again tomorrow night.");
            terminal.PrintLine("And so will you.");
            terminal.PrintLine("");
            terminal.PrintLine("<color=#555555>You did everything right. That's the problem.</color>");
        }
        else if (rep >= 3 && crack < 8)
        {
            terminal.PrintLine("You noticed the pattern. The timestamps. The workstation number.");
            terminal.PrintLine("But you stopped before the last layer.");
            terminal.PrintLine("");
            terminal.PrintLine("The system marks your shift as INCOMPLETE.");
            terminal.PrintLine("Building lights shut off floor by floor.");
            terminal.PrintLine("Your monitor stays on.");
            terminal.PrintLine("");
            terminal.PrintLine("At 02:41, it displays one line:");
            terminal.PrintLine("'You were almost ready to remember.'");
            terminal.PrintLine("");
            terminal.PrintLine("<color=#555555>Half-truths are heavier than lies.</color>");
        }
        else if (crack >= 8)
        {
            terminal.PrintLine("You pulled every thread until the cases unraveled into one story.");
            terminal.PrintLine("Your story.");
            terminal.PrintLine("");
            terminal.PrintLine("At 02:41 AM, you overrode a failing grade.");
            terminal.PrintLine("A student who would have been expelled stayed enrolled.");
            terminal.PrintLine("You didn't tell anyone. You didn't log it properly.");
            terminal.PrintLine("You wrote a draft about it and forgot you wrote it.");
            terminal.PrintLine("");
            terminal.PrintLine("The system found everything.");
            terminal.PrintLine("It queued the cases in order so you'd find it too.");
            terminal.PrintLine("");
            terminal.PrintLine("Final prompt:");
            terminal.PrintLine("'Now that you remember — was it the right thing to do?'");
            terminal.PrintLine("");
            terminal.PrintLine("<color=#555555>The terminal waits. It will wait as long as you need.</color>");
        }
        else
        {
            terminal.PrintLine("Your decisions were inconsistent. Your digging was shallow.");
            terminal.PrintLine("The system can't tell if you're careless or protecting something.");
            terminal.PrintLine("");
            terminal.PrintLine("Shift status: INCONCLUSIVE.");
            terminal.PrintLine("You've been reassigned to permanent night review.");
            terminal.PrintLine("");
            terminal.PrintLine("Before logout, one line appears on screen:");
            terminal.PrintLine("'See you at 02:41.'");
            terminal.PrintLine("");
            terminal.PrintLine("<color=#555555>Some people avoid the truth by never sitting still long enough to find it.</color>");
        }

        terminal.PrintLine("");
        terminal.PrintLine("Final stats — Reputation: " + rep + " / Crack: " + crack);
        if (capturedCount > 0)
            terminal.PrintLine("Signal fragments recovered: " + capturedCount + "/4");
    }

    void OverthinkCheck()
    {
        if (asks + extraDigs >= 6)
        {
            rep--;
            terminal.PrintLine("<color=#888888>Excessive probing noted. Reputation -1.</color>");
        }
    }

    void Stats()
    {
        string s = "REP " + rep + " | CRACK " + crack;
        if (capturedCount > 0) s += " | SIGNAL " + capturedCount + "/4";
        terminal.PrintLine("<color=#666666>" + s + "</color>");
    }

    // shuffle indices [0..count) — keeps last 2 items at the end (story-critical dig layers)
    List<int> ShuffleRange(int count)
    {
        if (count <= 2) return new List<int> { 0, 1 };

        var early = new List<int>();
        for (int i = 0; i < count - 2; i++) early.Add(i);

        // fisher-yates on early portion
        for (int i = early.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (early[i], early[j]) = (early[j], early[i]);
        }

        // pin last 2 at the end
        early.Add(count - 2);
        early.Add(count - 1);
        return early;
    }

    string Pick(string[] arr) => arr[UnityEngine.Random.Range(0, arr.Length)];
}
