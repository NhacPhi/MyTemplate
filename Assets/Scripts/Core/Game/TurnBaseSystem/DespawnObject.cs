using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnObject : MonoBehaviour
{
    [SerializeField] public float DespawnTime = 2f;
    protected Tween despawnTween;

    protected virtual void OnEnable()
    {
        if (despawnTween != null)
        {
            despawnTween.Restart();
            return;
        }

        despawnTween = DOVirtual.DelayedCall(DespawnTime, () =>
        {
            this.gameObject.SetActive(false);
        }).SetAutoKill(false);
    }

    protected virtual void OnDestroy()
    {
        if (!despawnTween.IsActive()) return;

        despawnTween.Kill();
    }
}
