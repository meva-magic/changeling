using UnityEngine;

public class ItemDropZone : MonoBehaviour
{
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float pushDistance = 2f;

    private void OnTriggerEnter2D(Collider2D other) { MoveItemOut(other); }
    private void OnTriggerStay2D(Collider2D other) { MoveItemOut(other); }

    private void MoveItemOut(Collider2D other)
    {
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        if (item == null || item.IsBeingCarried) return;

        if (dropPoint != null)
            item.transform.position = dropPoint.position;
        else
        {
            Vector3 dir = (item.transform.position - transform.position).normalized;
            if (dir.magnitude < 0.1f) dir = Vector3.right;
            item.transform.position = transform.position + dir * pushDistance;
        }

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }
}
