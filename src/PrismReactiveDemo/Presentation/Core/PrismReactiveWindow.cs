using System.Reactive.Disposables;
using System.Windows;
using ReactiveUI;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// Prism 与 ReactiveUI 共用的窗口基类。
/// Shared window base class for Prism and ReactiveUI.
/// </summary>
public abstract class PrismReactiveWindow<TViewModel> : ReactiveWindow<TViewModel>
    where TViewModel : class
{
    protected PrismReactiveWindow()
    {
        this.WhenActivated(disposables =>
        {
            SetupBindings(disposables);
            SetupInteractions(disposables);
        });

        // 将 Prism 注入的 DataContext 同步给 ReactiveUI 的强类型 ViewModel 属性，激活各类 this.Bind。
        this.DataContextChanged += (s, e) =>
        {
            if (e.NewValue is TViewModel vm)
            {
                ViewModel = vm;
            }
        };
    }

    protected virtual void SetupBindings(CompositeDisposable disposables) { }

    protected virtual void SetupInteractions(CompositeDisposable disposables) { }
}
