using VContainer;
using VContainer.Unity;

public class OceanScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<StepController>().AsSelf();
    }
}
