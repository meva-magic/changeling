using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform carryPoint;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float slowSpeedMultiplier = 0.6f;
    [SerializeField] private string pickupDropSound = "";
    [SerializeField] private GameObject interactionIndicator;

    private GameObject carriedObject;
    private PickupableItem carriedPickupable;
    private PlayerController playerController;
    private float originalMoveSpeed;
    private PickupableItem nearestItem;
    private Rigidbody2D carriedRb;
    private Speaker nearestSpeaker;
    private MonoBehaviour nearestMinigame;

    public bool IsCarryingObject => carriedObject != null;
    public GameObject CarriedObject => carriedObject;
    public Transform CarryPoint => carryPoint;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController != null) originalMoveSpeed = playerController.MoveSpeed;
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
    }

    private void Update()
    {
        FindNearestItem();
        FindNearestSpeaker();
        FindNearestMinigame();
        UpdateIndicator();

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsShowing)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (nearestSpeaker != null)
            {
                nearestSpeaker.Speak();
                return;
            }

            if (nearestMinigame != null)
            {
                StartNearestMinigame();
                return;
            }

            if (IsCarryingObject)
            {
                if (nearestItem != null)
                    SwapItems();
                else
                    DropObject();
                return;
            }

            if (nearestItem != null)
            {
                PickupItem();
            }
        }
    }

    private void FixedUpdate()
    {
        if (carriedObject != null && carriedRb != null)
            carriedRb.MovePosition(carryPoint.position);
    }

    private void PlayPickupDropSound()
    {
        if (!string.IsNullOrEmpty(pickupDropSound) && AudioManager.instance != null)
            AudioManager.instance.Play(pickupDropSound);
    }

    private void FindNearestSpeaker()
    {
        nearestSpeaker = null;
        Speaker[] speakers = FindObjectsOfType<Speaker>();
        foreach (Speaker speaker in speakers)
        {
            if (!speaker.IsPlayerInRange) continue;
            float distance = Vector2.Distance(transform.position, speaker.transform.position);
            if (distance <= 3f)
            {
                nearestSpeaker = speaker;
                break;
            }
        }
    }

    private void FindNearestMinigame()
    {
        nearestMinigame = null;
        float closestDistance = 3f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D hit in hits)
        {
            WellMinigame well = hit.GetComponent<WellMinigame>();
            if (well != null && well.enabled && !well.IsCompleted)
            {
                float dist = Vector2.Distance(transform.position, well.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearestMinigame = well;
                }
            }

            OvenMinigame oven = hit.GetComponent<OvenMinigame>();
            if (oven != null && oven.enabled && !oven.IsCompleted)
            {
                float dist = Vector2.Distance(transform.position, oven.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearestMinigame = oven;
                }
            }
        }
    }

    private void StartNearestMinigame()
    {
        if (nearestMinigame is WellMinigame well)
            well.StartMinigame();
        else if (nearestMinigame is OvenMinigame oven)
            oven.StartMinigame();
    }

    private void FindNearestItem()
    {
        nearestItem = null;
        float closestDistance = Mathf.Infinity;

        GameObject[] itemObjects = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject obj in itemObjects)
        {
            PickupableItem item = obj.GetComponent<PickupableItem>();
            if (item == null) item = obj.GetComponentInChildren<PickupableItem>();
            if (item == null) continue;
            if (item == carriedPickupable || item.IsBeingCarried) continue;

            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance <= item.pickupRange && distance < closestDistance)
            {
                closestDistance = distance;
                nearestItem = item;
            }
        }
    }

    private void UpdateIndicator()
    {
        if (interactionIndicator == null) return;
        bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsShowing;
        bool show = !dialogueActive && (nearestSpeaker != null || nearestMinigame != null || nearestItem != null);
        interactionIndicator.SetActive(show);
    }

    private void PickupItem()
    {
        if (nearestItem == null) return;
        carriedObject = nearestItem.gameObject;
        carriedPickupable = nearestItem;
        carriedRb = carriedObject.GetComponent<Rigidbody2D>();
        nearestItem.OnPickup(carryPoint);
        PlayPickupDropSound();
        if (nearestItem.slowsPlayer && playerController != null)
            playerController.SetMoveSpeed(originalMoveSpeed * slowSpeedMultiplier);
    }

    private void SwapItems()
    {
        Vector3 swapPosition = nearestItem.transform.position;
        carriedPickupable.OnDrop(swapPosition);
        carriedObject = nearestItem.gameObject;
        carriedPickupable = nearestItem;
        carriedRb = carriedObject.GetComponent<Rigidbody2D>();
        nearestItem.OnPickup(carryPoint);
        PlayPickupDropSound();
        if (carriedPickupable.slowsPlayer && playerController != null)
            playerController.SetMoveSpeed(originalMoveSpeed * slowSpeedMultiplier);
        else if (playerController != null)
            playerController.SetMoveSpeed(originalMoveSpeed);
    }

    private void DropObject()
    {
        if (carriedObject == null) return;
        carriedPickupable.OnDrop(dropPoint.position);
        carriedObject = null;
        carriedPickupable = null;
        carriedRb = null;
        PlayPickupDropSound();
        if (playerController != null) playerController.SetMoveSpeed(originalMoveSpeed);
    }
}