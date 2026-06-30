using UnityEngine;
using VContainer;

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
            var rootScope = FindObjectOfType<RootScope>();
            if (rootScope != null)
            {
                var sessionContext = rootScope.Container.Resolve<BattleSessionContext>();
                if (sessionContext != null && sessionContext.ReturnCameraPosition.HasValue)
                {
                    transform.position = sessionContext.ReturnCameraPosition.Value;
                    sessionContext.ReturnCameraPosition = null;
                }
            }

            offset = protagonistTransformAnchor.Value.position - transform.position;
            Debug.Log("Player tranform: " + protagonistTransformAnchor.Value.position);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (protagonistTransformAnchor.isSet)
        {
            transform.position = protagonistTransformAnchor.Value.position - offset;
        }
    }


}
