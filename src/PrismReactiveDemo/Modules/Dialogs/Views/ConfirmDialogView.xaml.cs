using ReactiveUI;
using System.Reactive.Disposables;
using PrismReactiveDemo.Modules.Dialogs.ViewModels;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Modules.Dialogs.Views;

/// <summary>
/// 确认对话框视图。
/// Confirmation dialog view.
/// </summary>
public partial class ConfirmDialogView : PrismReactiveView<ConfirmDialogViewModel>
{
    public ConfirmDialogView()
    {
        InitializeComponent();
    }

    protected override void SetupBindings(CompositeDisposable disposables)
    {
        this.BindCommand(ViewModel, vm => vm.ConfirmCommand, view => view.ConfirmButton)
            .DisposeWith(disposables);

        this.BindCommand(ViewModel, vm => vm.CancelCommand, view => view.CancelButton)
            .DisposeWith(disposables);
    }
}
