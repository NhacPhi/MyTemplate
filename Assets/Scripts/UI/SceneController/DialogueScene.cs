using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using TMPro;

public class DialogueScene : WindowController
{
    [SerializeField] private TextMeshProUGUI content;

    [SerializeField] private List<DialogueChoiceUI> choices;

    // Start is called before the first frame update
    void Start()
    {
        
    }


}
