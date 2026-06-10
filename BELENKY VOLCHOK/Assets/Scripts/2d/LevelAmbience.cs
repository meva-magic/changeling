using UnityEngine;

public class LevelAmbience : MonoBehaviour
{
    [SerializeField] private string ambienceSound = "";
    [SerializeField] private string stopConditionItemID = "Changeling";
    [SerializeField] private int stopWhenPlayerInRoom = 1;

    private bool stopped;

    private void Start()
    {
        if (!string.IsNullOrEmpty(ambienceSound) && AudioManager.instance != null)
            AudioManager.instance.Play(ambienceSound);
    }

    private void Update()
    {
        if (stopped) return;

        if (IsChangelingActive() && IsPlayerInRoom(stopWhenPlayerInRoom))
        {
            if (!string.IsNullOrEmpty(ambienceSound) && AudioManager.instance != null)
                AudioManager.instance.Stop(ambienceSound);
            stopped = true;
        }
    }

    private bool IsChangelingActive()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject obj in items)
        {
            PickupableItem item = obj.GetComponent<PickupableItem>();
            if (item != null && item.itemID == stopConditionItemID && obj.activeInHierarchy)
                return true;
        }
        return false;
    }

    private bool IsPlayerInRoom(int roomIndex)
    {
        Room[] rooms = FindObjectsOfType<Room>();
        if (roomIndex < 0 || roomIndex >= rooms.Length) return false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        float distance = Vector3.Distance(player.transform.position, rooms[roomIndex].transform.position);
        return distance < 10f;
    }
}
