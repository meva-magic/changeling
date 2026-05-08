using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName) { StartCoroutine(LoadSceneRoutine(sceneName)); }
    
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        ZoneCamera zoneCamera = ZoneCamera.Instance;
        if (zoneCamera == null) zoneCamera = FindObjectOfType<ZoneCamera>();
        
        if (zoneCamera != null)
            yield return zoneCamera.FadeToBlack();
        else
            yield return new WaitForSeconds(0.5f);
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}