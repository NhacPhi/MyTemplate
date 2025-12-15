using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject[] ticks; 
    [SerializeField] private Slider heathSlider;
    [SerializeField] private Slider damageSlider;
    [SerializeField] private Slider shieldSlider;

    [SerializeField] private GameObject ticketContainer;

    private int MAX_HP = 9800;
    private int currentHP;
    private int currentDamage;
    private int currentShield;

    private int shield = 0;

    private int healh = 0;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            TakeDamage(2000);
            UIEvent.DamagePopup?.Invoke(2000, gameObject.transform.position, false);
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            Heal(2000);
            UIEvent.HealPopup?.Invoke(2000, gameObject.transform.position);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            BuffShield(1000);
        }
    }
    public void Setup()
    {
        currentHP = currentDamage = currentShield = MAX_HP;

        heathSlider.maxValue = MAX_HP;
        heathSlider.value = currentHP;

        damageSlider.maxValue = MAX_HP;
        damageSlider.value = currentDamage;

        shieldSlider.maxValue = MAX_HP;

        shieldSlider.value = currentShield;



        ResetTick(MAX_HP);
    }

    private void ResetTick(int hp)
    {
        int tickCount = hp / 3000;

        foreach (var tick in ticks)
        {
            tick.gameObject.SetActive(false);
        }


        float offset = (ticketContainer.GetComponent<RectTransform>().rect.width * 3000) / hp;
        float offsetTick = offset;
        for (int i = 0; i < tickCount; i++)
        {
            ticks[i].gameObject.SetActive(true);
            ticks[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetTick, 0);
            offsetTick += offset;
        }
    }

    public void TakeDamage(int damage)
    {
        // Has Shield
        if(HasShield())
        {
            var result = shield - damage;
            // Compare shield with damage
            if (shield >= damage)
            {
                shield = result;
                SetCurrentShield(currentShield - damage);
            }
            else
            {
                shield = 0;

                var newDamge = damage + result;
                StartCoroutine(EffectTakeDamage(damage));

                SetCurrentHP(currentHP - newDamge);
                SetCurrentShield(currentHP);
            }
        }
        else
        {
            // No Shield
            StartCoroutine(EffectTakeDamage(damage));
            // Set value
            SetCurrentHP(currentHP - damage);
            SetCurrentShield(currentHP);
        }

        CheckShield();
    }
    
    public void Heal(int value)
    {
        if(currentHP < MAX_HP)
        {
            StartCoroutine(EffectHeal(value));
        }
    }

    public void BuffShield(int value)
    {
        shield += value;

        SetCurrentShield(currentShield + value);

        CheckShield();
    }

    IEnumerator EffectHeal(int value)
    {
        if(CheckShieldOver())
        {
            float hp = healh;

            float ratio = value / (MAX_HP + shield);

            float result = ratio * MAX_HP;

            while (hp < healh + result)
            {
                heathSlider.value = hp;
                damageSlider.value = hp;
                hp += 50 / (MAX_HP + shield);
                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            float hp = currentHP;

            while (hp < currentHP + value)
            {
                heathSlider.value = hp;
                damageSlider.value = hp;
                hp += 50;
                yield return new WaitForSeconds(0.01f);
            }

        }

        //Set Value 
        SetCurrentHP(currentHP + value);
        SetCurrentShield(currentShield + value);

        CheckShield();
    }

    IEnumerator EffectTakeDamage(int damage)
    {
        float effect = currentDamage; 

        while(effect >= currentDamage - damage)
        {
            damageSlider.value = effect;
            yield return new WaitForSeconds(0.01f);
            effect -= 50;
        }

    }

    bool HasShield()
    {
        return currentShield > currentHP;
    }

    private bool CheckShieldOver()
    {
        return currentShield > MAX_HP;
    }

    private void SetCurrentHP(int hp)
    {
        if(hp > MAX_HP)
            hp = MAX_HP;
        currentHP = hp;
        currentDamage = currentHP;

        heathSlider.value = currentDamage;
        damageSlider.value = currentDamage;
    }

    private void SetCurrentShield(int value)
    {
        currentShield = value;
        shieldSlider.value = value;
    }

    private void CheckShield()
    {
        if (CheckShieldOver())
        {
            var sum = currentHP + shield;
            ResetTick(sum);
            float ratio = (float)shield / sum;
            var result = MAX_HP - (float)ratio * MAX_HP;
            heathSlider.value = result;
            damageSlider.value = result;
            healh = (int)result;
            shieldSlider.value = MAX_HP;
        }
        else
        {
            ResetTick(MAX_HP);
        }
    }
}
