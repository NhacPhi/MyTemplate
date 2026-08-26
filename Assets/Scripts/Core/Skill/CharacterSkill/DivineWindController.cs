using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineWindController : MonoBehaviour
{
    private Entity _caster;
    private Entity _specificTarget;
    private IImpactSkill _skillHandler;
    private Vector3 _flyDirection;
    [SerializeField] private float _speed = 15f;
    private bool _hasHit = false;

    [SerializeField] private GameObject elipEffect;

    private GameObject effectPrefab;
    private void Awake()
    {
        if (elipEffect != null)
        {
            effectPrefab = Instantiate(elipEffect, Vector3.zero, Quaternion.identity);
            effectPrefab.gameObject.SetActive(false);
        }
    }

    public void Initialize(Entity caster, IImpactSkill skill, Vector3 direction, Entity specificTarget = null)
    {
        _caster = caster;
        _skillHandler = skill;
        _flyDirection = direction.normalized;
        _specificTarget = specificTarget != null ? specificTarget : (_caster != null && _caster.Target != null ? _caster.Target.GetComponent<Entity>() : null);
        _hasHit = false;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (_caster == null || _hasHit) return;

        float stepDistance = _speed * Time.deltaTime;
        Vector3 origin = transform.position;

        float radius = 0.5f;
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        GameObject targetGo = _specificTarget != null ? _specificTarget.gameObject : _caster.Target;

        RaycastHit hit;
        if (Physics.SphereCast(origin, radius, _flyDirection, out hit, stepDistance))
        {
            Collider other = hit.collider;
            if (other != null && other.gameObject == targetGo)
            {
                Entity target = other.GetComponent<Entity>();
                HandleHit(target != null ? target : _specificTarget, hit.point);
                return;
            }
        }

        // Kiểm tra khoảng cách gần mục tiêu nếu đã bay sát hoặc tới đích
        if (_specificTarget != null)
        {
            if (Vector3.Distance(transform.position, _specificTarget.transform.position) < 0.8f)
            {
                HandleHit(_specificTarget, transform.position);
                return;
            }
        }

        transform.Translate(_flyDirection * stepDistance, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_caster == null || _hasHit) return;

        if (other.gameObject == _caster.gameObject) return;
        GameObject targetGo = _specificTarget != null ? _specificTarget.gameObject : _caster.Target;
        if (other.gameObject == targetGo)
        {
            Entity target = other.GetComponent<Entity>();
            HandleHit(target != null ? target : _specificTarget, transform.position);
        }
    }

    private void HandleHit(Entity target, Vector3 contactPoint)
    {
        if (_hasHit) return;
        _hasHit = true;

        if (_skillHandler != null)
        {
            _skillHandler.OnProjectileImpact(target, contactPoint);
        }

        if (effectPrefab != null && target != null)
        {
            effectPrefab.transform.position = target.transform.position;
            effectPrefab.gameObject.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
