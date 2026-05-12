using UnityEngine;
using System.Collections;

public class Zone : MonoBehaviour
{
    [HideInInspector] public bool isTransitioning;
    
    public void TransitionToZone(Zone targetZone, Transform spawnPoint)
    {
        if (isTransitioning) return;
        StartCoroutine(DoTransition(targetZone, spawnPoint));
    }
    
    private IEnumerator DoTransition(Zone targetZone, Transform spawnPoint)
    {
        isTransitioning = true;
        
        if (ZoneCamera.Instance != null)
            yield return ZoneCamera.Instance.FadeToBlack();
        else
            yield return new WaitForSeconds(0.5f);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : targetZone.transform.position;
            player.transform.position = spawnPos;
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = Vector2.zero;
            
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.InputBlocked = true;
        }
        
        Camera.main.transform.position = new Vector3(
            targetZone.transform.position.x,
            targetZone.transform.position.y,
            Camera.main.transform.position.z
        );
        
        if (ZoneCamera.Instance != null)
            yield return ZoneCamera.Instance.FadeFromBlack();
        
        GameObject playerAfter = GameObject.FindGameObjectWithTag("Player");
        if (playerAfter != null)
        {
            PlayerMovement movement = playerAfter.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.InputBlocked = false;
        }
        
        isTransitioning = false;
    }
}