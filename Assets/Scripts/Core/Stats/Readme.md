<h1><div align="center"> Entities Status System With Attribute And Stat And Effect </div></h1>

Attribute Is Value Like Hp, Exp, Mana, Stamina,... Dynamic values that frequently change during gameplay.

Stat Is Value Like MaxHp, MaxStamina, ATK, DEF, CritDmg, CritRate,... Fixed or calculated values that determine limits or effects in the game.

Effect Is Buff Or DeBuff Like Poison, ATK Increase In 1 Second, ...

To Add Stat Or Attribute Modify StatType Or AttributeType enum

Attribute Max Value Need Linking To Stat Mean Stat Is Max Value Of Attribute

See Assets/Data/EntitiesStat.json For Example
Each Of Entities Has ID To Define Stats

![](StatsEX1.png)

Search ID Button Will Search All Entities Stats In Assets/Data/EntitiesStat.json For Faster Input
All Attribute And Stat Entity Has Will Show

Change EntitiesStat.json Path In StatsControllerEditor

![](StatsEX2.png)

Runtime Debugging On PlayMode

![](StatsEX3.png)

Change Value Of Attribute

```
GetAttribute(AttributeType.Hp).Value += 1f;
GetAttribute(AttributeType.Hp).SetValueWithoutNotify(newValue);
```

SetValueWithoutNotify Will Not Call Callback Change

Change Value Of Stat 

```
AddModifier(StatType.MaxHp, new Modifer(1f, ModifyType.Constant));
```

Formula Of ModifyType

```
float baseConstant = 0f;
float constant = 0f;
float percent = 0f;

foreach (Modifier modifier in _statModifiers)
{
    switch (modifier.Type)
    {
        case ModifyType.BaseConstant:
            baseConstant += modifier.Value; continue;
        case ModifyType.Constant:
            constant += modifier.Value; continue;
        case ModifyType.Percent:
            percent += modifier.Value; continue;
    };
}

float _finalValue = (BaseValue + baseConstant) * (1f + percent) + constant;

_value = (float)Math.Round(_finalValue, 4);

```

<h3><div align="center"> FinalValue = (BaseValue + BaseConstant) × (1 + Percent) + Constant </div></h3>

AddModifierWithoutNotify(StatType.MaxHp, new Modifer(1f, ModifyType.Constant) Similar Attribute

Renew To Reset All Stat, Attribute, Effect 
Example: Use OnEnemySpawnFrom Pool 

```
OnEnable()
{
    Renew();
}
```

Apply Effect On Entity Can Stackable
```
public void ApplyEffect(YourStatusEffect effect)
```
#KatTheDev