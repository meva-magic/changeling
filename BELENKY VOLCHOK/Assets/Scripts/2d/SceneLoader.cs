using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        ScreenFader fader = ScreenFader.Instance;
        if (fader == null) fader = FindObjectOfType<ScreenFader>();

        if (fader != null)
            yield return fader.FadeToBlack();
        else
            yield return new WaitForSeconds(0.5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
