using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY;
    [SerializeField] private float fixedY;
    [SerializeField] private bool useClamp;
    [SerializeField] private Vector2 minClamp;
    [SerializeField] private Vector2 maxClamp;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = transform.position;
        if (followX) desiredPosition.x = target.position.x + offset.x;
        if (followY) desiredPosition.y = target.position.y + offset.y;
        desiredPosition.z = offset.z;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (!followY)
            smoothedPosition.y = fixedY;

        if (useClamp)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minClamp.x, maxClamp.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minClamp.y, maxClamp.y);
        }

        transform.position = smoothedPosition;
    }
}
