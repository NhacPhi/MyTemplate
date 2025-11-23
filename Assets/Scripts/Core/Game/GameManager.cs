using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class GameManager : MonoBehaviour
{
    // QuestManager
    // GameState
    //[Inject] GameStateManager gameState;
    //InventoryMM
    [Inject] GameNarrativeData gameNarrative;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartGame()
    {

    }
}
