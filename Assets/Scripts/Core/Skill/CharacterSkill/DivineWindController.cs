using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineWindController : MonoBehaviour
{
    private Entity _caster;
    private IImpactSkill _skillHandler;
    private Vector3 _flyDirection;
    [SerializeField] private float _speed;
    private bool _hasHit = false;

    [SerializeField] private GameObject elipEffect;

    private GameObject effectPrefab;
    private void Awake()
    {
        effectPrefab = Instantiate(elipEffect, Vector3.zero, Quaternion.identity);
        effectPrefab.gameObject.SetActive(false);
    }
    public void Initialize(Entity caster, IImpactSkill skill, Vector3 direction)
    {
        _caster = caster;
        _skillHandler = skill;
        _flyDirection = direction.normalized;
        _hasHit = false;
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

        RaycastHit hit;
        if (Physics.SphereCast(origin, radius, _flyDirection, out hit, stepDistance))
        {
            Collider other = hit.collider;
            if (other.gameObject == _caster.Target)
            {
                Entity target = other.GetComponent<Entity>();
                HandleHit(target, hit.point);
                return;
            }
        }

        transform.Translate(_flyDirection * stepDistance, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_caster == null || _hasHit) return;

        if (other.gameObject == _caster.gameObject) return;
        if (other.gameObject == _caster.Target)
        {
            Entity target = other.GetComponent<Entity>();
            HandleHit(target, transform.position);
        }
    }

    private void HandleHit(Entity target, Vector3 contactPoint)
    {
        _hasHit = true;

        if (_skillHandler != null)
        {
            _skillHandler.OnProjectileImpact(target, contactPoint);
            gameObject.SetActive(false);
        }

        if (elipEffect != null)
        {
            effectPrefab.transform.position = target.transform.position;
            effectPrefab.gameObject.SetActive(true);
        }
    }
}
