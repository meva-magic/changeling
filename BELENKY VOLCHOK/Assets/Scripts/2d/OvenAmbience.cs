using UnityEngine;

public class OvenAmbience : MonoBehaviour
{
    [SerializeField] private string levelAmbience = "";
    [SerializeField] private string babyCry = "";
    [SerializeField] private string ovenAmbience = "";

    private bool ovenActivated;

    private void Start()
    {
        if (!string.IsNullOrEmpty(levelAmbience) && AudioManager.instance != null)
            AudioManager.instance.Play(levelAmbience);
        if (!string.IsNullOrEmpty(babyCry) && AudioManager.instance != null)
            AudioManager.instance.Play(babyCry);
    }

    public void ActivateOvenAmbience()
    {
        if (ovenActivated) return;
        ovenActivated = true;

        if (!string.IsNullOrEmpty(levelAmbience) && AudioManager.instance != null)
            AudioManager.instance.Stop(levelAmbience);
        if (!string.IsNullOrEmpty(ovenAmbience) && AudioManager.instance != null)
            AudioManager.instance.Play(ovenAmbience);
    }

    public void StopAll()
    {
        if (!string.IsNullOrEmpty(levelAmbience) && AudioManager.instance != null)
            AudioManager.instance.Stop(levelAmbience);
        if (!string.IsNullOrEmpty(ovenAmbience) && AudioManager.instance != null)
            AudioManager.instance.Stop(ovenAmbience);
        if (!string.IsNullOrEmpty(babyCry) && AudioManager.instance != null)
            AudioManager.instance.Stop(babyCry);
    }
}
