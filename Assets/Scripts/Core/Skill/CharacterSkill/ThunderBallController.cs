using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderBallController : MonoBehaviour
{
    //Runtion variables
    private Entity _caster;
    private IImpactSkill _skillHandler;
    private Vector3 _flyDirection;
    [SerializeField] private float _speed;
    [SerializeField] private Sprite image;
    private bool _hasHit = false;
    private GameObject explosionObj;
    private Animator anim;

    private SpriteRenderer originSprite;
    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        originSprite = gameObject.GetComponent<SpriteRenderer>();
    }
    private void OnDisable()
    {
        originSprite.sprite = image;
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
        if (_hasHit) return;

        transform.Translate(_flyDirection * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        if (other.gameObject == _caster.gameObject) return;

        //if (other.isTrigger) return;
        if(other.gameObject == _caster.target)
        {
            _hasHit = true;

            Entity target = other.GetComponent<Entity>();

            if (_skillHandler != null)
            {
                _skillHandler.OnProjectileImpact(target, transform.position);
                anim.SetTrigger("Detected");
            }
        }

    }

    public void DeActiveObject()
    {
        gameObject.SetActive(false);
    }
}
