using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public DialogueNode rootNode;
    public DialogueNode questSuccessNode;
    public DialogueNode questReminderNode;
    public DialogueNode postQuestNode;
}
