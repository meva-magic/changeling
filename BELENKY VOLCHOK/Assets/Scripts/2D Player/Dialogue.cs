using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Simple Dialogue/Dialogue")]
public class SimpleDialogue : ScriptableObject
{
    [Header("Direct Text (leave empty if using localization)")]
    [TextArea(3, 10)]
    public string[] dialogueLines;
    
    [Header("Localization (optional)")]
    public bool useLocalization = false;
    public LocalizedStringTable localizedStringTable;
    public string[] localizedLineKeys;
    
    public SimpleDialogue nextDialogue;
    public bool givesQuest;
    public string questID;
    public bool completesQuest;
    public string completeQuestID;
    public string voiceSoundName = "";
    
    public string GetLine(int index)
    {
        // Try localization first
        if (useLocalization && localizedStringTable != null && localizedLineKeys != null)
        {
            if (index < localizedLineKeys.Length)
            {
                var table = localizedStringTable.GetTable();
                if (table != null)
                {
                    var entry = table[localizedLineKeys[index]];
                    if (entry != null)
                        return entry.LocalizedValue;
                }
            }
        }
        
        // Fall back to direct text
        if (dialogueLines != null && index < dialogueLines.Length)
            return dialogueLines[index];
        
        return "";
    }
    
    public int GetLineCount()
    {
        if (useLocalization && localizedLineKeys != null)
            return localizedLineKeys.Length;
        
        if (dialogueLines != null)
            return dialogueLines.Length;
        
        return 0;
    }
}
