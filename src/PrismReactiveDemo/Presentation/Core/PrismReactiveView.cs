using System.Reactive.Disposables;
using ReactiveUI;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// Prism 与 ReactiveUI 共用的用户控件基类。
/// Shared user control base class for Prism and ReactiveUI.
/// </summary>
public abstract class PrismReactiveView<TViewModel> : ReactiveUserControl<TViewModel>
    where TViewModel : class
{
    protected PrismReactiveView()
    {
        // DataContext → ViewModel 同步已由 ReactiveUserControl<TViewModel> 基类处理，无需重复。
        // DataContext → ViewModel sync is handled by ReactiveUserControl<TViewModel> base class.
        this.WhenActivated(disposables =>
        {
            SetupBindings(disposables);
            SetupInteractions(disposables);
        });

        // Prism 会自动注入 DataContext，但 ReactiveUI 强类型的 this.Bind 需要 ViewModel 属性有值。
        // 手动将 DataContext 同步给 ViewModel 属性。
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
