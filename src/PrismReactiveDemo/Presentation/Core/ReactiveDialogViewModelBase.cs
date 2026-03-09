using System.Reactive.Disposables;
using Prism.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// 对话框 ViewModel 基类。
/// Base ViewModel for dialogs.
/// </summary>
public abstract class ReactiveDialogViewModelBase
    : ReactiveObject,
        IDialogAware,
        IActivatableViewModel
{
    protected CompositeDisposable LifetimeDisposables { get; } = new();

    public ViewModelActivator Activator { get; } = new();

    public DialogCloseListener RequestClose { get; } = new();

    // [Reactive] 让 Fody 织入 PropertyChanged 通知，使 Prism 对话框标题栏在 OnDialogOpened 赋值后能实时刷新。
    // [Reactive] lets Fody inject PropertyChanged so Prism dialog title bar updates when assigned in OnDialogOpened.
    [Reactive]
    public virtual string Title { get; protected set; } = string.Empty;

    protected ReactiveDialogViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            OnActivated(disposables);
        });
    }

    protected virtual void OnActivated(CompositeDisposable disposables) { }

    public virtual bool CanCloseDialog() => true;

    public virtual void OnDialogOpened(IDialogParameters parameters) { }

    public virtual void OnDialogClosed()
    {
        LifetimeDisposables.Dispose();
    }
}
