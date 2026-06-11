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
        
        Debug.Log("TutorialTransition: Квест завершён, запускаем затемнение");
        
        if (FadeToBlack.Instance != null)
        {
            FadeToBlack.Instance.FadeOut(() => {
                Debug.Log($"TutorialTransition: Загрузка сцены {nextSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            });
        }
        else
        {
            Debug.LogError("TutorialTransition: FadeToBlack.Instance не найден!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}