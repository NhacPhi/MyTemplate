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

    private int maxHP = 9800;
    private int currentHP;
    private int currentDamage;
    private int currentShield;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
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
        currentHP = currentDamage = currentShield = maxHP;

        heathSlider.maxValue = maxHP;
        heathSlider.value = currentHP;

        damageSlider.maxValue = maxHP;
        damageSlider.value = currentDamage;

        shieldSlider.maxValue = maxHP;
        shieldSlider.value = currentShield;



        ResetTick(maxHP);
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
        int finalDamage = 0;
        if (HasShield())
        {
            finalDamage = damage - (currentShield - currentHP);

            if (finalDamage > 0)
            {
                // Shield k du lon
                currentHP -= finalDamage;
                currentShield = currentHP;

                heathSlider.value = currentHP;
                shieldSlider.value = currentShield;

                StartCoroutine(EffectTakeDamage(finalDamage));
            }
            else
            {
                if (OverShield())
                {
                    currentShield -= damage;
                    shieldSlider.value = maxHP;
                    float offset = ((float)currentHP / (float)currentShield) * maxHP;
                    Debug.Log("LOL:" + offset);
                    damageSlider.value = offset;
                    heathSlider.value = offset;

                    ResetTick(currentShield);
                }
                else
                {
                    currentShield -= damage;
                    shieldSlider.value = currentShield;
                }
            }
        }
        else
        {
            currentHP -= damage;
            heathSlider.value = currentHP;

            currentShield = currentHP;
            shieldSlider.value = currentShield;

            StartCoroutine(EffectTakeDamage(damage));
        }

    }
    
    public void Heal(int value)
    {
        StartCoroutine(EffectHeal(value));
    }

    public void BuffShield(int value)
    {
        currentShield += value;
        if(OverShield())
        {
            shieldSlider.value = maxHP;
            float offset = ((float)currentHP / (float)currentShield) * maxHP;
            Debug.Log("LOL:" + offset);
            damageSlider.value = offset;
            heathSlider.value = offset;

            ResetTick(currentShield);
        }
        else
        {
            shieldSlider.value = currentShield;
        }
    }

    IEnumerator EffectHeal(int value)
    {
        float hp = currentHP;

        while(hp < currentHP + value)
        {
            hp += 50;
            shieldSlider.value = currentShield + hp - currentHP;
            heathSlider.value = hp;
            yield return new WaitForSeconds(0.01f);
        }

        if(HasShield())
        {
            currentShield = currentShield + value;
        }
        else
        {
            currentShield = currentHP +  value;
        }
        currentHP = currentHP + value;

        currentDamage = currentHP;

        heathSlider.value = currentHP;
        damageSlider.value = currentDamage;
        shieldSlider.value = currentShield;
    }

    IEnumerator EffectTakeDamage(int damage)
    {
        float effect = currentDamage; 

        while(effect >= currentDamage - damage)
        {
            damageSlider.value = effect;
            yield return new WaitForSeconds(0.01f);
            effect -= 50;
            effect -= 50;
        }

        currentDamage = currentHP;
        damageSlider.value = currentDamage;
    }

    private bool HasShield()
    {
        return currentShield > currentHP;
    }

    private bool OverShield()
    {
        return currentShield > maxHP;
    }
}
