using UnityEngine;

[CreateAssetMenu(fileName = "StringTable", menuName = "Simple Dialogue/String Table")]
public class StringTable : ScriptableObject
{
    [System.Serializable]
    public class StringEntry
    {
        public string key;
        [TextArea(1, 5)]
        public string value;
    }
    
    public StringEntry[] entries;
    
    public string GetLocalizedString(string key)
    {
        if (entries == null) return null;
        
        foreach (var entry in entries)
        {
            if (entry.key == key)
                return entry.value;
        }
        return null;
    }
}