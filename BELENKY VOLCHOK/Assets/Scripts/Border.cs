using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CircleBoundary : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Vector3 direction = other.transform.position - transform.position;
        if (direction.magnitude > GetComponent<SphereCollider>().radius * transform.localScale.x)
        {
            Vector3 clampedPosition = transform.position + direction.normalized * (GetComponent<SphereCollider>().radius * transform.localScale.x);
            other.transform.position = clampedPosition;
        }
    }
}
}