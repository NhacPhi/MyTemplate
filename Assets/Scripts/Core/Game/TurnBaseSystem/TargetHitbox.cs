using UnityEngine;
using UnityEngine.EventSystems;

public class TargetHitbox : MonoBehaviour, IPointerClickHandler
{
    private Entity target;
    private void Awake()
    {
        target = gameObject.GetComponentInParent<Entity>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
           if(target != null)
            {
                UIEvent.OnChooseTargetEnemy?.Invoke(target);
                UIEvent.OnExecuteSkill?.Invoke();
            }
        }
    }
}
