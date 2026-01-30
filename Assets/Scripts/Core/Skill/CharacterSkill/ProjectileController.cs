using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileState
{
    Outward,
    Returning
}
public class ProjectileController : MonoBehaviour
{
    private ProjectileState _state;
    private Entity _caster;
    private IReturningProjectileSkill _skillHandler;

    private Vector3 _startPosition;
    private Vector3 _throwDirection;
    public float _moveSpeed;
    private float _maxDistance;

    public void Initialize(Entity caster, IReturningProjectileSkill skill, Vector3 direction, float maxDist)
    {
        _caster = caster;
        _skillHandler = skill;

        _throwDirection = direction.normalized;
        _maxDistance = maxDist;

        _startPosition = transform.position;
        _state = ProjectileState.Outward;
    }

    private void Update()
    {
        switch(_state)
        {
            case ProjectileState.Outward:
                HandleOutwardMovement();
                break;
            case ProjectileState.Returning:
                HandleReturnMovement();
                break;
        }
    }

    private void HandleOutwardMovement()
    {
        transform.Translate(_throwDirection * _moveSpeed * Time.deltaTime, Space.World);

        float distanceTravlled = Vector3.Distance(_startPosition, transform.position);

        if(distanceTravlled >= _maxDistance)
        {
            SwitchToReturnState();
        }
    }

    private void HandleReturnMovement()
    {
        transform.position = Vector3.MoveTowards(transform.position, _startPosition, _moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _startPosition) < 0.1f)
        {
            _skillHandler.OnProjectileReturned(this.gameObject);
        }
    }

    private void SwitchToReturnState()
    {
        _state = ProjectileState.Returning;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;    }

    private void OnTriggerEnter(Collider other)
    {
        Entity target = other.GetComponent<Entity>();

        if(target != null && target != _caster)
        {
            _skillHandler.OnProjectileHit(target, this.gameObject);
        }
    }
}