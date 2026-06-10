using UnityEngine;

public class ZoneAmbience : MonoBehaviour
{
    public enum SpaceType { Interior, Exterior }
    
    [SerializeField] private SpaceType zoneType;
    [SerializeField] private string ambienceSoundName;
    [SerializeField] private string musicSoundName;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (!string.IsNullOrEmpty(ambienceSoundName))
            AudioManager.instance?.Play(ambienceSoundName);
        
        if (!string.IsNullOrEmpty(musicSoundName))
            AudioManager.instance?.Play(musicSoundName);
        
        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetFootstepContext(zoneType == SpaceType.Interior);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (!string.IsNullOrEmpty(ambienceSoundName))
            AudioManager.instance?.Stop(ambienceSoundName);
        
        if (!string.IsNullOrEmpty(musicSoundName))
            AudioManager.instance?.Stop(musicSoundName);
    }
}