using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Definition")]
public class QuestData : ScriptableObject
{
    public string Id;
    public QuestStageDefinition[] Stages;
}

[System.Serializable]
public class QuestStageDefinition
{
    public string Id;
    public string ObjectiveKey;
    public int RequiredQuantity;
    public string RequiredTag;
}