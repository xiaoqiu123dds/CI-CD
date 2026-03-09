using Prism.Ioc;
using Prism.Modularity;
using PrismReactiveDemo.Core.Constants;
using PrismReactiveDemo.Modules.Editor.Views;

namespace PrismReactiveDemo.Modules.Editor;

/// <summary>
/// Editor 模块。
/// Editor module.
/// </summary>
public sealed class EditorModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<EditorView>(NavigationTargets.Editor);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
