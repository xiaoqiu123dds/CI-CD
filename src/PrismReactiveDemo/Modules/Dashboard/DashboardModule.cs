using Prism.Ioc;
using Prism.Modularity;
using PrismReactiveDemo.Core.Constants;
using PrismReactiveDemo.Modules.Dashboard.Views;

namespace PrismReactiveDemo.Modules.Dashboard;

/// <summary>
/// Dashboard 模块。
/// Dashboard module.
/// </summary>
public sealed class DashboardModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<DashboardView>(NavigationTargets.Dashboard);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
