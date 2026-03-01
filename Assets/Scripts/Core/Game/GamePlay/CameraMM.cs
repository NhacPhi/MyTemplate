using UnityEngine;

public class CameraMM : MonoBehaviour
{
    [SerializeField] private TransformAnchor cameraTransformAnchor = default;
    [SerializeField] private TransformAnchor protagonistTransformAnchor = default;

    Vector3 offset;
    private void OnEnable()
    {
        cameraTransformAnchor.Provide(this.transform);
        GameEvent.OnPlayerSpawned += SetupCamera;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerSpawned -= SetupCamera;
    }

    private void SetupCamera()
    {
        if (protagonistTransformAnchor.isSet)
        {
            offset = protagonistTransformAnchor.Value.position - transform.position;
            Debug.Log("Player tranform: " + protagonistTransformAnchor.Value.position);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = protagonistTransformAnchor.Value.position - offset;
    }


}
