using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Simple Dialogue/Dialogue")]
public class SimpleDialogue : ScriptableObject
{
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public SimpleDialogue nextDialogue;
    public bool givesQuest;
    public string questID;
    public bool completesQuest;
    public string completeQuestID;
    public string voiceSoundName = "Voice";
}