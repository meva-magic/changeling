using UnityEngine;

public interface QuestTracker
{
    void BeginQuest(QuestData quest);
    void RecordCollectedItem(string itemTag);
    void CompleteObjective(string objectiveTag);
    QuestStageDefinition GetCurrentStage();
    bool IsCurrentStageFinished();
    bool IsQuestFinished();
}
