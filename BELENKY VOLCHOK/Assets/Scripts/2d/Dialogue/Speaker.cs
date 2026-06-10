using UnityEngine;
using System.Collections;

public class Speaker : MonoBehaviour
{
    [SerializeField] private DialogueAsset dialogueAsset;
    [SerializeField] private KeyCode interactKey = KeyCode.Space;
    [SerializeField] private Transform itemDropPoint;

    private bool playerInRange;
    private bool isSpeaking;
    private bool rootNodeCompleted;
    private DialogueNode repeatingNode;

    public bool IsPlayerInRange => playerInRange;

    private void Start()
    {
        repeatingNode = null;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey) && !isSpeaking && !DialogueManager.Instance.IsShowing)
            Speak();
    }

    public void Speak()
    {
        if (DialogueManager.Instance.IsShowing) return;
        isSpeaking = true;

        ResetRepeatingIfNeeded();

        DialogueNode nodeToShow = GetDialogueNode();
        if (nodeToShow != null)
        {
            DialogueManager.Instance.ShowDialogue(nodeToShow);
            StartCoroutine(WaitForDialogueEnd());
        }
        else
        {
            isSpeaking = false;
        }
    }

    private void ResetRepeatingIfNeeded()
    {
        Quest activeQuest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest() : null;

        if (activeQuest != null && QuestManager.Instance.CanCompleteQuest(activeQuest))
        {
            if (repeatingNode == dialogueAsset.questReminderNode)
            {
                repeatingNode = null;
            }
        }
    }

    private DialogueNode GetDialogueNode()
    {
        if (repeatingNode != null)
            return repeatingNode;

        Quest activeQuest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest() : null;

        if (activeQuest == null && rootNodeCompleted && dialogueAsset.postQuestNode != null)
        {
            if (dialogueAsset.postQuestNode.isRepeating)
                repeatingNode = dialogueAsset.postQuestNode;
            return dialogueAsset.postQuestNode;
        }

        if (activeQuest != null)
        {
            if (QuestManager.Instance.CanCompleteQuest(activeQuest) && dialogueAsset.questSuccessNode != null)
                return dialogueAsset.questSuccessNode;

            if (dialogueAsset.questReminderNode != null)
            {
                if (dialogueAsset.questReminderNode.isRepeating)
                    repeatingNode = dialogueAsset.questReminderNode;
                return dialogueAsset.questReminderNode;
            }
        }

        if (!rootNodeCompleted && dialogueAsset.rootNode != null)
            return dialogueAsset.rootNode;

        return null;
    }

    private IEnumerator WaitForDialogueEnd()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsShowing);

        DialogueNode lastNode = DialogueManager.Instance.GetLastNode();

        if (lastNode == dialogueAsset.rootNode)
            rootNodeCompleted = true;

        if (lastNode == dialogueAsset.questSuccessNode)
        {
            ScriptedEvent scriptedEvent = FindObjectOfType<ScriptedEvent>();
            if (scriptedEvent != null)
                scriptedEvent.SwapBabyForChangeling();
        }

        if (lastNode != null && lastNode.isRepeating)
            repeatingNode = lastNode;

        isSpeaking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;

        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        if (item != null && !item.IsBeingCarried) MoveItemOut(item);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        if (item != null && !item.IsBeingCarried) MoveItemOut(item);
    }

    private void MoveItemOut(PickupableItem item)
    {
        if (itemDropPoint != null)
            item.transform.position = itemDropPoint.position;
        else
        {
            Vector3 dir = (item.transform.position - transform.position).normalized;
            if (dir.magnitude < 0.1f) dir = Vector3.right;
            item.transform.position = transform.position + dir * 2f;
        }

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}