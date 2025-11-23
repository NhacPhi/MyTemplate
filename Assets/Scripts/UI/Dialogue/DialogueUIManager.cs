using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Image avatar;
    // Event show Choices
    // Start is called before the first frame update
    private void Awake()
    {
        GameEvent.OnOpenDialogue += SetDialogue;
    }

    private void OnDestroy()
    {
        GameEvent.OnOpenDialogue -= SetDialogue;
    }
    void Start()
    {
        
    }

    public void SetDialogue(string str, ActorData actor)
    {
        content.text = str;
        avatar.sprite = actor.Image;
    }
}
