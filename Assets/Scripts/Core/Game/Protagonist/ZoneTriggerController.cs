using UnityEngine.Events;
using UnityEngine;


public class ZoneTriggerController : MonoBehaviour
{
    [SerializeField] private LayerMask layers = default;

    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & layers) != 0)
        {
            GameEvent.OnTriggerZoneChanged?.Invoke(true, other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer & layers) != 0)
        {
            GameEvent.OnTriggerZoneChanged?.Invoke(false, other.gameObject);
        }
    }
}
