using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCard : MonoBehaviour
{
    [SerializeField] private CharacterTap type;
    public CharacterTap Type => type;
}
