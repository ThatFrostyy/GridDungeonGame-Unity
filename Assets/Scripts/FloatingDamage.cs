using UnityEngine;

public class FloatingDamage : MonoBehaviour
{

    [SerializeField] private float destroyDelay = 1f;
    [SerializeField] private float offset = 0.2f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);
        transform.localPosition += new Vector3(0f, offset, 0f);
    }
}
