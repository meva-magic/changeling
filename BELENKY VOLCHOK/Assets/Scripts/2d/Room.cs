using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour
{
    [HideInInspector] public bool isTransitioning;

    public void TransitionTo(Room targetRoom, Transform spawnPoint)
    {
        if (isTransitioning) return;
        StartCoroutine(DoTransition(targetRoom, spawnPoint));
    }

    private IEnumerator DoTransition(Room targetRoom, Transform spawnPoint)
    {
        isTransitioning = true;

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeToBlack();
        else
            yield return new WaitForSeconds(0.5f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPoint != null ? spawnPoint.position : targetRoom.transform.position;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            PlayerController movement = player.GetComponent<PlayerController>();
            if (movement != null) movement.InputBlocked = true;
        }

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeFromBlack();

        GameObject playerAfter = GameObject.FindGameObjectWithTag("Player");
        if (playerAfter != null)
        {
            PlayerController movement = playerAfter.GetComponent<PlayerController>();
            if (movement != null) movement.InputBlocked = false;
        }

        isTransitioning = false;
    }
}