using System.Reactive;
using Prism.Dialogs;
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PrismReactiveDemo.Modules.Dialogs.ViewModels;

/// <summary>
/// 确认导航对话框 ViewModel。
/// ViewModel for the navigation confirmation dialog.
/// </summary>
public sealed class ConfirmDialogViewModel : ReactiveDialogViewModelBase
{
    public ConfirmDialogViewModel()
    {
        ConfirmCommand = ReactiveCommand.Create(() => RequestClose.Invoke(ButtonResult.OK));
        CancelCommand = ReactiveCommand.Create(() => RequestClose.Invoke(ButtonResult.Cancel));
    }

    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    [Reactive]
    public string Message { get; set; } = string.Empty;

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        Title = parameters.GetValue<string>("Title") ?? "确认";
        Message = parameters.GetValue<string>("Message") ?? "确定继续吗？";
    }
}
