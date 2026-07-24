using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballController : MonoBehaviour
{
    [SerializeField] private GameObject explosionVFX;

    //Runtion variables
    private Entity _caster;
    private IImpactSkill _skillHandler;
    private Vector3 _flyDirection;
    [SerializeField] private float _speed;
    private bool _hasHit = false;
    private GameObject explosionObj;

    [SerializeField] private AudioDataConfig _audioTrigerrDetect;
    private void Awake()
    {
        explosionObj = Instantiate(explosionVFX, Vector3.zero, Quaternion.identity);
        explosionObj.gameObject.SetActive(false);
    }
    public void Initialize(Entity caster, IImpactSkill skill, Vector3 direction)
    {
        _caster = caster;
        _skillHandler = skill;
        _flyDirection = direction.normalized;
        _hasHit = false;
        explosionObj.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_caster == null || _hasHit) return;

        float stepDistance = _speed * Time.deltaTime;
        Vector3 origin = transform.position;

        // Lấy bán kính thực tế từ SphereCollider để làm bán kính quét
        float radius = 0.5f;
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        RaycastHit hit;
        // Quét hình cầu dọc theo hướng bay trong frame này để chống đi xuyên qua Collider (tunneling)
        if (Physics.SphereCast(origin, radius, _flyDirection, out hit, stepDistance))
        {
            Collider other = hit.collider;
            if (other.gameObject != _caster.gameObject)
            {
                Entity target = other.GetComponent<Entity>();
                bool isTargetHit = (_caster.Target != null)
                    ? (other.gameObject == _caster.Target || target == _caster.Target)
                    : (target != null && target.Team != _caster.Team);

                if (target != null && isTargetHit)
                {
                    HandleHit(target, hit.point);
                    return;
                }
            }
        }

        transform.Translate(_flyDirection * stepDistance, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_caster == null || _hasHit) return;

        if (other.gameObject == _caster.gameObject) return;

        Entity target = other.GetComponent<Entity>();

        bool isTargetHit = (_caster.Target != null)
            ? (other.gameObject == _caster.Target || target == _caster.Target)
            : (target != null && target.Team != _caster.Team);

        if (target != null && isTargetHit)
        {
            HandleHit(target, transform.position);
        }
    }

    private void HandleHit(Entity target, Vector3 contactPoint)
    {
        _hasHit = true;

        if (_skillHandler != null)
        {
            if (_audioTrigerrDetect != null)
            {
                _caster.PlaySFX(_audioTrigerrDetect.AudioID);
            }
            _skillHandler.OnProjectileImpact(target, contactPoint);
            gameObject.SetActive(false);
        }

        if (target != null)
        {
            explosionObj.transform.position = target.transform.position;
            explosionObj.gameObject.SetActive(true);
        }
    }
}
