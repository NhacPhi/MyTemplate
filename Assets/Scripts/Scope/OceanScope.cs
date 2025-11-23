using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class OceanScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<StepController>().AsSelf();
    }
}
