using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using Prism.Dialogs;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Modules.Dialogs.ViewModels;

/// <summary>
/// 信息对话框 ViewModel。
/// ViewModel for the informational dialog.
/// </summary>
public sealed class InfoDialogViewModel : ReactiveDialogViewModelBase
{
    public InfoDialogViewModel()
    {
        CloseCommand = ReactiveCommand.Create(() => RequestClose.Invoke(ButtonResult.OK));
    }

    public override string Title => DialogTitle;

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    [Reactive]
    public string DialogTitle { get; set; } = "信息";

    [Reactive]
    public string Message { get; set; } = string.Empty;

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        DialogTitle = parameters.GetValue<string>("Title") ?? "信息";
        Message = parameters.GetValue<string>("Message") ?? "默认信息。";
    }
}
