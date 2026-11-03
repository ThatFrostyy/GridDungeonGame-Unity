using UnityEngine;

/// <summary>
/// Keeps the camera centered on the player, with light smoothing.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.1f;

    private Transform target;
    private Vector3 velocity;

    private void Start()
    {
        FindTarget();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null)
            {
                return;
            }
        }

        Vector3 desired = new(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    private void FindTarget()
    {
        if (GameObjectLocator.Instance != null && GameObjectLocator.Instance.Player != null)
        {
            target = GameObjectLocator.Instance.Player.transform;
        }
    }

    private void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}
