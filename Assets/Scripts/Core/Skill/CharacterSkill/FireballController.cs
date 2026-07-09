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

        transform.Translate(_flyDirection * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_caster == null || _hasHit) return;

        if (other.gameObject == _caster.gameObject) return;

        Entity target = other.GetComponent<Entity>();

        // Check if the collided object is an entity and belongs to the opposing team
        if (target != null && target.Team != _caster.Team)
        {
            _hasHit = true;

            if (_skillHandler != null)
            {
                if (_audioTrigerrDetect != null)
                {
                    _caster.PlaySFX(_audioTrigerrDetect.AudioID);
                }
                _skillHandler.OnProjectileImpact(target, transform.position);
                gameObject.SetActive(false);
            }

            if (target != null)
            {
                explosionObj.transform.position = other.transform.position;
                explosionObj.gameObject.SetActive(true);
            }
        }


    }
}
