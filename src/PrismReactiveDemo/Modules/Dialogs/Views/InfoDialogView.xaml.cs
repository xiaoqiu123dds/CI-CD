using ReactiveUI;
using System.Reactive.Disposables;
using PrismReactiveDemo.Modules.Dialogs.ViewModels;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Modules.Dialogs.Views;

/// <summary>
/// 信息对话框视图。
/// Informational dialog view.
/// </summary>
public partial class InfoDialogView : PrismReactiveView<InfoDialogViewModel>
{
    public InfoDialogView()
    {
        InitializeComponent();
    }

    protected override void SetupBindings(CompositeDisposable disposables)
    {
        this.BindCommand(ViewModel, vm => vm.CloseCommand, view => view.CloseButton)
            .DisposeWith(disposables);
    }
}
