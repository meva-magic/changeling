using UnityEngine;

public class Fireplace : MonoBehaviour, IClickable
{
    [SerializeField] private ParticleSystem flameEffect;
    [SerializeField] private GameObject firewoodVisual;
    [SerializeField] private float clickStrength = 10f;
    [SerializeField] private float decayStrength = 5f;
    [SerializeField] private float activationRange = 3f;
    [SerializeField] private string finishSound = "minigame_fire_spawn";
    
    private bool isBurning;
    private bool isLightingInProgress;
    private Transform playerTransform;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
    
    public void OnInteract()
    {
        if (!IsPlayerInRange()) return;
        
        if (isBurning)
        {
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            ui?.ShowMessage("fire_already_lit");
            return;
        }
        
        QuestTracker quest = ServiceLocator.Get<QuestTracker>();
        if (quest == null) return;
        
        QuestStageDefinition stage = quest.GetCurrentStage();
        bool canLight = stage != null && stage.RequiredTag == "Fireplace";
        
        if (!canLight)
        {
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            ui?.ShowMessage("need_gather_wood");
            return;
        }
        
        BeginLighting();
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        return Vector3.Distance(transform.position, playerTransform.position) <= activationRange;
    }
    
    private void BeginLighting()
    {
        if (isLightingInProgress) return;
        isLightingInProgress = true;
        
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame == null) return;
        
        MinigameConfiguration config = new MinigameConfiguration();
        config.Name = "Fireplace";
        config.ClickPower = clickStrength;
        config.DecayRate = decayStrength;
        config.UseMatchAnimation = true;
        config.SpawnFireOnFinish = true;
        config.LinkedObject = gameObject;
        config.OnFinished = OnLightingComplete;
        config.OnCancelled = OnLightingCancelled;
        
        minigame.StartMinigame(config);
    }
    
    private void OnLightingComplete()
    {
        isLightingInProgress = false;
        isBurning = true;
        
        if (flameEffect != null) flameEffect.Play();
        if (firewoodVisual != null) firewoodVisual.SetActive(true);
        
        if (!string.IsNullOrEmpty(finishSound))
        {
            AudioManager.instance?.Play(finishSound);
        }
        
        QuestTracker quest = ServiceLocator.Get<QuestTracker>();
        quest?.CompleteObjective("Fireplace");
        
        EventBus.Broadcast(GameEvents.FireplaceLit);
    }
    
    private void OnLightingCancelled()
    {
        isLightingInProgress = false;
    }
    
    public string GetPromptKey()
    {
        return "";
    }
}