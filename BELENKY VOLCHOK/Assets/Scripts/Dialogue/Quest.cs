using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Simple Quest/Quest")]
public class SimpleQuest : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea(2, 5)]
    public string description;
    public string requiredItemID;
    public string completionScene;
    public float questTimeLimit = 0f;
}