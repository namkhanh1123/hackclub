using UnityEngine;

[CreateAssetMenu(menuName = "Campfire/Case Data")]
public class CaseData : ScriptableObject
{
    public string studentName;

    [TextArea(3, 10)] public string prompt;
    [TextArea(3, 10)] public string surfaceSolution;

    public bool isCorrect;

    [Header("Beneath the Surface")]
    [TextArea(2, 10)] public string[] digLayers;
    [TextArea(2, 10)] public string memoryFragment;

    [Header("Dialogue")]
    [TextArea(2, 10)] public string[] askLines;
}