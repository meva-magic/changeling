using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Localization")]
    [SerializeField] private LocalizedStringTable stringTable;
    [SerializeField] private string[] lineKeys;

    public List<DialogueResponse> responses;
    public bool isRepeating;
    public string voiceSound = "";

    public int GetLineCount()
    {
        return lineKeys != null ? lineKeys.Length : 0;
    }

    public string GetLine(int index)
    {
        if (lineKeys == null || index >= lineKeys.Length) return "";
        if (stringTable == null) return $"[No Table: {lineKeys[index]}]";

        var table = stringTable.GetTable();
        if (table == null) return $"[Table Error: {lineKeys[index]}]";

        var entry = table[lineKeys[index]];
        return entry != null ? entry.LocalizedValue : $"[Missing: {lineKeys[index]}]";
    }

    public bool IsLastNode()
    {
        return responses == null || responses.Count == 0;
    }
}