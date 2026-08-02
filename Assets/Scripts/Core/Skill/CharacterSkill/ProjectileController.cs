using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileState
{
    Outward,
    PauseAtApex,
    Returning
}
public class ProjectileController : MonoBehaviour
{
    private ProjectileState _state;
    private Entity _caster;
    private IReturningProjectileSkill _skillHandler;

    [SerializeField] private AudioDataConfig _audioTrigerrDetect;
    [SerializeField] private float _pauseDuration = 0.15f; // Thời gian khựng nhẹ vật lý ở điểm cực đại (giây)

    private Vector3 _startPosition;
    private Vector3 _throwDirection;
    public float _moveSpeed;
    private float _maxDistance;
    private float _pauseTimer = 0f;

    private bool _hasHitOutward = false;
    private bool _hasHitReturn = false;

    public void Initialize(Entity caster, IReturningProjectileSkill skill, Vector3 direction, float maxDist)
    {
        _caster = caster;
        _skillHandler = skill;

        _throwDirection = direction.normalized;
        _maxDistance = maxDist;

        _startPosition = transform.position;
        _state = ProjectileState.Outward;

        _hasHitOutward = false;
        _hasHitReturn = false;
        _pauseTimer = 0f;
    }

    private void Update()
    {
        switch(_state)
        {
            case ProjectileState.Outward:
                HandleOutwardMovement();
                break;
            case ProjectileState.PauseAtApex:
                HandlePauseAtApex();
                break;
            case ProjectileState.Returning:
                HandleReturnMovement();
                break;
        }
    }

    private void HandleOutwardMovement()
    {
        float stepDistance = _moveSpeed * Time.deltaTime;
        Vector3 origin = transform.position;

        if (!_hasHitOutward && _caster != null && _caster.Target != null)
        {
            float radius = 0.5f;
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            }

            if (Physics.SphereCast(origin, radius, _throwDirection, out RaycastHit hit, stepDistance))
            {
                Collider other = hit.collider;
                if (other.gameObject == _caster.Target)
                {
                    Entity target = other.GetComponent<Entity>();
                    _hasHitOutward = true;
                    HandleHit(target);
                }
            }
        }

        transform.Translate(_throwDirection * stepDistance, Space.World);

        float distanceTravelled = Vector3.Distance(_startPosition, transform.position);

        if (distanceTravelled >= _maxDistance)
        {
            _state = ProjectileState.PauseAtApex;
            _pauseTimer = 0f;
        }
    }

    private void HandlePauseAtApex()
    {
        _pauseTimer += Time.deltaTime;
        if (_pauseTimer >= _pauseDuration)
        {
            SwitchToReturnState();
        }
    }

    private void HandleReturnMovement()
    {
        float stepDistance = _moveSpeed * Time.deltaTime;

        if (!_hasHitReturn && _caster != null && _caster.Target != null)
        {
            Vector3 origin = transform.position;
            Vector3 returnDirection = (_startPosition - transform.position).normalized;

            float radius = 0.5f;
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            }

            if (Physics.SphereCast(origin, radius, returnDirection, out RaycastHit hit, stepDistance))
            {
                Collider other = hit.collider;
                if (other.gameObject == _caster.Target)
                {
                    Entity target = other.GetComponent<Entity>();
                    _hasHitReturn = true;
                    HandleHit(target);
                }
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, _startPosition, stepDistance);

        if (Vector3.Distance(transform.position, _startPosition) < 0.1f)
        {
            _skillHandler.OnProjectileReturned(this.gameObject);
        }
    }

    private void SwitchToReturnState()
    {
        _state = ProjectileState.Returning;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_caster == null || _caster.Target == null || other.gameObject != _caster.Target) return;

        Entity target = other.GetComponent<Entity>();

        if (_state == ProjectileState.Outward && !_hasHitOutward)
        {
            _hasHitOutward = true;
            HandleHit(target);
        }
        else if (_state == ProjectileState.Returning && !_hasHitReturn)
        {
            _hasHitReturn = true;
            HandleHit(target);
        }
    }

    private void HandleHit(Entity target)
    {
        if (_audioTrigerrDetect != null && _caster != null)
        {
            _caster.PlaySFX(_audioTrigerrDetect.AudioID);
        }
        _skillHandler.OnProjectileHit(target, this.gameObject);
    }
}