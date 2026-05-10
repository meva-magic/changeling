using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Simple Dialogue/Dialogue")]
public class SimpleDialogue : ScriptableObject
{
    [Header("Localization")]
    [SerializeField] private LocalizedStringTable stringTable;
    [SerializeField] private string[] lineKeys;
    
    public SimpleDialogue nextDialogue;
    
    [Header("Quest Settings")]
    public bool givesQuest;
    public SimpleQuest questToGive;
    public bool completesQuest;
    public SimpleQuest questToComplete;
    
    [Header("Audio")]
    public string voiceSoundName = "";
    
    public string GetLine(int index)
    {
        if (lineKeys == null || index >= lineKeys.Length)
            return "";
        
        if (stringTable == null)
        {
            Debug.LogWarning($"String Table not assigned on '{name}'");
            return $"[No Table: {lineKeys[index]}]";
        }
        
        var table = stringTable.GetTable();
        if (table == null)
        {
            Debug.LogWarning($"Could not load table for '{name}'");
            return $"[Table Error: {lineKeys[index]}]";
        }
        
        var entry = table[lineKeys[index]];
        if (entry == null)
        {
            Debug.LogWarning($"Key '{lineKeys[index]}' not found");
            return $"[Missing: {lineKeys[index]}]";
        }
        
        return entry.LocalizedValue;
    }
    
    public int GetLineCount()
    {
        return lineKeys != null ? lineKeys.Length : 0;
    }
}