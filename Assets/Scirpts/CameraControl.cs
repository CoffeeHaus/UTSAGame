using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;
    public float zOffset = -10f; // The fixed Z-axis position

    void FixedUpdate()
    {
        if (target == null) return;

        // Only consider the X and Y position of the target
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, zOffset);

        // Interpolate between the current position and the target's position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
