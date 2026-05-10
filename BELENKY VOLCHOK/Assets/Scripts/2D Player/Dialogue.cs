using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Simple Dialogue/Dialogue")]
public class SimpleDialogue : ScriptableObject
{
    [Header("Localization")]
    [SerializeField] private LocalizedStringTable stringTable;
    [SerializeField] private string[] lineKeys;  // Table entry keys
    
    public SimpleDialogue nextDialogue;
    public bool givesQuest;
    public string questID;
    public bool completesQuest;
    public string completeQuestID;
    public string voiceSoundName = "";
    
    public string GetLine(int index)
    {
        if (lineKeys == null || index >= lineKeys.Length)
            return "";
        
        if (stringTable == null)
        {
            Debug.LogWarning($"String Table not assigned on dialogue '{name}'");
            return $"[Missing Table: {lineKeys[index]}]";
        }
        
        var table = stringTable.GetTable();
        if (table == null)
        {
            Debug.LogWarning($"Could not load string table for dialogue '{name}'");
            return $"[Table Not Loaded: {lineKeys[index]}]";
        }
        
        var entry = table[lineKeys[index]];
        if (entry == null)
        {
            Debug.LogWarning($"Key '{lineKeys[index]}' not found in string table");
            return $"[Missing Key: {lineKeys[index]}]";
        }
        
        return entry.LocalizedValue;
    }
    
    public int GetLineCount()
    {
        return lineKeys != null ? lineKeys.Length : 0;
    }
}