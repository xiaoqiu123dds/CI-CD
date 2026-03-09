using ReactiveUI;
using System.Reactive.Disposables;
using PrismReactiveDemo.Modules.Dashboard.ViewModels;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Modules.Dashboard.Views;

/// <summary>
/// Dashboard 页面。
/// Dashboard page.
/// </summary>
public partial class DashboardView : PrismReactiveView<DashboardViewModel>
{
    public DashboardView()
    {
        InitializeComponent();
    }

    protected override void SetupBindings(CompositeDisposable disposables)
    {
        this.BindCommand(ViewModel, vm => vm.RefreshCommand, view => view.RefreshButton)
            .DisposeWith(disposables);

        this.BindCommand(ViewModel, vm => vm.ShowInfoDialogCommand, view => view.InfoDialogButton)
            .DisposeWith(disposables);
    }
}
