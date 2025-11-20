using UnityEngine;
using UnityEngine.AddressableAssets;
public class GameSceneSO : DescriptionBaseSO
{
    public GameSceneType sceneType;
    public AssetReference sceneReference; //use load the scene from Asset Bundle
}

public enum GameSceneType
{
    //Special Scene
    Initialization,
    PersistenManager,
    GamePlay,

    //Menu
    Menu,

    //Location
    Location
}
