using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class DialogueResponse
{
    [Header("Localization")]
    [SerializeField] private LocalizedStringTable stringTable;
    [SerializeField] private string textKey;

    public DialogueNode nextNode;
    public bool givesQuest;
    public Quest questToGive;
    public bool completesQuest;
    public Quest questToComplete;

    public string GetText()
    {
        if (stringTable == null || string.IsNullOrEmpty(textKey)) return "";
        var table = stringTable.GetTable();
        if (table == null) return "";
        var entry = table[textKey];
        return entry != null ? entry.LocalizedValue : "";
    }
}