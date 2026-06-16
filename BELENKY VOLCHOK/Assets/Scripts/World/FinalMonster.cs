using UnityEngine;

public class FinalMonster : MonoBehaviour, IClickable
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private GameObject outlineTarget;
    [SerializeField] private string dialogueKey = "final_monster_dialogue";
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private float fadeDuration = 2f;
    
    private bool isActivated = false;
    private Outline cachedOutline;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    private void Start()
    {
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (cachedOutline != null)
            cachedOutline.enabled = false;
    }
    
    public void OnInteract()
    {
        if (isActivated) return;
        if (!IsPlayerInRange()) return;
        
        isActivated = true;
        
        if (cachedOutline != null) cachedOutline.enabled = false;
        
        DialogueSystem.Instance.SetBlockPlayerInput(true);
        DialogueSystem.Instance.SetAutoCloseDelay(0);
        DialogueSystem.Instance.ShowDialogue(dialogueKey, () => {
            FadeToBlack.Instance?.FadeOut(() => {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            });
        });
    }
    
    private bool IsPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return true;
        return Vector3.Distance(transform.position, player.transform.position) <= interactionRange;
    }
    
    public string GetPromptKey() { return "final_monster"; }
    public float GetInteractionRange() { return interactionRange; }
    public GameObject GetOutlineTarget() { return EffectiveOutlineTarget; }
}
