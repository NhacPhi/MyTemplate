using System;
using System.Reflection;
using UnityEngine;

namespace Observer
{
    public static class GameEvent
    {
        public static Action<GameSceneSO,bool, bool> OnLoadColdStartupLocation;
        public static Action<GameSceneSO, bool, bool> OnLoadSceneLocation;
    }

}
