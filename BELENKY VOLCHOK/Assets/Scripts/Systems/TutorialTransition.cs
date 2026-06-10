using UnityEngine;

public class TutorialTransition : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainScene";
    
    private bool hasTransitioned;
    
    private void OnEnable()
    {
        EventBus.Listen(GameEvents.QuestFinished, OnQuestFinished);
    }
    
    private void OnDisable()
    {
        EventBus.StopListening(GameEvents.QuestFinished, OnQuestFinished);
    }
    
    private void OnQuestFinished()
    {
        if (hasTransitioned) return;
        hasTransitioned = true;
        
        FadeToBlack.Instance?.FadeOut(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        });
    }
}
