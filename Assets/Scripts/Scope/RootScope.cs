using Core.Scope;
using System.Collections;
using System.Collections.Generic;
using Tech.Pool;
using UIFramework;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootScope : LifetimeScope
{
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private AutoSaveManager _autoSave;

    [SerializeField] private List<AudioDatabase> _activeAudioDatabases;

    [SerializeField] UISettings uiSetings;

    protected override void Configure(IContainerBuilder builder)
    {
        // Data Service
        _audioManager.Init(_activeAudioDatabases);

        //builder.Register<EventManager>(Lifetime.Singleton);
        builder.Register<SaveSystem>(Lifetime.Singleton);
        builder.Register<CurrencyManager>(Lifetime.Singleton);
        builder.Register<AtlasProvider>(Lifetime.Singleton);
        builder.Register<GameDataBase>(Lifetime.Singleton);
        builder.Register<BattleSessionContext>(Lifetime.Singleton);
        
        builder.Register<InventoryManager>(Lifetime.Singleton);
        builder.Register<ForgeManager>(Lifetime.Singleton);
        builder.Register<PlayerCharacterManager>(Lifetime.Singleton);

        builder.RegisterComponent(_audioManager).AsImplementedInterfaces().AsSelf();
        //builder.Register<GameNarrativeData>(Lifetime.Singleton);

        builder.RegisterComponent(uiSetings);

        // Hireachy
        builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
        builder.RegisterComponentInHierarchy<SceneLoader>().AsSelf();
        builder.RegisterComponentInHierarchy<PoolManager>().AsSelf();
        builder.RegisterComponentInHierarchy<AutoSaveManager>().AsSelf();

        //Entry point
        builder.RegisterEntryPoint<RootPreLoad>(Lifetime.Singleton).As<IPreload>();

    }
}
