using System;
using UnityEngine;


public static class GameEvent
{
    public static Action<GameSceneSO,bool, bool> OnLoadColdStartupLocation;
    public static Action<GameSceneSO, bool, bool> OnLoadSceneLocation;

    public static Action<Vector2> OnPlayerMove;
    public static Action<Vector2> OnCameraMove;

    public static Action OnPlayerAttack;
    public static Action OnPlayerTransform;
}
