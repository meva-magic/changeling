using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string requiredItemID;

    [Header("Localization")]
    [SerializeField] private LocalizedStringTable stringTable;
    [SerializeField] private string descriptionKey;
    [SerializeField] private string nameKey;

    public string GetDescription()
    {
        if (stringTable == null || string.IsNullOrEmpty(descriptionKey)) return "";
        var table = stringTable.GetTable();
        if (table == null) return "";
        var entry = table[descriptionKey];
        return entry != null ? entry.LocalizedValue : "";
    }

    public string GetLocalizedName()
    {
        if (stringTable == null || string.IsNullOrEmpty(nameKey)) return questName;
        var table = stringTable.GetTable();
        if (table == null) return questName;
        var entry = table[nameKey];
        return entry != null ? entry.LocalizedValue : questName;
    }
}