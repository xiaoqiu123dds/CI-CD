using System.Reactive;
using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using PrismReactiveDemo.Core.Constants;
using PrismReactiveDemo.Core.Interfaces;
using PrismReactiveDemo.Infrastructure.Diagnostics;
using PrismReactiveDemo.Modules.Dashboard;
using PrismReactiveDemo.Modules.Dialogs.ViewModels;
using PrismReactiveDemo.Modules.Dialogs.Views;
using PrismReactiveDemo.Modules.Editor;
using PrismReactiveDemo.Modules.Shell.Views;
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;

namespace PrismReactiveDemo;

/// <summary>
/// 示例应用入口。
/// Example application entry point.
/// </summary>
public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<ShellWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IRxExceptionDispatcher, RxExceptionDispatcher>();
        containerRegistry.RegisterDialog<ConfirmDialogView, ConfirmDialogViewModel>(
            DialogNames.Confirm
        );
        containerRegistry.RegisterDialog<InfoDialogView, InfoDialogViewModel>(DialogNames.Info);
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<DashboardModule>();
        moduleCatalog.AddModule<EditorModule>();
    }

    protected override void ConfigureViewModelLocator()
    {
        base.ConfigureViewModelLocator();
        ViewModelLocationProvider.Register<ShellWindow, Modules.Shell.ViewModels.ShellViewModel>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var exceptionDispatcher = Container.Resolve<IRxExceptionDispatcher>();
        GlobalExceptionHandler.Initialize(
            Current,
            (source, exception) => exceptionDispatcher.Dispatch(source, exception)
        );
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(exception =>
        {
            exceptionDispatcher.Dispatch(nameof(RxApp.DefaultExceptionHandler), exception);
        });

        var regionManager = Container.Resolve<Prism.Navigation.Regions.IRegionManager>();
        regionManager.RequestNavigate(RegionNames.MainRegion, NavigationTargets.Dashboard);
    }
}
