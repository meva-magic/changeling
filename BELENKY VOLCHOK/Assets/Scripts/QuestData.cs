using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string titleKey;
    public QuestStage[] stages;
}

[System.Serializable]
public class QuestStage
{
    public string stageId;
    public string objectiveKey;
    public int requiredAmount;
    public string targetTag;
}